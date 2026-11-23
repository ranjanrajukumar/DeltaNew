using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record GradingRuleRequestDto(string? Board, decimal? PassPercent, JsonArray? Bands, string? SourceRef, string? ChangeNote);
