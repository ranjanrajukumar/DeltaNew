using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Syllabus progress per class and subject (Dapper).</summary>
public interface ISyllabusQueryRepository
{
    public sealed record ProgressRow(
        int ClassId, int SubjectId, string ClassName, string SubjectName, int Total, int Completed, int InProgress, int Behind);

    Task<List<ProgressRow>> ProgressAsync(DateOnly today, CancellationToken ct = default);
}
