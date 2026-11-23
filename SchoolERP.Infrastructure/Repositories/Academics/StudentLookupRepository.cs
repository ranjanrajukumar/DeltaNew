using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudentLookupRepository(ApplicationDbContext db) : IStudentLookupRepository
{
    public Task<List<Student>> ActiveInClassAsync(int classId, CancellationToken ct = default) =>
        db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);
}
