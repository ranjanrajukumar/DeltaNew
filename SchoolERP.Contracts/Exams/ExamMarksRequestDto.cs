using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record ExamMarksRequestDto(JsonArray? Records);
