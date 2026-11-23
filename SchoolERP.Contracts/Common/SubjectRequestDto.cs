namespace SchoolERP.Contracts.Common;

/// <summary>Allowed write fields for a subject.</summary>
public sealed record SubjectRequestDto(string? Name, string? Code);
