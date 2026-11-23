using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ExamGroupRepository(ApplicationDbContext db) : Repository<ExamGroup>(db), IExamGroupRepository
{
    public Task<bool> ExistsAsync(int classId, string? name, CancellationToken ct = default) =>
        Set.AnyAsync(g => g.ClassId == classId && g.Name == name, ct);

    public Task<ExamGroup?> GetWithClassAsync(int groupId, CancellationToken ct = default) =>
        Read.Include(g => g.SchoolClass).FirstOrDefaultAsync(g => g.Id == groupId, ct);

    public Task<ExamGroup?> GetAsync(int classId, string? name, CancellationToken ct = default) =>
        Read.FirstOrDefaultAsync(g => g.ClassId == classId && g.Name == name, ct);
}
