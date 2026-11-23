using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Job applicants (EF Core).</summary>
public interface IJobApplicantRepository : IRepository<JobApplicant>
{
    Task<int> HiredCountAsync(int openingId, CancellationToken ct = default);

    Task<bool> AnyForOpeningAsync(int openingId, CancellationToken ct = default);

    /// <summary>Applicants (with their opening), newest first; only one opening's when given.</summary>
    Task<List<JobApplicant>> ListWithOpeningAsync(int? openingId, CancellationToken ct = default);
}
