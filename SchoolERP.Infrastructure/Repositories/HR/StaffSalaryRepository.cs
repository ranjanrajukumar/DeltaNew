using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StaffSalaryRepository(ApplicationDbContext db) : Repository<StaffSalary>(db), IStaffSalaryRepository
{
    public Task<StaffSalary?> FindByStaffAsync(int staffId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(x => x.StaffId == staffId, ct);

    public Task<List<StaffSalary>> ForActiveStaffAsync(CancellationToken ct = default) =>
        Read.Where(s => s.Staff != null && s.Staff.Status == "active").ToListAsync(ct);
}
