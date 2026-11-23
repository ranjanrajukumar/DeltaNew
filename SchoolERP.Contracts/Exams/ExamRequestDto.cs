using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record ExamRequestDto(string? Name, int? ClassId, DateOnly? StartDate, DateOnly? EndDate, int? MaxMarks);
