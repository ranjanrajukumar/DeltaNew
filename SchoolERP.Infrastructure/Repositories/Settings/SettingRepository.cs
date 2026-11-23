using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SettingRepository(ApplicationDbContext db) : Repository<Setting>(db), ISettingRepository
{
    public Task<List<Setting>> AllAsync(CancellationToken ct = default) => Read.ToListAsync(ct);

    public Task<Setting?> GetByKeyAsync(string key, CancellationToken ct = default) => Read.FirstOrDefaultAsync(s => s.Key == key, ct);

    public Task<Setting?> FindByKeyAsync(string key, CancellationToken ct = default) => Set.FirstOrDefaultAsync(s => s.Key == key, ct);

    public Task<List<Setting>> ListByKeyAsync(string key, CancellationToken ct = default) => Set.Where(s => s.Key == key).ToListAsync(ct);

    public async Task<string?> RawValueAsync(string key, CancellationToken ct = default) =>
        await Read.Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);
}
