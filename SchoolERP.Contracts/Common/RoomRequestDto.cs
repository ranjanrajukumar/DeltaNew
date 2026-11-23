namespace SchoolERP.Contracts.Common;

/// <summary>Allowed write fields for a room.</summary>
public sealed record RoomRequestDto(string? Name, string? Kind, int? Capacity);
