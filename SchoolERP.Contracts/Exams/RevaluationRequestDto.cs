using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record RevaluationRequestDto(int? ExamId, int? StudentId, int? SubjectId, string? Reason);
