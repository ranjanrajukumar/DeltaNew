using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Calendar events (EF Core).</summary>
public interface ICalendarRepository : IRepository<CalendarEvent>
{
    /// <summary>Events overlapping [from, to]; only these audiences when given.</summary>
    Task<List<CalendarEvent>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences, CancellationToken ct = default);

    /// <summary>Activities that are not cancelled overlapping [from, to]; only those open to every class when <paramref name="schoolWideOnly"/>.</summary>
    Task<List<Activity>> ActivitiesBetweenAsync(DateOnly from, DateOnly to, bool schoolWideOnly, CancellationToken ct = default);

    /// <summary>Holidays (for everyone or this audience) overlapping [from, to].</summary>
    Task<List<CalendarEvent>> HolidaysAsync(DateOnly from, DateOnly to, string audience, CancellationToken ct = default);
}

/// <summary>Homework and submissions (EF Core).</summary>
public interface IHomeworkRepository : IRepository<Homework>
{
    /// <summary>One homework with its class and subject.</summary>
    Task<Homework?> GetWithNamesAsync(int id, CancellationToken ct = default);

    /// <summary>The highest mark given for a homework, or null.</summary>
    Task<decimal?> HighestMarkAsync(int homeworkId, CancellationToken ct = default);

    Task<List<HomeworkSubmission>> SubmissionsAsync(int homeworkId, CancellationToken ct = default);

    /// <summary>Tracked submission of one student, or null.</summary>
    Task<HomeworkSubmission?> FindSubmissionAsync(int homeworkId, int studentId, CancellationToken ct = default);

    void AddSubmission(HomeworkSubmission submission);
}

/// <summary>Homework lists with names and counts (Dapper).</summary>
public interface IHomeworkQueryRepository
{
    public sealed record HomeworkRow(
        int Id, int ClassId, int SubjectId, string Title, string? Description, DateOnly AssignedOn, DateOnly DueDate, int? MaxMarks,
        string? CreatedByName, string ClassName, string SubjectName, int Submitted, int Total);

    public sealed record StudentHomeworkRow(
        int Id, int ClassId, int SubjectId, string Title, string? Description, DateOnly AssignedOn, DateOnly DueDate, int? MaxMarks,
        string? CreatedByName, string ClassName, string SubjectName, string? Status, decimal? Marks, string? Remarks);

    /// <summary>Newest due first, at most 300, with how many students handed it in.</summary>
    Task<List<HomeworkRow>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    /// <summary>A class's homework (newest due first, at most 100) with one student's own submission.</summary>
    Task<List<StudentHomeworkRow>> ForStudentAsync(int classId, int studentId, CancellationToken ct = default);
}

/// <summary>Study materials (EF Core).</summary>
public interface IStudyMaterialRepository : IRepository<StudyMaterial>
{
    /// <summary>Materials with class and subject loaded, newest first.</summary>
    Task<List<StudyMaterial>> ListWithNamesAsync(int? classId, int? subjectId, CancellationToken ct = default);
}

/// <summary>Syllabus topics (EF Core).</summary>
public interface ISyllabusRepository : IRepository<SyllabusTopic>
{
    /// <summary>Topics in teaching order.</summary>
    Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);
}

/// <summary>Syllabus progress per class and subject (Dapper).</summary>
public interface ISyllabusQueryRepository
{
    public sealed record ProgressRow(
        int ClassId, int SubjectId, string ClassName, string SubjectName, int Total, int Completed, int InProgress, int Behind);

    Task<List<ProgressRow>> ProgressAsync(DateOnly today, CancellationToken ct = default);
}

/// <summary>Online classes (EF Core).</summary>
public interface IOnlineClassRepository : IRepository<OnlineClass>
{
    /// <summary>With class and subject; "upcoming" (from <paramref name="today"/>, soonest first), "past" (newest first) or all; at most 300.</summary>
    Task<List<OnlineClass>> ListAsync(int? classId, string? when, DateOnly today, CancellationToken ct = default);

    Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default);

    Task<bool> SubjectExistsAsync(int subjectId, CancellationToken ct = default);
}

/// <summary>Subjects (EF Core).</summary>
public interface ISubjectRepository : IRepository<Subject>
{
    /// <summary>Every subject, tracked.</summary>
    Task<List<Subject>> ListTrackedAsync(CancellationToken ct = default);

    Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default);
}

/// <summary>Class–subject links (EF Core).</summary>
public interface IClassSubjectRepository : IRepository<ClassSubject>
{
    Task<bool> ExistsAsync(int classId, int subjectId, CancellationToken ct = default);

    /// <summary>Tracked link of a subject in a class, or null.</summary>
    Task<ClassSubject?> FindAsync(int classId, int subjectId, CancellationToken ct = default);

    /// <summary>The links of these classes (read-only).</summary>
    Task<List<ClassSubject>> OfClassesAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default);

    /// <summary>Every link that has a teacher (read-only).</summary>
    Task<List<ClassSubject>> WithTeacherAsync(CancellationToken ct = default);
}

/// <summary>Class–subject lists with names (Dapper).</summary>
public interface IClassSubjectQueryRepository
{
    public sealed record ClassSubjectRow(
        int Id, int ClassId, int SubjectId, int? StaffId, string Component, bool IsElective, string? ElectiveGroup,
        int? PeriodsPerWeek, int? PracticalShare, string ClassName, string SubjectName, string? TeacherName);

    /// <summary>Every assignment with class, subject and teacher names, by class then subject.</summary>
    Task<List<ClassSubjectRow>> ListAsync(CancellationToken ct = default);
}

/// <summary>A class teacher's view of their class today (Dapper).</summary>
public interface IMyClassQueryRepository
{
    public sealed record RollRow(int Id, string AdmissionNo, string FirstName, string LastName, string? GuardianName, string? GuardianPhone, string? Today);
    public sealed record LeaveRow(int StudentId, string Status, DateOnly FromDate, DateOnly ToDate);

    /// <summary>Active students by name with their mark on <paramref name="date"/>, and leave that is pending or approved and not over.</summary>
    Task<(List<RollRow> Roll, List<LeaveRow> Leaves)> TodayAsync(int classId, DateOnly date, CancellationToken ct = default);
}

/// <summary>Students as the academics services read them (EF Core).</summary>
public interface IStudentLookupRepository
{
    /// <summary>Active students of a class, by first name.</summary>
    Task<List<Student>> ActiveInClassAsync(int classId, CancellationToken ct = default);
}
