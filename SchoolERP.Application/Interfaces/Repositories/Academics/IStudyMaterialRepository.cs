using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Study materials (EF Core).</summary>
public interface IStudyMaterialRepository : IRepository<StudyMaterial>
{
    /// <summary>Materials with class and subject loaded, newest first.</summary>
    Task<List<StudyMaterial>> ListWithNamesAsync(int? classId, int? subjectId, CancellationToken ct = default);
}
