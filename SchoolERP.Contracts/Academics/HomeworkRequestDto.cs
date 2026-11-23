namespace SchoolERP.Contracts.Academics;

public sealed record HomeworkRequestDto(int? ClassId, int? SubjectId, string? Title, string? Description, DateOnly? AssignedOn, DateOnly? DueDate, int? MaxMarks);
