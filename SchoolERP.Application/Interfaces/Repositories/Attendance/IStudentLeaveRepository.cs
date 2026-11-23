using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Student leave requests (EF Core).</summary>
public interface IStudentLeaveRepository : IRepository<StudentLeave>
{
    /// <summary>Students with an approved leave covering the date (one entry per leave).</summary>
    Task<List<int>> ApprovedStudentIdsOnAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>Requests (newest first, at most 300) with student, class and leave type loaded.</summary>
    Task<List<StudentLeave>> ListWithDetailsAsync(string? status, int? studentId, int? classId, CancellationToken ct = default);

    /// <summary>Approved leaves of these students overlapping [from, to] (read-only).</summary>
    Task<List<StudentLeave>> ApprovedBetweenAsync(IReadOnlyCollection<int> studentIds, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>A pending or approved leave of the student overlapping [from, to], or null.</summary>
    Task<StudentLeave?> ClashAsync(int studentId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>The student with their class (read-only), or null.</summary>
    Task<Student?> StudentWithClassAsync(int studentId, CancellationToken ct = default);

    /// <summary>The student (read-only); throws when there is none.</summary>
    Task<Student> StudentAsync(int studentId, CancellationToken ct = default);

    /// <summary>The user id of the student's own login, or null.</summary>
    Task<int?> StudentUserIdAsync(int studentId, CancellationToken ct = default);

    /// <summary>The user ids of the student's parents.</summary>
    Task<List<int?>> ParentUserIdsAsync(int studentId, CancellationToken ct = default);

    /// <summary>The user id of the login linked to a staff member, or null.</summary>
    Task<int?> UserIdOfStaffAsync(int staffId, CancellationToken ct = default);

    /// <summary>The user ids of every admin.</summary>
    Task<List<int?>> AdminUserIdsAsync(CancellationToken ct = default);
}
