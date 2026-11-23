using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>A class teacher's view of their class today (Dapper).</summary>
public interface IMyClassQueryRepository
{
    public sealed record RollRow(int Id, string AdmissionNo, string FirstName, string LastName, string? GuardianName, string? GuardianPhone, string? Today);
    public sealed record LeaveRow(int StudentId, string Status, DateOnly FromDate, DateOnly ToDate);

    /// <summary>Active students by name with their mark on <paramref name="date"/>, and leave that is pending or approved and not over.</summary>
    Task<(List<RollRow> Roll, List<LeaveRow> Leaves)> TodayAsync(int classId, DateOnly date, CancellationToken ct = default);
}
