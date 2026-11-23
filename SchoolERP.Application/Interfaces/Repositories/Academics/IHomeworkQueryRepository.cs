using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Homework lists with names and counts (Dapper).</summary>
public interface IHomeworkQueryRepository
{
    public sealed record HomeworkRow(
        int Id, int ClassId, int SubjectId, string Title, string? Description, DateOnly AssignedOn, DateOnly DueDate, int? MaxMarks,
        string? CreatedByName, string ClassName, string SubjectName, int Submitted, int Total);

    public sealed record StudentHomeworkRow(
        int Id, int ClassId, int SubjectId, string Title, string? Description, DateOnly AssignedOn, DateOnly DueDate, int? MaxMarks,
        string? CreatedByName, string ClassName, string SubjectName, string? Status, decimal? Marks, string? Remarks);

    /// <summary>Newest due first, at most 300, with how many students handed it in.</summary>
    Task<List<HomeworkRow>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    /// <summary>A class's homework (newest due first, at most 100) with one student's own submission.</summary>
    Task<List<StudentHomeworkRow>> ForStudentAsync(int classId, int studentId, CancellationToken ct = default);
}
