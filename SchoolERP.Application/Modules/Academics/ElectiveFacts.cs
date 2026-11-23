namespace SchoolERP.Application.Modules.Academics;

/// <summary>What the exams need to know about a class: which of its subjects are electives, and who takes each.</summary>
public sealed class ElectiveFacts(HashSet<int> electives, HashSet<(int StudentId, int SubjectId)> taken)
{
    public HashSet<int> Electives { get; } = electives;

    /// <summary>A core subject is taken by everyone; an elective only by those who chose it.</summary>
    public bool Takes(int studentId, int subjectId) => !Electives.Contains(subjectId) || taken.Contains((studentId, subjectId));
}
