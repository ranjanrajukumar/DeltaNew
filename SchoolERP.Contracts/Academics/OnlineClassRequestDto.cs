namespace SchoolERP.Contracts.Academics;

public sealed record OnlineClassRequestDto(string? Title, int? ClassId, int? SubjectId, DateOnly? ClassDate, string? StartTime, string? EndTime, string? Link, string? TeacherName, string? Notes);
