using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudyMaterialRepository(ApplicationDbContext db) : Repository<StudyMaterial>(db), IStudyMaterialRepository
{
    public Task<List<StudyMaterial>> ListWithNamesAsync(int? classId, int? subjectId, CancellationToken ct = default) =>
        Read.Include(m => m.SchoolClass).Include(m => m.Subject)
            .Where(m => m.SchoolClass != null && m.Subject != null
                && (classId == null || m.ClassId == classId) && (subjectId == null || m.SubjectId == subjectId))
            .OrderByDescending(m => m.Id).ToListAsync(ct);
}
