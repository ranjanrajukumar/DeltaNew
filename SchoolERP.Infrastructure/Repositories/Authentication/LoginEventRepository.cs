using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class LoginEventRepository(ApplicationDbContext db) : Repository<LoginEvent>(db), ILoginEventRepository
{
    public Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken ct = default) =>
        Set.Where(e => e.CreatedAt < cutoff).ExecuteDeleteAsync(ct);
}
