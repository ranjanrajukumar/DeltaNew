using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Subjects (EF Core).</summary>
public interface ISubjectRepository : IRepository<Subject>
{
    /// <summary>Every subject, tracked.</summary>
    Task<List<Subject>> ListTrackedAsync(CancellationToken ct = default);

    Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default);
}
