using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class RecordExtraRepository(ApplicationDbContext db) : Repository<RecordExtra>(db), IRecordExtraRepository
{
    public Task<RecordExtra?> GetForAsync(string entity, int entityId, CancellationToken ct = default) =>
        Read.FirstOrDefaultAsync(r => r.Entity == entity && r.EntityId == entityId, ct);

    public Task<RecordExtra?> FindForAsync(string entity, int entityId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(r => r.Entity == entity && r.EntityId == entityId, ct);
}
