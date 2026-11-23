using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class TimetableRepository(ApplicationDbContext db) : Repository<TimetableSlot>(db), ITimetableRepository
{
    private DbSet<StaffUnavailability> Unavailability => Db.Set<StaffUnavailability>();

    public Task<string?> SettingValueAsync(string key, CancellationToken ct = default) =>
        Db.Set<Setting>().AsNoTracking().Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);

    public Task<List<int>> UsedPeriodsAsync(CancellationToken ct = default) => Set.Select(s => s.Period).Distinct().ToListAsync(ct);

    public Task<string?> ClassBoardAsync(int classId, CancellationToken ct = default) =>
        Db.Set<SchoolClass>().AsNoTracking().Where(c => c.Id == classId).Select(c => c.Board).FirstOrDefaultAsync(ct);

    public Task<bool> StaffExistsAsync(int staffId, CancellationToken ct = default) => Db.Set<Staff>().AnyAsync(s => s.Id == staffId, ct);

    public Task<List<int>> ActiveStaffIdsAsync(CancellationToken ct = default) =>
        Db.Set<Staff>().Where(s => s.Status == "active").Select(s => s.Id).ToListAsync(ct);

    public Task<List<int>> LabRoomIdsAsync(CancellationToken ct = default) =>
        Db.Set<Room>().Where(r => r.Kind == "lab").Select(r => r.Id).ToListAsync(ct);

    public Task<bool> TeachesAsync(int staffId, int day, int period, CancellationToken ct = default) =>
        Set.AnyAsync(s => s.StaffId == staffId && s.Day == day && s.Period == period, ct);

    public Task<List<StaffUnavailability>> AvailabilityAsync(int staffId, CancellationToken ct = default) =>
        Unavailability.AsNoTracking().Where(u => u.StaffId == staffId).OrderBy(u => u.Day).ThenBy(u => u.Period).ToListAsync(ct);

    public Task<List<StaffUnavailability>> FindAvailabilityAsync(int staffId, CancellationToken ct = default) =>
        Unavailability.Where(u => u.StaffId == staffId).ToListAsync(ct);

    public Task<bool> IsUnavailableAsync(int staffId, int day, int period, CancellationToken ct = default) =>
        Unavailability.AnyAsync(u => u.StaffId == staffId && u.Day == day && u.Period == period, ct);

    public void AddAvailability(IEnumerable<StaffUnavailability> rows) => Unavailability.AddRange(rows);

    public void RemoveAvailability(IEnumerable<StaffUnavailability> rows) => Unavailability.RemoveRange(rows);

    public async Task<List<(int StaffId, int Day, int Period)>> BlockedAsync(CancellationToken ct = default) =>
        (await Unavailability.AsNoTracking().Select(u => new { u.StaffId, u.Day, u.Period }).ToListAsync(ct))
            .Select(u => (u.StaffId, u.Day, u.Period)).ToList();

    public Task<List<TimetableGenerator.Busy>> BusyAsync(IReadOnlyCollection<int>? exceptClassIds, CancellationToken ct = default)
    {
        var replace = exceptClassIds is not null;
        var classIds = exceptClassIds ?? [];
        return Read.Where(s => !replace || !classIds.Contains(s.ClassId))
            .Select(s => new TimetableGenerator.Busy(s.ClassId, s.Day, s.Period, s.StaffId, s.RoomId))
            .ToListAsync(ct);
    }

    public async Task<List<(int ClassId, int SubjectId)>> PlacedSubjectsAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default) =>
        (await Read.Where(s => classIds.Contains(s.ClassId)).Select(s => new { s.ClassId, s.SubjectId }).ToListAsync(ct))
            .Select(s => (s.ClassId, s.SubjectId)).ToList();

    public Task<List<TimetableSlot>> FindOfClassesAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default) =>
        Set.Where(s => classIds.Contains(s.ClassId)).ToListAsync(ct);

    public Task<TimetableSlot?> FindSlotAsync(int? classId, int day, int period, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(s => s.ClassId == classId && s.Day == day && s.Period == period, ct);

    public Task<TimetableSlot?> TeacherClashAsync(int staffId, int day, int period, int? classId, CancellationToken ct = default) =>
        Read.Include(s => s.SchoolClass)
            .FirstOrDefaultAsync(s => s.StaffId == staffId && s.Day == day && s.Period == period && s.ClassId != classId && s.SchoolClass != null, ct);

    public Task<TimetableSlot?> RoomClashAsync(int roomId, int day, int period, int? classId, CancellationToken ct = default) =>
        Read.Include(s => s.SchoolClass)
            .FirstOrDefaultAsync(s => s.RoomId == roomId && s.Day == day && s.Period == period && s.ClassId != classId && s.SchoolClass != null, ct);

    public Task<TimetableSlot?> LessonInRoomAsync(int roomId, int day, int period, CancellationToken ct = default) =>
        Read.Include(s => s.SchoolClass)
            .FirstOrDefaultAsync(s => s.RoomId == roomId && s.Day == day && s.Period == period && s.SchoolClass != null, ct);

    public Task<List<int>> UsedRoomIdsAsync(CancellationToken ct = default) =>
        Set.Where(s => s.RoomId != null).Select(s => s.RoomId!.Value).ToListAsync(ct);

    public Task<bool> UsesRoomAsync(int roomId, CancellationToken ct = default) => Set.AnyAsync(s => s.RoomId == roomId, ct);

    public Task<List<TimetableSlot>> OfRoomAsync(int roomId, CancellationToken ct = default) =>
        Read.Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Staff)
            .Where(s => s.RoomId == roomId && s.SchoolClass != null && s.Subject != null)
            .OrderBy(s => s.Day).ThenBy(s => s.Period).ToListAsync(ct);

    public Task<List<int>> TeachingStaffIdsAsync(int day, int period, CancellationToken ct = default) =>
        Set.Where(s => s.Day == day && s.Period == period && s.StaffId != null).Select(s => s.StaffId!.Value).ToListAsync(ct);

    public Task<List<TimetableSlot>> OfTeachersOnDayAsync(int day, IReadOnlyCollection<int> staffIds, CancellationToken ct = default) =>
        Read.Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Staff)
            .Where(s => s.Day == day && s.StaffId != null && staffIds.Contains(s.StaffId.Value) && s.SchoolClass != null && s.Subject != null)
            .OrderBy(s => s.Period).ToListAsync(ct);

    public Task<List<TimetableSlot>> WithNamesAsync(CancellationToken ct = default) =>
        Read.Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Staff)
            .Where(s => s.SchoolClass != null && s.Subject != null).ToListAsync(ct);
}
