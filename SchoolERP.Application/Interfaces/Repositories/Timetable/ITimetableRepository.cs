using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Timetable slots and teacher availability (EF Core).</summary>
public interface ITimetableRepository : IRepository<TimetableSlot>
{
    /// <summary>The value of a settings row (the school-wide bell times saved before boards had rules), or null.</summary>
    Task<string?> SettingValueAsync(string key, CancellationToken ct = default);

    /// <summary>Every period number that holds at least one slot.</summary>
    Task<List<int>> UsedPeriodsAsync(CancellationToken ct = default);

    /// <summary>The board of a class, or null when the class does not exist.</summary>
    Task<string?> ClassBoardAsync(int classId, CancellationToken ct = default);

    Task<bool> StaffExistsAsync(int staffId, CancellationToken ct = default);

    /// <summary>Ids of active staff.</summary>
    Task<List<int>> ActiveStaffIdsAsync(CancellationToken ct = default);

    /// <summary>Ids of rooms that are labs.</summary>
    Task<List<int>> LabRoomIdsAsync(CancellationToken ct = default);

    /// <summary>Whether the teacher has a lesson in that day and period.</summary>
    Task<bool> TeachesAsync(int staffId, int day, int period, CancellationToken ct = default);

    /// <summary>A teacher's blocked periods by day then period (read-only).</summary>
    Task<List<StaffUnavailability>> AvailabilityAsync(int staffId, CancellationToken ct = default);

    /// <summary>A teacher's blocked periods, tracked.</summary>
    Task<List<StaffUnavailability>> FindAvailabilityAsync(int staffId, CancellationToken ct = default);

    Task<bool> IsUnavailableAsync(int staffId, int day, int period, CancellationToken ct = default);

    void AddAvailability(IEnumerable<StaffUnavailability> rows);

    void RemoveAvailability(IEnumerable<StaffUnavailability> rows);

    /// <summary>Every blocked (staff, day, period).</summary>
    Task<List<(int StaffId, int Day, int Period)>> BlockedAsync(CancellationToken ct = default);

    /// <summary>Slots the generator must work around: all of them, or those outside <paramref name="exceptClassIds"/> when given.</summary>
    Task<List<TimetableGenerator.Busy>> BusyAsync(IReadOnlyCollection<int>? exceptClassIds, CancellationToken ct = default);

    /// <summary>(class, subject) of every slot of these classes.</summary>
    Task<List<(int ClassId, int SubjectId)>> PlacedSubjectsAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default);

    /// <summary>The slots of these classes, tracked.</summary>
    Task<List<TimetableSlot>> FindOfClassesAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default);

    /// <summary>Tracked slot of a class in a day and period, or null.</summary>
    Task<TimetableSlot?> FindSlotAsync(int? classId, int day, int period, CancellationToken ct = default);

    /// <summary>A lesson (with its class) of the teacher in that period in another class, or null.</summary>
    Task<TimetableSlot?> TeacherClashAsync(int staffId, int day, int period, int? classId, CancellationToken ct = default);

    /// <summary>A lesson (with its class) in the room in that period in another class, or null.</summary>
    Task<TimetableSlot?> RoomClashAsync(int roomId, int day, int period, int? classId, CancellationToken ct = default);

    /// <summary>A lesson (with its class) held in the room in that day and period, or null.</summary>
    Task<TimetableSlot?> LessonInRoomAsync(int roomId, int day, int period, CancellationToken ct = default);

    /// <summary>Room id of every slot that has a room.</summary>
    Task<List<int>> UsedRoomIdsAsync(CancellationToken ct = default);

    Task<bool> UsesRoomAsync(int roomId, CancellationToken ct = default);

    /// <summary>A room's slots with class, subject and teacher, by day then period.</summary>
    Task<List<TimetableSlot>> OfRoomAsync(int roomId, CancellationToken ct = default);

    /// <summary>Teacher ids with a lesson in that day and period.</summary>
    Task<List<int>> TeachingStaffIdsAsync(int day, int period, CancellationToken ct = default);

    /// <summary>The day's slots of these teachers with class, subject and teacher, by period.</summary>
    Task<List<TimetableSlot>> OfTeachersOnDayAsync(int day, IReadOnlyCollection<int> staffIds, CancellationToken ct = default);

    /// <summary>Every slot with class, subject and teacher.</summary>
    Task<List<TimetableSlot>> WithNamesAsync(CancellationToken ct = default);
}
