namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// Email and SMS delivery (the Node app's messaging/providers.js). Never throws: one bad number must not stop a bulk send.
/// With no credentials configured a channel runs in "log" mode: the message is recorded but not delivered.
/// </summary>
public interface IMessageDelivery
{
    /// <summary>{ email: "smtp" | "log", sms: "twilio" | "log" }. Never includes secrets.</summary>
    (string Email, string Sms) ProviderStatus();

    /// <summary>
    /// A phone number in E.164 (+14155550123). One without a country code gets DEFAULT_COUNTRY_CODE; with none configured it is
    /// rejected (null), because guessing could text a stranger in another country.
    /// </summary>
    string? NormalizePhone(string? raw);

    Task<DeliveryResult> DeliverAsync(string channel, string to, string? subject, string body, CancellationToken ct = default);
}
