using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Syllabus topics (EF Core).</summary>
public interface ISyllabusRepository : IRepository<SyllabusTopic>
{
    /// <summary>Topics in teaching order.</summary>
    Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);
}
