using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Timetable weeks with names (Dapper).</summary>
public interface ITimetableQueryRepository
{
    public sealed record SlotRow(
        int Id, int ClassId, int Day, int Period, int SubjectId, int? StaffId, int? RoomId,
        string? RoomName, string ClassName, string SubjectName, string? TeacherName);

    /// <summary>A class's week and/or one teacher's week, by day then period.</summary>
    Task<List<SlotRow>> WeekAsync(int? classId, int? staffId, CancellationToken ct = default);
}
