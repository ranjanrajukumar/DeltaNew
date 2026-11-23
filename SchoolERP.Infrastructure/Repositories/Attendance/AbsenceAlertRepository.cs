using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AbsenceAlertRepository(ApplicationDbContext db) : Repository<AbsenceAlert>(db), IAbsenceAlertRepository
{
    public Task<List<AbsenceAlert>> ForStudentsAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Read.Where(a => a.Date == date && studentIds.Contains(a.StudentId)).OrderByDescending(a => a.Id).ToListAsync(ct);

    public Task<List<int>> AlertedStudentIdsAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Set.Where(a => a.Date == date && studentIds.Contains(a.StudentId)).Select(a => a.StudentId).ToListAsync(ct);
}
