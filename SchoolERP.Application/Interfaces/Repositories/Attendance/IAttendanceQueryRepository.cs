using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Attendance list screens: the daily roster, the correction log, period summaries (Dapper).</summary>
public interface IAttendanceQueryRepository
{
    public sealed record RosterRow(
        int StudentId, string AdmissionNo, string FirstName, string LastName, string? Status, string? ArrivalTime, string? Note,
        DateTime? SavedAt, int? ClassId, string? ClassName, string? Section);

    public sealed record CorrectionRow(
        int Id, DateOnly Date, int StudentId, string FirstName, string LastName, string AdmissionNo, string OldStatus, string NewStatus,
        string? Reason, string? CorrectedByName, DateTime CreatedAt);

    public sealed record PeriodSummaryRow(int Id, string AdmissionNo, string FirstName, string LastName, int Periods, int Absent, int Late);

    /// <summary>Active students (optionally one class) by first name, with their mark for the day and their class.</summary>
    Task<List<RosterRow>> RosterAsync(DateOnly date, int? classId, CancellationToken ct = default);

    /// <summary>Corrections of students that exist, newest first (at most 300).</summary>
    Task<List<CorrectionRow>> CorrectionsAsync(int? studentId, DateOnly? from, DateOnly? to, CancellationToken ct = default);

    /// <summary>Active students of a class (by id) with how many periods were marked, missed and late in [from, to].</summary>
    Task<List<PeriodSummaryRow>> PeriodSummaryAsync(int classId, DateOnly from, DateOnly to, CancellationToken ct = default);
}
