using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record ExamGroupRequestDto(int? ClassId, string? Name);
