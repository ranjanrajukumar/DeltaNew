using System.Text.Json.Nodes;

namespace SchoolERP.Application.Common.Interfaces;

/// <summary>Browser push notifications (the Node app's push/push.service.js).</summary>
public interface IPushService
{
    /// <summary>The VAPID public key browsers subscribe with.</summary>
    Task<string> PublicKeyAsync(CancellationToken ct = default);

    /// <summary>Registers (or takes over) a browser endpoint for the signed-in user.</summary>
    Task SubscribeAsync(int userId, JsonObject? subscription, string? userAgent, CancellationToken ct = default);

    Task UnsubscribeAsync(int userId, string? endpoint, CancellationToken ct = default);

    Task<int> MySubscriptionsAsync(int userId, CancellationToken ct = default);

    /// <summary>Sends to specific people (by user id). Never throws: a notification must not stop what triggered it.</summary>
    Task NotifyUsersAsync(IEnumerable<int?> userIds, string title, string body, string? url, CancellationToken ct = default);

    /// <summary>{ people, devices } for an audience: staff, parents, students or everyone.</summary>
    Task<object> AudienceSizeAsync(string? audience, CancellationToken ct = default);

    /// <summary>Sends to every registered device of an audience; returns { devices, delivered, failed, removed }.</summary>
    Task<object> SendAsync(JsonObject? body, CancellationToken ct = default);
}
