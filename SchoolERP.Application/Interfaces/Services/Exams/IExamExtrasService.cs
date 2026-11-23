using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Grade setup, the board exam pattern, exam groups, merit lists and revaluation (the Node app's exams/examExtras.service.js).</summary>
public interface IExamExtrasService
{
    Task SaveGradingAsync(JsonObject? json, CancellationToken ct = default);

    Task<object> GradingJsonAsync(string? board, CancellationToken ct = default);

    Task<object> ApplyExamPatternAsync(JsonNode? classIdsNode, CancellationToken ct = default);

    Task<List<object>> ListGroupsAsync(CancellationToken ct = default);

    Task<int> CreateGroupAsync(Body b, CancellationToken ct = default);

    Task DeleteGroupAsync(int id, CancellationToken ct = default);

    Task AssignExamAsync(Body b, CancellationToken ct = default);

    /// <summary>Combined result for a group: each student's exam percentages, averaged by the exams' weights.</summary>
    Task<ExamExtrasService.GroupResults> GroupResultsAsync(int groupId, CancellationToken ct = default);

    Task<object> MeritListAsync(int? examId, int? groupId, double? limit, CancellationToken ct = default);

    Task<List<JsonObject>> ListRevaluationsAsync(string? status, int? studentId, int? examId, CancellationToken ct = default);

    Task<int> RequestRevaluationAsync(Body b, int requestedBy, CancellationToken ct = default);

    Task DecideRevaluationAsync(int id, string decision, Body b, CancellationToken ct = default);
}
