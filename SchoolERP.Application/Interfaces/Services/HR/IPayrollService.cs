using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Salaries and payslips (the Node app's hr/payroll.routes.js).</summary>
public interface IPayrollService
{
    Task<List<object>> SalariesAsync(CancellationToken ct = default);

    Task SaveSalaryAsync(int staffId, Body b, CancellationToken ct = default);

    Task<List<JsonObject>> PayslipsAsync(string? month, CancellationToken ct = default);

    Task<object> GenerateAsync(string? month, CancellationToken ct = default);

    Task PayAsync(int id, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
