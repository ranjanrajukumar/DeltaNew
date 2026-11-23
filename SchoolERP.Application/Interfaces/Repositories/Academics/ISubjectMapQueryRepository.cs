namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Read-only subject-map queries with joins and counts (Dapper).</summary>
public interface ISubjectMapQueryRepository
{
    /// <summary>One subject of a class with its subject, teacher and how many students chose it (when an elective).</summary>
    public sealed record ClassSubjectRow(
        int Id, int SubjectId, string Name, string Code, string Component, bool IsElective, string? ElectiveGroup,
        int? PeriodsPerWeek, int? PracticalShare, int? StaffId, string? TeacherName, int Choosers);

    /// <summary>A class's subjects, core first then by id.</summary>
    Task<List<ClassSubjectRow>> ClassSubjectsAsync(int classId, CancellationToken ct = default);
}
