namespace SchoolERP.Contracts.Academics;

public sealed record SyllabusTopicRequestDto(int? ClassId, int? SubjectId, int? Sequence, string? Title, string? Objective, DateOnly? PlannedDate, string? Status);
