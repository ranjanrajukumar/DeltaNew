using System.Text.Json.Nodes;

namespace SchoolERP.Contracts.Exams;

public sealed record StudentPromotionRequestDto(int? FromClassId, int? ToClassId, List<int>? StudentIds, bool? OnlyEligible, bool? AllowCrossBoard);
