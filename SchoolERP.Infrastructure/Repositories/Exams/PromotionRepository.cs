using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class PromotionRepository(ApplicationDbContext db) : IPromotionRepository
{
    public Task<SchoolClass?> ClassAsync(int classId, CancellationToken ct = default) =>
        db.Set<SchoolClass>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId, ct);

    public Task<bool> ClassExistsAsync(int? classId, CancellationToken ct = default) => db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct);

    public Task<List<int>> ActiveStudentIdsAsync(int classId, CancellationToken ct = default) =>
        db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.Id).Select(s => s.Id).ToListAsync(ct);

    public Task<List<Student>> ActiveStudentsToMoveAsync(int? classId, IReadOnlyCollection<int>? only, CancellationToken ct = default) =>
        db.Set<Student>().Where(s => s.ClassId == classId && s.Status == "active" && (only == null || only.Contains(s.Id))).ToListAsync(ct);
}
