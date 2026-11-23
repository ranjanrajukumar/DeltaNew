namespace SchoolERP.Contracts.Academics;

/// <summary>Academic year returned by the API.</summary>
public sealed record AcademicYearResponse(int Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsCurrent);
