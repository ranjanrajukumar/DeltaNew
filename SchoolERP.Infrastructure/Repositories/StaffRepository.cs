using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StaffRepository(SchoolErpDbContext db) : Repository<Staff>(db), IStaffRepository
{
    public Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default) =>
        Read.ToDictionaryAsync(s => s.Id, s => s.FirstName + " " + s.LastName, ct);

    public Task<string?> NameAsync(int id, CancellationToken ct = default) =>
        Read.Where(s => s.Id == id).Select(s => s.FirstName + " " + s.LastName).FirstOrDefaultAsync(ct);
}
