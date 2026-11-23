using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class TeacherScopeRepository(ApplicationDbContext db) : ITeacherScopeRepository
{
    public Task<string?> UserEmailAsync(int userId, CancellationToken ct = default) =>
        db.Set<User>().Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync(ct);

    public Task<List<int>> ActiveStaffIdsByEmailAsync(string email, CancellationToken ct = default) =>
        db.Set<Staff>().Where(s => s.Email == email && s.Status == "active").Select(s => s.Id).ToListAsync(ct);

    public Task<List<int>> ClassTeacherClassIdsAsync(IReadOnlyCollection<int> staffIds, CancellationToken ct = default) =>
        db.Set<SchoolClass>().Where(c => c.ClassTeacherId != null && staffIds.Contains(c.ClassTeacherId.Value)).Select(c => c.Id).ToListAsync(ct);

    public Task<List<int>> SubjectClassIdsAsync(IReadOnlyCollection<int> staffIds, CancellationToken ct = default) =>
        db.Set<ClassSubject>().Where(c => c.StaffId != null && staffIds.Contains(c.StaffId.Value)).Select(c => c.ClassId).ToListAsync(ct);

    public Task<List<int>> TimetableClassIdsAsync(IReadOnlyCollection<int> staffIds, CancellationToken ct = default) =>
        db.Set<TimetableSlot>().Where(s => s.StaffId != null && staffIds.Contains(s.StaffId.Value)).Select(s => s.ClassId).ToListAsync(ct);

    public Task<List<int>> SubstitutedSlotIdsAsync(IReadOnlyCollection<int> staffIds, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        db.Set<Substitution>().Where(s => staffIds.Contains(s.SubstituteId) && s.Date >= from && s.Date <= to).Select(s => s.SlotId).ToListAsync(ct);

    public Task<List<int>> ClassIdsOfSlotsAsync(IReadOnlyCollection<int> slotIds, CancellationToken ct = default) =>
        db.Set<TimetableSlot>().Where(s => slotIds.Contains(s.Id)).Select(s => s.ClassId).ToListAsync(ct);

    public Task<List<int?>> StudentClassIdsAsync(IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        db.Set<Student>().Where(s => studentIds.Contains(s.Id)).Select(s => s.ClassId).ToListAsync(ct);
}
