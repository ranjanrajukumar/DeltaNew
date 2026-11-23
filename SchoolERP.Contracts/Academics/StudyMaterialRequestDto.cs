namespace SchoolERP.Contracts.Academics;

public sealed record StudyMaterialRequestDto(int? ClassId, int? SubjectId, string? Title, string? Url, string? Description);
