using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class RoleRightRepository(ApplicationDbContext db) : Repository<RoleRight>(db), IRoleRightRepository
{
    public Task<List<RoleRight>> AllAsync(CancellationToken ct = default) => Read.ToListAsync(ct);
}
