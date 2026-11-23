using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AttendanceRepository(ApplicationDbContext db) : Repository<AttendanceMark>(db), IAttendanceRepository
{
    public Task<int> CountStudentsAsync(IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Db.Set<Student>().CountAsync(s => studentIds.Contains(s.Id), ct);

    public Task<List<AttendanceMark>> FindForDateAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Set.Where(a => a.Date == date && studentIds.Contains(a.StudentId)).ToListAsync(ct);

    public Task<List<AttendanceMark>> ListForDateAsync(DateOnly date, IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Read.Where(a => a.Date == date && studentIds.Contains(a.StudentId)).ToListAsync(ct);

    public Task<List<AttendanceMark>> ListBetweenAsync(IReadOnlyCollection<int> studentIds, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.Where(a => studentIds.Contains(a.StudentId) && a.Date >= from && a.Date <= to).ToListAsync(ct);

    public void AddCorrection(AttendanceCorrection correction) => Db.Set<AttendanceCorrection>().Add(correction);

    public Task<bool> IsAbsentAsync(int studentId, DateOnly date, CancellationToken ct = default) =>
        Set.AnyAsync(a => a.StudentId == studentId && a.Date == date && a.Status == "absent", ct);

    public Task<List<AttendanceMark>> AbsentWithStudentsAsync(DateOnly date, int? classId, CancellationToken ct = default) =>
        Read.Include(a => a.Student).ThenInclude(s => s!.SchoolClass)
            .Where(a => a.Date == date && a.Status == "absent" && a.Student != null && a.Student.Status == "active"
                && (classId == null || a.Student.ClassId == classId))
            .ToListAsync(ct);

    public Task<List<IAttendanceRepository.AbsentStudent>> AbsentStudentsAsync(DateOnly date, CancellationToken ct = default) =>
        Read.Where(a => a.Date == date && a.Status == "absent" && a.Student != null && a.Student.Status == "active")
            .Select(a => new IAttendanceRepository.AbsentStudent(a.StudentId, a.Student!.FirstName, a.Student.LastName))
            .ToListAsync(ct);

    public Task<List<Student>> ActiveStudentsWithClassAsync(int? classId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass)
            .Where(s => s.Status == "active" && (classId == null || s.ClassId == classId))
            .OrderBy(s => s.FirstName).ToListAsync(ct);

    public Task<List<CalendarEvent>> HolidaysAsync(DateOnly from, DateOnly to, string audience, CancellationToken ct = default) =>
        Db.Set<CalendarEvent>().AsNoTracking()
            .Where(e => e.Type == "holiday" && (e.Audience == "all" || e.Audience == audience) && e.StartDate <= to && e.EndDate >= from)
            .ToListAsync(ct);
}
