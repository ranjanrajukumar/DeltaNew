using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class LeaveTypeRepository(ApplicationDbContext db) : Repository<LeaveType>(db), ILeaveTypeRepository
{
    public Task<List<LeaveType>> ListAsync(string? kind, bool all, CancellationToken ct = default) =>
        Read.Where(t => (all || t.Active) && (string.IsNullOrEmpty(kind) || t.AppliesTo == kind || t.AppliesTo == "both"))
            .OrderBy(t => t.Id)
            .ToListAsync(ct);

    public Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken ct = default) =>
        Set.AnyAsync(t => t.Name == name && t.Id != (exceptId ?? 0), ct);

    public async Task<int> UsageCountAsync(int id, CancellationToken ct = default) =>
        await Db.Set<LeaveRequest>().CountAsync(l => l.LeaveTypeId == id, ct) + await Db.Set<StudentLeave>().CountAsync(l => l.LeaveTypeId == id, ct);

    public Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default) =>
        Read.ToDictionaryAsync(t => t.Id, t => t.Name, ct);

    public Task<decimal> StaffDaysUsedAsync(int staffId, int typeId, DateOnly start, DateOnly end, bool includePending, int? exceptId, CancellationToken ct = default) =>
        Db.Set<LeaveRequest>()
            .Where(l => l.StaffId == staffId && l.LeaveTypeId == typeId && (l.Status == "approved" || (includePending && l.Status == "pending"))
                && l.FromDate >= start && l.ToDate <= end && (exceptId == null || l.Id != exceptId))
            .SumAsync(l => l.Days, ct);

    public Task<decimal> StudentDaysUsedAsync(int studentId, int typeId, DateOnly start, DateOnly end, bool includePending, int? exceptId, CancellationToken ct = default) =>
        Db.Set<StudentLeave>()
            .Where(l => l.StudentId == studentId && l.LeaveTypeId == typeId && (l.Status == "approved" || (includePending && l.Status == "pending"))
                && l.FromDate >= start && l.ToDate <= end && (exceptId == null || l.Id != exceptId))
            .SumAsync(l => l.Days, ct);
}
