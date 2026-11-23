using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Classes (the Node app's academics/classes.routes.js and classes.service.js).</summary>
public interface IClassService
{
    Task<List<ClassListRow>> ListAsync(CancellationToken ct = default);

    Task<SchoolClass?> GetAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    /// <summary>False when the class does not exist.</summary>
    Task<bool> UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task<ClassDeleteResult> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>The Academics overview: tiles and one row per class.</summary>
    Task<object> OverviewAsync(CancellationToken ct = default);

    /// <summary>One class on a page; null when the class does not exist.</summary>
    Task<object?> ClassMapAsync(int id, CancellationToken ct = default);

    /// <summary>The classes a staff member is class teacher of, as { id, name, section }.</summary>
    Task<List<object>> ClassesOfAsync(int? staffId, CancellationToken ct = default);

    Task<List<int>> ClassIdsOfAsync(int? staffId, CancellationToken ct = default);
}
