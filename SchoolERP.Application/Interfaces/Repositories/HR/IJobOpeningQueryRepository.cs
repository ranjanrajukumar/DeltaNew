using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Job openings with applicant counts (Dapper).</summary>
public interface IJobOpeningQueryRepository
{
    public sealed record OpeningRow(
        int Id, string Title, int? DepartmentId, string? DepartmentName, int Vacancies, string? Description, DateOnly? ClosingDate,
        string Status, int Applicants, int Hired);

    /// <summary>Every opening, newest first, with its department name and applicant / hired counts.</summary>
    Task<List<OpeningRow>> ListAsync(CancellationToken ct = default);
}
