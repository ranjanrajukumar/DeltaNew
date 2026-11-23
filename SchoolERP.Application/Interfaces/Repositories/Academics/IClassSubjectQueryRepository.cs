using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Class–subject lists with names (Dapper).</summary>
public interface IClassSubjectQueryRepository
{
    public sealed record ClassSubjectRow(
        int Id, int ClassId, int SubjectId, int? StaffId, string Component, bool IsElective, string? ElectiveGroup,
        int? PeriodsPerWeek, int? PracticalShare, string ClassName, string SubjectName, string? TeacherName);

    /// <summary>Every assignment with class, subject and teacher names, by class then subject.</summary>
    Task<List<ClassSubjectRow>> ListAsync(CancellationToken ct = default);
}
