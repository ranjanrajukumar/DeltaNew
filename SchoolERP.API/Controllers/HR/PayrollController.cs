using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;
using SchoolERP.Contracts.Common;
using SchoolERP.Domain.Entities;

namespace SchoolERP.API.Controllers;

/// <summary>/api/payroll: admin and accountant; only an admin marks a payslip paid (hr/payroll.routes.js).</summary>
[ApiController]
[Route("api/payroll")]
public sealed class PayrollController(IPayrollService payroll) : ControllerBase
{
    [HttpGet("salaries")]
    public async Task<IActionResult> Salaries(CancellationToken ct) => Ok(await payroll.SalariesAsync(ct));

    /// <summary>{ basic, allowances, deductions }</summary>
    [HttpPut("salaries/{staffId:int}")]
    public async Task<IActionResult> SaveSalary(int staffId, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await payroll.SaveSalaryAsync(staffId, new Body(json), ct);
        return Ok(new { ok = true });
    }

    /// <summary>?month=YYYY-MM</summary>
    [HttpGet("payslips")]
    public async Task<IActionResult> Payslips([FromQuery] string? month, CancellationToken ct) => Ok(await payroll.PayslipsAsync(month, ct));

    /// <summary>{ month }: a draft payslip for each active staff member with salary terms.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct) =>
        Ok(await payroll.GenerateAsync(new Body(json).Str("month"), ct));

    [HttpPost("payslips/{id:int}/pay")]
    [RequireRole("admin")]
    public async Task<IActionResult> Pay(int id, CancellationToken ct)
    {
        await payroll.PayAsync(id, ct);
        return Ok(new { ok = true });
    }

    [HttpDelete("payslips/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await payroll.DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
