using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Job openings and applicants (the Node app's hr/recruitment.service.js).</summary>
public interface IRecruitmentService
{
    Task<List<object>> ListOpeningsAsync(CancellationToken ct = default);

    Task<int> SaveOpeningAsync(int? id, Body b, CancellationToken ct = default);

    Task DeleteOpeningAsync(int id, CancellationToken ct = default);

    Task<List<JsonObject>> ListApplicantsAsync(int? openingId, CancellationToken ct = default);

    Task<int> AddApplicantAsync(Body b, CancellationToken ct = default);

    Task ChangeStatusAsync(int id, string? status, CancellationToken ct = default);

    Task DeleteApplicantAsync(int id, CancellationToken ct = default);

    Task<object> HireAsync(int id, Body b, CancellationToken ct = default);
}
