using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AcademicYearRepository(ApplicationDbContext db) : Repository<AcademicYear>(db), IAcademicYearRepository
{
    public Task<List<AcademicYear>> ListByStartDescendingAsync(CancellationToken ct = default) => Read.OrderByDescending(y => y.StartDate).ToListAsync(ct);

    public Task<AcademicYear?> CurrentAsync(CancellationToken ct = default) => Read.FirstOrDefaultAsync(y => y.IsCurrent, ct);

    public Task<List<AcademicYear>> OtherCurrentYearsAsync(int exceptId, CancellationToken ct = default) =>
        Set.Where(y => y.IsCurrent && y.Id != exceptId).ToListAsync(ct);

    public Task<Dictionary<int, DateOnly>> StartDatesAsync(CancellationToken ct = default) => Read.ToDictionaryAsync(y => y.Id, y => y.StartDate, ct);

    public Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default) => Read.ToDictionaryAsync(y => y.Id, y => y.Name, ct);
}
