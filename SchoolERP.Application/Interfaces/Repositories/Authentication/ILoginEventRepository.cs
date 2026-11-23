using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Sign-in history (EF Core; hard-deleted).</summary>
public interface ILoginEventRepository : IRepository<LoginEvent>
{
    /// <summary>Deletes (immediately, without SaveChanges) every event created before <paramref name="cutoff"/>.</summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken ct = default);
}
