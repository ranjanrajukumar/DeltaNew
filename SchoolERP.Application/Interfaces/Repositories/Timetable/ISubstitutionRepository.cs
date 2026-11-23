using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Cover for absent teachers (EF Core).</summary>
public interface ISubstitutionRepository : IRepository<Substitution>
{
    /// <summary>Staff ids with approved leave covering <paramref name="date"/>.</summary>
    Task<List<int>> OnLeaveStaffIdsAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>Active staff by first name (read-only).</summary>
    Task<List<Staff>> ActiveStaffByFirstNameAsync(CancellationToken ct = default);

    /// <summary>Active staff (read-only).</summary>
    Task<List<Staff>> ActiveStaffAsync(CancellationToken ct = default);

    /// <summary>Substitute ids already covering that period on that date.</summary>
    Task<List<int>> CoveringStaffIdsAsync(DateOnly? date, int period, CancellationToken ct = default);

    /// <summary>The date's cover with the substitute loaded (read-only).</summary>
    Task<List<Substitution>> OnDateAsync(DateOnly? date, CancellationToken ct = default);

    Task<bool> SlotCoveredAsync(int slotId, DateOnly? date, CancellationToken ct = default);

    /// <summary>Cover on or after <paramref name="since"/> (read-only).</summary>
    Task<List<Substitution>> SinceAsync(DateOnly since, CancellationToken ct = default);
}
