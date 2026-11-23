using SchoolERP.Application.Modules.Academics;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Read-only class queries with joins and counts (Dapper).</summary>
public interface IClassQueryRepository
{
    public sealed record SubjectLink(int ClassId, int SubjectId, int? StaffId);
    public sealed record SlotSubject(int ClassId, int SubjectId);
    public sealed record StaffDesignation(int Id, string? Designation);
    public sealed record OverviewData(List<SubjectLink> Links, List<SlotSubject> Slots, List<StaffDesignation> ActiveStaff, int SubjectCount);

    public sealed record FeeItem(int Id, string Title, decimal Amount, DateOnly? DueDate);
    public sealed record OpenActivity(int Id, string Title, string Kind, DateOnly StartDate, string Status, int? ClassId, decimal Fee);
    public sealed record RouteRiders(int Id, string Name, int Riders);
    public sealed record ClassMapData(
        string? TeacherName, int ActiveStudents, List<FeeItem> Fees, List<OpenActivity> Activities,
        int Riders, List<RouteRiders> Routes, int Boarders, int SlotCount);

    /// <summary>Classes by name then section, each with its number of students and its class teacher's name.</summary>
    Task<List<ClassListRow>> ListAsync(CancellationToken ct = default);

    /// <summary>What the Academics overview counts: subject links, timetable slots, active staff and the number of subjects.</summary>
    Task<OverviewData> OverviewDataAsync(CancellationToken ct = default);

    /// <summary>What one class's page shows besides its subject map.</summary>
    Task<ClassMapData> ClassMapDataAsync(int classId, int? classTeacherId, CancellationToken ct = default);
}
