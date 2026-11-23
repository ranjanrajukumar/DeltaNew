using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class PeriodAttendanceRepository(ApplicationDbContext db) : Repository<PeriodAttendance>(db), IPeriodAttendanceRepository
{
    public Task<List<Student>> ActiveStudentsOfClassAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);

    public Task<List<int>> StudentIdsOfClassAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().Where(s => s.ClassId == classId).Select(s => s.Id).ToListAsync(ct);

    public Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default) =>
        Db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct);

    public Task<List<PeriodAttendance>> ListAsync(int classId, DateOnly date, int period, CancellationToken ct = default) =>
        Read.Where(p => p.ClassId == classId && p.Date == date && p.Period == period).ToListAsync(ct);

    public Task<PeriodAttendance?> FindAsync(int studentId, DateOnly date, int period, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(p => p.StudentId == studentId && p.Date == date && p.Period == period, ct);

    public Task<TimetableSlot?> SlotWithSubjectAsync(int classId, int day, int period, CancellationToken ct = default) =>
        Db.Set<TimetableSlot>().AsNoTracking().Include(s => s.Subject)
            .Where(s => s.ClassId == classId && s.Day == day && s.Period == period && s.Subject != null).FirstOrDefaultAsync(ct);
}
