using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ClassRepository(SchoolErpDbContext db) : Repository<SchoolClass>(db), IClassRepository
{
    public Task<List<SchoolClass>> ListByNameAsync(CancellationToken ct = default) =>
        Read.OrderBy(c => c.Name).ThenBy(c => c.Section).ToListAsync(ct);

    public Task<List<SchoolClass>> ListByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Read.Where(c => ids.Count == 0 || ids.Contains(c.Id)).OrderBy(c => c.Id).ToListAsync(ct);

    public Task<bool> HasStudentsAsync(int classId, CancellationToken ct = default) => Db.Set<Student>().AnyAsync(s => s.ClassId == classId, ct);

    public Task<bool> HasExamsAsync(int classId, CancellationToken ct = default) => Db.Set<Exam>().AnyAsync(e => e.ClassId == classId, ct);

    public Task<List<SchoolClass>> ClassesOfTeacherAsync(int staffId, CancellationToken ct = default) =>
        Read.Where(c => c.ClassTeacherId == staffId).OrderBy(c => c.Name).ThenBy(c => c.Section).ToListAsync(ct);
}
