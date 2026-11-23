using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SyllabusRepository(ApplicationDbContext db) : Repository<SyllabusTopic>(db), ISyllabusRepository
{
    public Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default) =>
        Read.Where(t => (classId == null || t.ClassId == classId) && (subjectId == null || t.SubjectId == subjectId))
            .OrderBy(t => t.ClassId).ThenBy(t => t.SubjectId).ThenBy(t => t.Sequence).ThenBy(t => t.Id)
            .ToListAsync(ct);
}
