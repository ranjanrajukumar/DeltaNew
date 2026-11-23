using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record ProgressAreaRequestDto(string? Name, string? Category);
