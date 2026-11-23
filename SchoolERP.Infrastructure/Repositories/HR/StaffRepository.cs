using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StaffRepository(ApplicationDbContext db) : Repository<Staff>(db), IStaffRepository
{
    public Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default) =>
        Read.ToDictionaryAsync(s => s.Id, s => s.FirstName + " " + s.LastName, ct);

    public Task<string?> NameAsync(int id, CancellationToken ct = default) =>
        Read.Where(s => s.Id == id).Select(s => s.FirstName + " " + s.LastName).FirstOrDefaultAsync(ct);

    public Task<int?> MaxIdIncludingDeletedAsync(CancellationToken ct = default) =>
        Set.IgnoreQueryFilters().MaxAsync(s => (int?)s.Id, ct);

    public Task<bool> StaffNoTakenAsync(string staffNo, CancellationToken ct = default) =>
        Set.IgnoreQueryFilters().AnyAsync(s => s.StaffNo == staffNo, ct);

    public Task<List<Staff>> ListWithDepartmentAsync(CancellationToken ct = default) =>
        Read.Include(s => s.Department).OrderByDescending(s => s.Id).ToListAsync(ct);

    public Task<Staff?> GetWithDepartmentAsync(int id, CancellationToken ct = default) =>
        Read.Include(s => s.Department).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<Staff>> ActiveAsync(string? category = null, CancellationToken ct = default) =>
        Read.Where(s => s.Status == "active" && (string.IsNullOrEmpty(category) || s.StaffCategory == category))
            .OrderBy(s => s.FirstName).ToListAsync(ct);

    public Task<List<Staff>> ActiveWithDepartmentAsync(CancellationToken ct = default) =>
        Read.Include(s => s.Department).Where(s => s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);

    public Task<List<Staff>> ActiveWithSalaryAsync(CancellationToken ct = default) =>
        Read.Include(s => s.StaffSalary).Where(s => s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);

    public async Task RemoveWithPhotosAsync(int id, CancellationToken ct = default)
    {
        Set.RemoveRange(await Set.Where(s => s.Id == id).ToListAsync(ct));
        Db.Set<Photo>().RemoveRange(await Db.Set<Photo>().Where(p => p.Entity == "staff" && p.EntityId == id).ToListAsync(ct));
    }

    public Task<int?> LoginUserIdAsync(int staffId, CancellationToken ct = default) =>
        Db.Set<User>().Where(u => u.StaffId == staffId).Select(u => (int?)u.Id).FirstOrDefaultAsync(ct);
}
