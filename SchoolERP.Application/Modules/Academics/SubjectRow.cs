namespace SchoolERP.Application.Modules.Academics;

/// <summary>One subject of a class's subject map.</summary>
public sealed record SubjectRow(
    int Id, int SubjectId, string? Name, string? Code, string Component, string? ComponentLabel, bool IsElective,
    string? ElectiveGroup, int? PeriodsPerWeek, int? PracticalShare, int? StaffId, string? TeacherName, int Takers);
