using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class OnlineClassRepository(ApplicationDbContext db) : Repository<OnlineClass>(db), IOnlineClassRepository
{
    public Task<List<OnlineClass>> ListAsync(int? classId, string? when, DateOnly today, CancellationToken ct = default)
    {
        var q = Read.Include(c => c.SchoolClass).Include(c => c.Subject).Where(c => c.SchoolClass != null && (classId == null || c.ClassId == classId));
        if (when == "upcoming") q = q.Where(c => c.ClassDate >= today);
        if (when == "past") q = q.Where(c => c.ClassDate < today);
        q = when == "past"
            ? q.OrderByDescending(c => c.ClassDate).ThenByDescending(c => c.StartTime)
            : q.OrderBy(c => c.ClassDate).ThenBy(c => c.StartTime);
        return q.Take(300).ToListAsync(ct);
    }

    public Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default) => Db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct);

    public Task<bool> SubjectExistsAsync(int subjectId, CancellationToken ct = default) => Db.Set<Subject>().AnyAsync(s => s.Id == subjectId, ct);
}
