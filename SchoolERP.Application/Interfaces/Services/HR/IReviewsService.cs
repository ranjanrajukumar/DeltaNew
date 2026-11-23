using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Staff performance reviews (the Node app's hr/reviews.service.js).</summary>
public interface IReviewsService
{
    Task<List<JsonObject>> ListAsync(int? staffId, CancellationToken ct = default);

    Task<int> CreateAsync(Body b, CancellationToken ct = default);

    Task UpdateAsync(int id, Body b, CancellationToken ct = default);

    Task FinalizeAsync(int id, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
