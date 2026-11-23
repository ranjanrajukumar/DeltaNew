using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ClassSubjectRepository(ApplicationDbContext db) : Repository<ClassSubject>(db), IClassSubjectRepository
{
    public Task<bool> ExistsAsync(int classId, int subjectId, CancellationToken ct = default) =>
        Set.AnyAsync(l => l.ClassId == classId && l.SubjectId == subjectId, ct);

    public Task<ClassSubject?> FindAsync(int classId, int subjectId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(l => l.ClassId == classId && l.SubjectId == subjectId, ct);

    public Task<List<ClassSubject>> OfClassesAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default) =>
        Read.Where(l => classIds.Contains(l.ClassId)).ToListAsync(ct);

    public Task<List<ClassSubject>> WithTeacherAsync(CancellationToken ct = default) => Read.Where(l => l.StaffId != null).ToListAsync(ct);

    public Task<List<SchoolClass>> ClassesAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Db.Set<SchoolClass>().AsNoTracking().Where(c => ids.Count == 0 || ids.Contains(c.Id)).ToListAsync(ct);
}
