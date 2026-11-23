using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudentLeaveRepository(ApplicationDbContext db) : Repository<StudentLeave>(db), IStudentLeaveRepository
{
    public Task<List<int>> ApprovedStudentIdsOnAsync(DateOnly date, CancellationToken ct = default) =>
        Set.Where(l => l.Status == "approved" && l.FromDate <= date && l.ToDate >= date).Select(l => l.StudentId).ToListAsync(ct);

    public Task<List<StudentLeave>> ListWithDetailsAsync(string? status, int? studentId, int? classId, CancellationToken ct = default) =>
        Read.Include(l => l.Student).ThenInclude(s => s!.SchoolClass)
            .Include(l => l.LeaveType)
            .Where(l => l.Student != null
                && (string.IsNullOrEmpty(status) || l.Status == status)
                && (studentId == null || l.StudentId == studentId)
                && (classId == null || l.Student.ClassId == classId))
            .OrderByDescending(l => l.Id)
            .Take(300)
            .ToListAsync(ct);

    public Task<List<StudentLeave>> ApprovedBetweenAsync(IReadOnlyCollection<int> studentIds, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.Where(l => studentIds.Contains(l.StudentId) && l.Status == "approved" && l.FromDate <= to && l.ToDate >= from).ToListAsync(ct);

    public Task<StudentLeave?> ClashAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        Read.FirstOrDefaultAsync(l =>
            l.StudentId == studentId && (l.Status == "pending" || l.Status == "approved") && l.FromDate <= to && l.ToDate >= from, ct);

    public Task<Student?> StudentWithClassAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).FirstOrDefaultAsync(s => s.Id == studentId, ct);

    public Task<Student> StudentAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().FirstAsync(s => s.Id == studentId, ct);

    public Task<int?> StudentUserIdAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<StudentAccount>().Where(a => a.StudentId == studentId).Select(a => (int?)a.UserId).FirstOrDefaultAsync(ct);

    public Task<List<int?>> ParentUserIdsAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<ParentStudent>().Where(l => l.StudentId == studentId && l.Parent != null).Select(l => (int?)l.Parent!.UserId).ToListAsync(ct);

    public Task<int?> UserIdOfStaffAsync(int staffId, CancellationToken ct = default) =>
        Db.Set<User>().Where(u => u.StaffId == staffId).Select(u => (int?)u.Id).FirstOrDefaultAsync(ct);

    public Task<List<int?>> AdminUserIdsAsync(CancellationToken ct = default) =>
        Db.Set<User>().Where(u => u.Role == "admin").Select(u => (int?)u.Id).ToListAsync(ct);
}
