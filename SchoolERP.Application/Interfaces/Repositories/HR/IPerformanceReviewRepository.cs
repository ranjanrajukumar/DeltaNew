using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Performance reviews (EF Core).</summary>
public interface IPerformanceReviewRepository : IRepository<PerformanceReview>
{
    /// <summary>Reviews with their staff (only those with a live staff record), latest review date first.</summary>
    Task<List<PerformanceReview>> ListWithStaffAsync(int? staffId, CancellationToken ct = default);
}
