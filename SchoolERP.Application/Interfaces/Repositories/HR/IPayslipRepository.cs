using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Payslips (EF Core).</summary>
public interface IPayslipRepository : IRepository<Payslip>
{
    Task<bool> AnyForStaffAsync(int staffId, CancellationToken ct = default);

    /// <summary>A month's payslips with their staff (only those with a live staff record), by id.</summary>
    Task<List<Payslip>> ForMonthWithStaffAsync(string month, CancellationToken ct = default);

    /// <summary>Ids of staff who already have a payslip for the month.</summary>
    Task<HashSet<int>> StaffIdsForMonthAsync(string month, CancellationToken ct = default);
}
