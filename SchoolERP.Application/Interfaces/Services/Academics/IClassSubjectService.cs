using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Which subjects each class studies.</summary>
public interface IClassSubjectService
{
    Task<List<object>> ListAsync(CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Gives each class its board's default subjects and periods per week.</summary>
    Task<object> ApplyTemplateAsync(JsonNode? classIds, CancellationToken ct = default);
}
