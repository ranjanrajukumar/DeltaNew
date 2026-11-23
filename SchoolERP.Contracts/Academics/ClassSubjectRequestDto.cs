namespace SchoolERP.Contracts.Academics;

public sealed record ClassSubjectRequestDto(int? ClassId, int? SubjectId, int? StaffId, string? Component, bool? IsElective, string? ElectiveGroup, int? PeriodsPerWeek, decimal? PracticalShare);
