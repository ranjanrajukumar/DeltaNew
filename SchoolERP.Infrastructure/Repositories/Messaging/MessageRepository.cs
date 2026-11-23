using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class MessageRepository(ApplicationDbContext db) : Repository<Message>(db), IMessageRepository
{
    public Task<bool> IsMarkedAbsentAsync(int studentId, DateOnly day, CancellationToken ct = default) =>
        Db.Set<AttendanceMark>().AnyAsync(a => a.StudentId == studentId && a.Date == day && a.Status == "absent", ct);

    public Task<bool> AbsenceSmsLoggedAsync(int studentId, DateOnly day, CancellationToken ct = default) =>
        Db.Set<AbsenceAlert>().AnyAsync(a => a.StudentId == studentId && a.Date == day && a.Method == "sms", ct);

    public Task<Student> StudentWithClassAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).FirstAsync(s => s.Id == studentId, ct);

    public void AddAbsenceAlert(AbsenceAlert alert) => Db.Set<AbsenceAlert>().Add(alert);
}
