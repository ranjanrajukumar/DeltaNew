using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.HR;

/// <summary>
/// Salaries and payslips (the Node app's hr/payroll.routes.js). Separation of duties: the accountant prepares salaries and
/// payslips; only an admin approves a payment, and the approver is recorded on the payslip.
/// </summary>
public sealed partial class PayrollService(IStaffRepository staffRepo, IStaffSalaryRepository salaries, IPayslipRepository payslips, IUnitOfWork unitOfWork, ICurrentUser user) : IPayrollService
{
    private static string NameOf(Staff s) => $"{s.FirstName} {s.LastName}";

    /// <summary>Math.round(Number(value) * 100) as whole cents.</summary>
    private static decimal Cents(decimal value) => Math.Floor(value * 100 + 0.5m);

    /// <summary>Every active staff member with their salary terms (if set).</summary>
    public async Task<List<object>> SalariesAsync(CancellationToken ct = default) =>
        (await staffRepo.ActiveWithSalaryAsync(ct))
        .Select(s => (object)new
        {
            staffId = s.Id, staffNo = s.StaffNo, name = NameOf(s),
            basic = s.StaffSalary?.Basic, allowances = s.StaffSalary?.Allowances, deductions = s.StaffSalary?.Deductions,
        }).ToList();

    /// <summary>{ basic, allowances = 0, deductions = 0 }</summary>
    public async Task SaveSalaryAsync(int staffId, Body b, CancellationToken ct = default)
    {
        if (!b.Has("basic") || (b.Raw("basic") is JsonValue v && v.TryGetValue<string>(out var s) && s == ""))
            throw HttpException.BadRequest("Basic salary is required");
        decimal Money(string key, decimal fallback) =>
            !b.Has(key) ? fallback : (decimal?)JsValue.ConvertTo(b.Raw(key), typeof(decimal?)) ?? throw HttpException.BadRequest("Invalid value in the request");
        var basic = Money("basic", 0);
        var allowances = Money("allowances", 0);
        var deductions = Money("deductions", 0);
        if (Cents(deductions) > Cents(basic) + Cents(allowances)) throw HttpException.BadRequest("Deductions exceed the gross salary");
        if (!await staffRepo.ExistsAsync(staffId, ct)) throw HttpException.NotFound("Staff not found");
        var row = await salaries.FindByStaffAsync(staffId, ct);
        if (row is null)
        {
            row = new StaffSalary { StaffId = staffId };
            salaries.Add(row);
        }
        (row.Basic, row.Allowances, row.Deductions) = (basic, allowances, deductions);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static void CheckMonth(string? month)
    {
        if (month is null || !Month().IsMatch(month)) throw HttpException.BadRequest("month must be YYYY-MM");
    }

    public async Task<List<JsonObject>> PayslipsAsync(string? month, CancellationToken ct = default)
    {
        CheckMonth(month);
        return (await payslips.ForMonthWithStaffAsync(month!, ct))
            .Select(p =>
            {
                var o = JsonDefaults.ToJsonObject(p);
                o.Remove("Staff");
                o["staffName"] = NameOf(p.Staff!);
                o["staffNo"] = p.Staff!.StaffNo;
                return o;
            }).ToList();
    }

    /// <summary>Creates a draft payslip for each active staff member with salary terms; existing ones are left alone.</summary>
    public async Task<object> GenerateAsync(string? month, CancellationToken ct = default)
    {
        CheckMonth(month);
        var terms = await salaries.ForActiveStaffAsync(ct);
        var existing = await payslips.StaffIdsForMonthAsync(month!, ct);
        var rows = terms.Where(s => !existing.Contains(s.StaffId)).Select(s => new Payslip
        {
            StaffId = s.StaffId, Month = month!, Basic = s.Basic, Allowances = s.Allowances, Deductions = s.Deductions,
            Net = (Cents(s.Basic) + Cents(s.Allowances) - Cents(s.Deductions)) / 100,
        }).ToList();
        if (rows.Count > 0)
        {
            payslips.AddRange(rows);
            await unitOfWork.SaveChangesAsync(ct);
        }
        return new { created = rows.Count, skipped = terms.Count - rows.Count };
    }

    public Task PayAsync(int id, CancellationToken ct = default) => unitOfWork.InTransactionAsync(async () =>
    {
        var slip = await payslips.FindAsync(id, ct) ?? throw HttpException.NotFound("Payslip not found");
        if (slip.Status == "paid") throw HttpException.Conflict("This payslip is already paid");
        slip.Status = "paid";
        slip.PaidOn = Clock.Today();
        slip.PaidByName = user.Name;
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }, ct);

    /// <summary>Only drafts can be removed (e.g. to regenerate after changing salary terms).</summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var slip = await payslips.FindAsync(id, ct) ?? throw HttpException.NotFound("Payslip not found");
        if (slip.Status == "paid") throw HttpException.Conflict("A paid payslip cannot be deleted");
        payslips.Remove(slip);
        await unitOfWork.SaveChangesAsync(ct);
    }

    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex Month();
}
