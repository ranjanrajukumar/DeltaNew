using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

public interface IAcademicYearRepository : IRepository<AcademicYear>
{
    /// <summary>Every year, latest start first.</summary>
    Task<List<AcademicYear>> ListByStartDescendingAsync(CancellationToken ct = default);

    /// <summary>The year marked current, if any.</summary>
    Task<AcademicYear?> CurrentAsync(CancellationToken ct = default);

    /// <summary>Tracked: every year marked current except this one (to clear the flag).</summary>
    Task<List<AcademicYear>> OtherCurrentYearsAsync(int exceptId, CancellationToken ct = default);

    Task<Dictionary<int, DateOnly>> StartDatesAsync(CancellationToken ct = default);

    Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default);
}
