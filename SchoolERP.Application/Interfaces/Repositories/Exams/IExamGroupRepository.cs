using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Exam groups (EF Core).</summary>
public interface IExamGroupRepository : IRepository<ExamGroup>
{
    Task<bool> ExistsAsync(int classId, string? name, CancellationToken ct = default);

    /// <summary>A group with its class (read-only), or null.</summary>
    Task<ExamGroup?> GetWithClassAsync(int groupId, CancellationToken ct = default);

    /// <summary>A class's group of this name (read-only), or null.</summary>
    Task<ExamGroup?> GetAsync(int classId, string? name, CancellationToken ct = default);
}
