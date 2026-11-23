using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Students (the Node app's academics/students.service.js).</summary>
public interface IStudentsService
{
    /// <summary>Problems with a student payload; empty when valid. <paramref name="partial"/> skips the required checks.</summary>
    Task<List<string>> ValidateAsync(Body body, bool partial, bool admissionNoAuto = false, CancellationToken ct = default);

    /// <summary>Students, newest first: the full list, or { rows, total, page, pageSize } with page.</summary>
    Task<object> ListAsync(StudentsService.ListQuery query, CancellationToken ct = default);

    Task<JsonObject?> GetAsync(int id, CancellationToken ct = default);

    /// <summary>Registers a student (optionally billing the class's fees in the same transaction). Returns { id, admissionNo, feesBilled }.</summary>
    Task<object> CreateAsync(Body body, bool allowDuplicate, bool billFees, CancellationToken ct = default);

    /// <summary>Returns false when the student does not exist.</summary>
    Task<bool> UpdateAsync(int id, Body body, CancellationToken ct = default);

    /// <summary>Soft-deletes a student with their records, files, login and photos. Returns false when the student does not exist.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    Task<object> RegistrationSummaryAsync(CancellationToken ct = default);

    Task<object> SummaryAsync(CancellationToken ct = default);
}
