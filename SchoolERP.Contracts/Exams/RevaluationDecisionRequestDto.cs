using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record RevaluationDecisionRequestDto(decimal? NewMarks, string? Remarks);
