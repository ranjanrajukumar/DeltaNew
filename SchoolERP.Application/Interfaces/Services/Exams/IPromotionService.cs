using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Year-end promotion (the Node app's academics/promotion.routes.js and promotionEligibility.js).</summary>
public interface IPromotionService
{
    Task<(string Board, JsonObject? Rule, List<Modules.Academics.PromotionService.Eligibility> Rows)> EligibilityAsync(int classId, CancellationToken ct = default);

    Task<object> EligibilityJsonAsync(int classId, CancellationToken ct = default);

    Task<object> PromoteAsync(JsonObject? json, CancellationToken ct = default);
}
