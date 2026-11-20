using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Notifications;

/// <summary>
/// Email over SMTP (MailKit) and SMS over Twilio, configured with the Node app's environment variables (they can also go in
/// appsettings under the same names): SMTP_HOST, SMTP_PORT, SMTP_USER, SMTP_PASSWORD, SMTP_FROM; SMS_PROVIDER=twilio,
/// TWILIO_SID, TWILIO_TOKEN, TWILIO_FROM or TWILIO_MESSAGING_SERVICE_SID; DEFAULT_COUNTRY_CODE (e.g. 91).
/// </summary>
public sealed partial class MessageDelivery(IConfiguration configuration, IHttpClientFactory httpClients) : IMessageDelivery
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private string? Cfg(string key) => configuration[key] is { Length: > 0 } v ? v : null;

    private bool SmtpConfigured => Cfg("SMTP_HOST") is not null;

    private string SmsProvider =>
        Cfg("SMS_PROVIDER") == "twilio" && Cfg("TWILIO_SID") is not null && Cfg("TWILIO_TOKEN") is not null
        && (Cfg("TWILIO_FROM") ?? Cfg("TWILIO_MESSAGING_SERVICE_SID")) is not null
            ? "twilio"
            : "log";

    public (string Email, string Sms) ProviderStatus() => (SmtpConfigured ? "smtp" : "log", SmsProvider);

    public string? NormalizePhone(string? raw)
    {
        var text = (raw ?? "").Trim();
        var digits = NotDigit().Replace(text, "");
        if (digits == "") return null;
        string e164;
        if (text.StartsWith('+')) e164 = digits;
        else if (text.StartsWith("00", StringComparison.Ordinal)) e164 = digits[2..];
        else
        {
            var country = NotDigit().Replace(Cfg("DEFAULT_COUNTRY_CODE") ?? "", "");
            if (country == "") return null;
            e164 = country + digits.TrimStart('0');
        }
        return e164.Length is >= 8 and <= 15 ? $"+{e164}" : null;
    }

    public Task<DeliveryResult> DeliverAsync(string channel, string to, string? subject, string body, CancellationToken ct = default) =>
        channel == "email" ? SendEmailAsync(to, subject, body, ct) : SendSmsAsync(to, body, ct);

    private static DeliveryResult Fail(string provider, string message) =>
        new("failed", provider, message.Length > 300 ? message[..300] : message);

    private async Task<DeliveryResult> SendEmailAsync(string to, string? subject, string body, CancellationToken ct)
    {
        if (!SmtpConfigured) return new DeliveryResult("logged", "log");
        try
        {
            var port = int.TryParse(Cfg("SMTP_PORT"), out var p) ? p : 587;
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(Cfg("SMTP_FROM") ?? Cfg("SMTP_USER") ?? ""));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = string.IsNullOrEmpty(subject) ? "(no subject)" : subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient { Timeout = (int)Timeout.TotalMilliseconds };
            await client.ConnectAsync(Cfg("SMTP_HOST")!, port, port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable, ct);
            if (Cfg("SMTP_USER") is { } user) await client.AuthenticateAsync(user, Cfg("SMTP_PASSWORD") ?? "", ct);
            await client.SendAsync(new MailKit.SendRequest(message), ct);
            await client.DisconnectAsync(true, ct);
            return new DeliveryResult("sent", "smtp");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            return Fail("smtp", ex.Message);
        }
    }

    private async Task<DeliveryResult> SendSmsAsync(string to, string body, CancellationToken ct)
    {
        if (SmsProvider == "log") return new DeliveryResult("logged", "log");
        try
        {
            var sid = Cfg("TWILIO_SID")!;
            var form = new Dictionary<string, string> { ["To"] = to, ["Body"] = body };
            if (Cfg("TWILIO_MESSAGING_SERVICE_SID") is { } service) form["MessagingServiceSid"] = service;
            else form["From"] = Cfg("TWILIO_FROM")!;

            var http = httpClients.CreateClient(nameof(MessageDelivery));
            http.Timeout = Timeout;
            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json")
            {
                Content = new FormUrlEncodedContent(form),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{sid}:{Cfg("TWILIO_TOKEN")}")));
            using var response = await http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode) return new DeliveryResult("sent", "twilio");
            string? detail = null;
            try { detail = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct))?["message"]?.GetValue<string>(); }
            catch (Exception) { /* not JSON */ }
            return Fail("twilio", detail ?? $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            return Fail("twilio", ex.Message);
        }
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex NotDigit();
}
