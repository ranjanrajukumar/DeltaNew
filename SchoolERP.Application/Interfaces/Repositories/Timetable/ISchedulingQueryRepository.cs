using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Cover lists with names (Dapper).</summary>
public interface ISchedulingQueryRepository
{
    public sealed record SubstitutionRow(
        int Id, DateOnly Date, int Period, string ClassName, string SubjectName, string? TeacherName, string SubstituteName, string? Reason);

    /// <summary>Cover from <paramref name="from"/> (to <paramref name="to"/> when given), newest first.</summary>
    Task<List<SubstitutionRow>> SubstitutionsAsync(DateOnly from, DateOnly? to, CancellationToken ct = default);
}
