using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class FeeRepository(ApplicationDbContext db) : Repository<Fee>(db), IFeeRepository
{
    public Task<List<int>> StudentsBilledForAsync(string title, IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Set.Where(f => f.Title == title && studentIds.Contains(f.StudentId)).Select(f => f.StudentId).ToListAsync(ct);

    public Task<List<FeeStructure>> StructureOfClassAsync(int classId, CancellationToken ct = default) =>
        Db.Set<FeeStructure>().AsNoTracking().Where(f => f.ClassId == classId).ToListAsync(ct);
}
