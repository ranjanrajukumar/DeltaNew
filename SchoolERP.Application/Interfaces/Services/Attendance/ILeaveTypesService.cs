using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Leave types, yearly limits and balances (the Node app's leave/leaveTypes.service.js).</summary>
public interface ILeaveTypesService
{
    Task<List<LeaveType>> ListAsync(string? kind, bool all, CancellationToken ct = default);

    /// <summary>The type an application names, checked to be active and to apply to this kind of person.</summary>
    Task<LeaveType> TypeForAsync(int? id, string kind, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<decimal> DaysUsedAsync(string kind, int personId, int typeId, int year, bool includePending = false, int? exceptId = null, CancellationToken ct = default);

    Task CheckLimitAsync(string kind, int personId, LeaveType type, int year, decimal days, bool includePending = false, int? exceptId = null, CancellationToken ct = default);

    Task<object> BalancesAsync(string? kind, int? year, CancellationToken ct = default);
}
