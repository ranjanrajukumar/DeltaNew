namespace SchoolERP.Application.Common.Interfaces;

/// <summary>What happened to one message: sent, logged (no provider configured, so only recorded) or failed.</summary>
public sealed record DeliveryResult(string Status, string Provider, string? Error = null);
