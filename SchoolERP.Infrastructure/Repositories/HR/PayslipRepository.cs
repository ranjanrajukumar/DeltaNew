using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class PayslipRepository(ApplicationDbContext db) : Repository<Payslip>(db), IPayslipRepository
{
    public Task<bool> AnyForStaffAsync(int staffId, CancellationToken ct = default) => Set.AnyAsync(p => p.StaffId == staffId, ct);

    public Task<List<Payslip>> ForMonthWithStaffAsync(string month, CancellationToken ct = default) =>
        Read.Include(p => p.Staff).Where(p => p.Month == month && p.Staff != null).OrderBy(p => p.Id).ToListAsync(ct);

    public async Task<HashSet<int>> StaffIdsForMonthAsync(string month, CancellationToken ct = default) =>
        (await Set.Where(p => p.Month == month).Select(p => p.StaffId).ToListAsync(ct)).ToHashSet();
}
