namespace SchoolERP.Contracts.Academics;

/// <summary>Fields accepted when creating or updating an academic year.</summary>
public sealed class AcademicYearRequest
{
    public string? Name { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}
