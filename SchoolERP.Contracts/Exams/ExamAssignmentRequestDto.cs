using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record ExamAssignmentRequestDto(int? ExamId, int? GroupId, decimal? Weight);
