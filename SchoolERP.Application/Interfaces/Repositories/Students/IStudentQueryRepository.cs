using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Student counts for the dashboard tiles (Dapper).</summary>
public interface IStudentQueryRepository
{
    public sealed record RegistrationSummaryRow(int Total, int Complete);

    public sealed record SummaryRow(int Total, int Active, int AddedThisMonth, int PendingEnrollment, int Classes);

    Task<RegistrationSummaryRow> RegistrationSummaryAsync(CancellationToken ct = default);

    /// <summary>Counts, with students added since <paramref name="monthStart"/> (UTC).</summary>
    Task<SummaryRow> SummaryAsync(DateTime monthStart, CancellationToken ct = default);
}
