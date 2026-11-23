using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using WebPushSubscription = Lib.Net.Http.WebPush.PushSubscription;
using SubscriptionRow = SchoolERP.Domain.Entities.PushSubscription;

namespace SchoolERP.Infrastructure.Notifications;

/// <summary>
/// Browser push (the Node app's push/push.service.js). Push needs a VAPID key pair that identifies this server: set
/// Vapid:PublicKey / Vapid:PrivateKey (or VAPID_PUBLIC_KEY / VAPID_PRIVATE_KEY) to choose your own; otherwise a pair is generated
/// once and kept in the settings table under push.vapid, the same row and format the Node app used, so its browser
/// subscriptions keep working.
/// </summary>
public sealed partial class PushService(
    IPushSubscriptionRepository subscriptions, IUserRepository users, ISettingRepository settings, IUnitOfWork unitOfWork,
    IConfiguration configuration, IHttpClientFactory httpClients, ILogger<PushService> logger) : IPushService
{
    private const string Key = "push.vapid";
    private const int MaxTitle = 80;
    private const int MaxBody = 300;
    private const int Batch = 10;

    private sealed record VapidKeys(string PublicKey, string PrivateKey);

    private static VapidKeys? _keys;
    private static readonly SemaphoreSlim KeyLock = new(1, 1);

    private async Task<VapidKeys> VapidAsync(CancellationToken ct)
    {
        if (_keys is { } known) return known;
        await KeyLock.WaitAsync(ct);
        try
        {
            if (_keys is { } again) return again;
            var pub = configuration["Vapid:PublicKey"] ?? configuration["VAPID_PUBLIC_KEY"];
            var priv = configuration["Vapid:PrivateKey"] ?? configuration["VAPID_PRIVATE_KEY"];
            if (!string.IsNullOrEmpty(pub) && !string.IsNullOrEmpty(priv)) return _keys = new VapidKeys(pub, priv);

            var saved = await settings.GetByKeyAsync(Key, ct);
            if (saved is not null && JsonNode.Parse(saved.Value) is JsonObject o
                && Body.AsString(o["publicKey"]) is { } p && Body.AsString(o["privateKey"]) is { } k)
                return _keys = new VapidKeys(p, k);

            var generated = Generate();
            settings.Add(new Setting
            {
                Key = Key,
                Value = JsonSerializer.Serialize(new { publicKey = generated.PublicKey, privateKey = generated.PrivateKey }),
                UpdatedByName = "system",
            });
            await unitOfWork.SaveChangesAsync(ct);
            return _keys = generated;
        }
        finally
        {
            KeyLock.Release();
        }
    }

    /// <summary>A P-256 key pair as web-push writes it: the uncompressed public point and the private scalar, base64url without padding.</summary>
    private static VapidKeys Generate()
    {
        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = ec.ExportParameters(includePrivateParameters: true);
        var point = new byte[65];
        point[0] = 0x04;
        p.Q.X!.CopyTo(point, 1);
        p.Q.Y!.CopyTo(point, 33);
        return new VapidKeys(Base64Url(point), Base64Url(p.D!));
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private async Task<PushServiceClient> ClientAsync(CancellationToken ct)
    {
        var keys = await VapidAsync(ct);
        var http = httpClients.CreateClient(nameof(PushService));
        http.Timeout = TimeSpan.FromSeconds(10);
        return new PushServiceClient(http)
        {
            DefaultAuthentication = new VapidAuthentication(keys.PublicKey, keys.PrivateKey)
            {
                Subject = configuration["Vapid:Subject"] ?? configuration["VAPID_SUBJECT"] ?? "mailto:admin@school.local",
            },
            DefaultTimeToLive = 86400,
        };
    }

    public async Task<string> PublicKeyAsync(CancellationToken ct = default) => (await VapidAsync(ct)).PublicKey;

    public async Task SubscribeAsync(int userId, JsonObject? subscription, string? userAgent, CancellationToken ct = default)
    {
        var endpoint = Body.AsString(subscription?["endpoint"]) ?? "";
        if (!HttpsUrl().IsMatch(endpoint) || endpoint.Length > 500) throw HttpException.BadRequest("Invalid subscription");
        var keys = subscription?["keys"] as JsonObject;
        var p256dh = Body.AsString(keys?["p256dh"]);
        var auth = Body.AsString(keys?["auth"]);
        if (string.IsNullOrEmpty(p256dh) || string.IsNullOrEmpty(auth)) throw HttpException.BadRequest("Invalid subscription");
        // one endpoint belongs to one person: a shared computer signing in as someone else takes the endpoint over
        var row = await subscriptions.FindByEndpointAsync(endpoint, ct);
        if (row is null)
        {
            row = new SubscriptionRow { Endpoint = endpoint };
            subscriptions.Add(row);
        }
        row.UserId = userId;
        row.P256dh = p256dh;
        row.Auth = auth;
        var agent = (userAgent ?? "").Length > 200 ? userAgent![..200] : userAgent;
        row.UserAgent = string.IsNullOrEmpty(agent) ? null : agent;
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UnsubscribeAsync(int userId, string? endpoint, CancellationToken ct = default)
    {
        var rows = await subscriptions.ListOfUserAsync(userId, endpoint ?? "", ct);
        subscriptions.RemoveRange(rows);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public Task<int> MySubscriptionsAsync(int userId, CancellationToken ct = default) =>
        subscriptions.CountOfUserAsync(userId, ct);

    /// <summary>Delivers one message; true when delivered, false when it failed, null when the browser says the endpoint is gone.</summary>
    private static async Task<bool?> DeliverAsync(PushServiceClient client, SubscriptionRow s, string payload, CancellationToken ct)
    {
        var target = new WebPushSubscription { Endpoint = s.Endpoint };
        target.SetKey(PushEncryptionKeyName.P256DH, s.P256dh);
        target.SetKey(PushEncryptionKeyName.Auth, s.Auth);
        try
        {
            await client.RequestPushMessageDeliveryAsync(target, new PushMessage(payload) { TimeToLive = 86400 }, ct);
            return true;
        }
        catch (PushServiceClientException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
        {
            return null;
        }
        catch (Exception ex) when (ex is PushServiceClientException or HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    public async Task NotifyUsersAsync(IEnumerable<int?> userIds, string title, string body, string? url, CancellationToken ct = default)
    {
        try
        {
            var ids = userIds.Where(id => id is not null and not 0).Select(id => id!.Value).Distinct().ToList();
            if (ids.Count == 0) return;
            var client = await ClientAsync(ct);
            var subs = await subscriptions.ListOfUsersAsync(ids, ct);
            var payload = JsonSerializer.Serialize(new { title, body, url = string.IsNullOrEmpty(url) ? "/" : url });
            var results = await Task.WhenAll(subs.Select(s => DeliverAsync(client, s, payload, ct)));
            var gone = subs.Where((_, i) => results[i] is null).ToList();
            if (gone.Count > 0)
            {
                subscriptions.RemoveRange(gone);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Notification failed: {Message}", ex.Message);
        }
    }

    /// <summary>The roles an audience covers; null for everyone.</summary>
    private static string[]? RolesOf(string? audience) => audience switch
    {
        "staff" => ["admin", "teacher", "accountant"],
        "parents" => ["parent"],
        "students" => ["student"],
        "everyone" => null,
        _ => throw HttpException.BadRequest("Choose who to notify"),
    };

    public async Task<object> AudienceSizeAsync(string? audience, CancellationToken ct = default)
    {
        var ids = await users.IdsWithRolesAsync(RolesOf(audience), ct);
        var devices = await subscriptions.CountOfUsersAsync(ids, ct);
        return new { people = ids.Count, devices };
    }

    public async Task<object> SendAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        var audience = b.Str("audience");
        var roles = RolesOf(audience); // validates the audience first
        var heading = b.Trimmed("title");
        var text = b.Trimmed("body");
        if (heading == "" || text == "") throw HttpException.BadRequest("A title and a message are required");
        if (heading.Length > MaxTitle) throw HttpException.BadRequest($"The title is limited to {MaxTitle} characters");
        if (text.Length > MaxBody) throw HttpException.BadRequest($"The message is limited to {MaxBody} characters");
        var url = b.Str("url");
        if (b.Truthy("url") && !url!.StartsWith('/')) throw HttpException.BadRequest("The link must be a page of this site, starting with /");
        var client = await ClientAsync(ct);

        var ids = await users.IdsWithRolesAsync(roles, ct);
        var subs = await subscriptions.ListOfUsersAsync(ids, ct);
        var payload = JsonSerializer.Serialize(new { title = heading, body = text, url = b.Truthy("url") ? url : "/" });
        int delivered = 0, failed = 0, removed = 0;
        foreach (var chunk in subs.Chunk(Batch))
        {
            var results = await Task.WhenAll(chunk.Select(s => DeliverAsync(client, s, payload, ct)));
            for (var i = 0; i < chunk.Length; i++)
            {
                switch (results[i])
                {
                    case true: delivered++; break;
                    case false: failed++; break;
                    default:
                        subscriptions.Remove(chunk[i]);
                        removed++;
                        break;
                }
            }
            await unitOfWork.SaveChangesAsync(ct);
        }
        return new { devices = subs.Count, delivered, failed, removed };
    }

    [GeneratedRegex("^https://")]
    private static partial Regex HttpsUrl();
}
