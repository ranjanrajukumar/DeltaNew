namespace SchoolERP.Contracts.Academics;

/// <summary>Whitelisted class fields accepted by create and update.</summary>
public sealed record ClassRequestDto(string? Name, string? Section, string? Room, int? BranchId, int? ClassTeacherId, int? BoardId, string? Board);
