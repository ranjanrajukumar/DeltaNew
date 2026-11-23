using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Exams, mark entry, results and report cards (the Node app's exams/exams.service.js).</summary>
public interface IExamsService
{
    Task<List<JsonObject>> ListAsync(CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>The exam (read-only); 404 when it does not exist.</summary>
    Task<Exam> LoadAsync(int id, CancellationToken ct = default);

    /// <summary>The subjects taught in the exam's class, in a stable order.</summary>
    Task<List<ExamsService.ExamSubject>> SubjectsAsync(Exam exam, CancellationToken ct = default);

    /// <summary>The mark-entry grid: every active student of the class against every subject of the class.</summary>
    Task<object> MarkSheetAsync(int examId, int userId, string? role, CancellationToken ct = default);

    /// <summary>Saves marks atomically; returns how many rows were sent.</summary>
    Task<int> SaveMarksAsync(int examId, JsonNode? recordsNode, int userId, string? role, CancellationToken ct = default);

    /// <summary>Totals, percentage, grade and rank for every student who has marks in the exam.</summary>
    Task<ExamsService.Results> ResultsAsync(int examId, GradingPolicy? policy = null, CancellationToken ct = default);

    Task<object> ResultsJsonAsync(int examId, CancellationToken ct = default);

    /// <summary>One student's report card: marks per subject, overall result, rank and attendance.</summary>
    Task<JsonObject> ReportCardAsync(int examId, int studentId, CancellationToken ct = default);
}
