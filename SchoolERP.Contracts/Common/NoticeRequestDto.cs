namespace SchoolERP.Contracts.Common;

/// <summary>Allowed write fields for a notice.</summary>
public sealed record NoticeRequestDto(string? Title, string? Body, string? Audience);
