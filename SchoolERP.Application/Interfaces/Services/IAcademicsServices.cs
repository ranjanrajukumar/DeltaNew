using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>The school calendar (the Node app's academics/calendar.routes.js).</summary>
public interface ICalendarService
{
    /// <summary>Events and (read-only) activities overlapping [from, to]; <paramref name="audiences"/> narrows it for families.</summary>
    Task<List<JsonObject>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences = null, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Homework and who handed it in (the Node app's academics/homework.service.js).</summary>
public interface IHomeworkService
{
    Task<List<JsonObject>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<object> SubmissionsAsync(int id, CancellationToken ct = default);

    Task<int> SaveSubmissionsAsync(int id, JsonNode? records, CancellationToken ct = default);

    /// <summary>A child's view: homework for their class with their own submission status.</summary>
    Task<List<JsonObject>> ForStudentAsync(Student student, CancellationToken ct = default);
}

/// <summary>Links to study material per class and subject.</summary>
public interface IStudyMaterialService
{
    Task<List<JsonObject>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Live classes over a video link.</summary>
public interface IOnlineClassService
{
    Task<List<JsonObject>> ListAsync(int? classId, string? when, CancellationToken ct = default);

    Task<int> CreateAsync(JsonObject? body, CancellationToken ct = default);

    Task UpdateAsync(int id, JsonObject? body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>A class teacher's own class at a glance.</summary>
public interface IMyClassService
{
    /// <summary>Roll, today's attendance and leave waiting for a decision; null when the class does not exist.</summary>
    Task<object?> MyClassAsync(int classId, CancellationToken ct = default);
}

/// <summary>Which subjects each class studies.</summary>
public interface IClassSubjectService
{
    Task<List<object>> ListAsync(CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Gives each class its board's default subjects and periods per week.</summary>
    Task<object> ApplyTemplateAsync(JsonNode? classIds, CancellationToken ct = default);
}

/// <summary>Syllabus topics and how far each class has got.</summary>
public interface ISyllabusService
{
    Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    Task<List<object>> ProgressAsync(CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
