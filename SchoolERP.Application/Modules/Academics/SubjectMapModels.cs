namespace SchoolERP.Application.Modules.Academics;

/// <summary>One subject of a class's subject map.</summary>
public sealed record SubjectRow(
    int Id, int SubjectId, string? Name, string? Code, string Component, string? ComponentLabel, bool IsElective,
    string? ElectiveGroup, int? PeriodsPerWeek, int? PracticalShare, int? StaffId, string? TeacherName, int Takers);

/// <summary>A class's subject map: each subject with its teacher, how it is taught and how many students take it.</summary>
public sealed record ClassMap(int ClassId, string ClassName, int Students, IReadOnlyList<SubjectRow> Subjects);

/// <summary>What the exams need to know about a class: which of its subjects are electives, and who takes each.</summary>
public sealed class ElectiveFacts(HashSet<int> electives, HashSet<(int StudentId, int SubjectId)> taken)
{
    public HashSet<int> Electives { get; } = electives;

    /// <summary>A core subject is taken by everyone; an elective only by those who chose it.</summary>
    public bool Takes(int studentId, int subjectId) => !Electives.Contains(subjectId) || taken.Contains((studentId, subjectId));
}
