using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>A board's grading policy (the Node app's exams/grading.js).</summary>
public interface IGradingService
{
    /// <summary>The grading policy of a board in an academic session (default: the current one).</summary>
    Task<GradingPolicy> LoadAsync(Board board, int? yearId = null, CancellationToken ct = default);

    /// <summary>A board by name or id ("CBSE" when not given).</summary>
    Task<GradingPolicy> LoadAsync(string? boardRef, CancellationToken ct = default);

    /// <summary>The grading policy of the board a class follows.</summary>
    Task<GradingPolicy> ForClassAsync(int? classId, CancellationToken ct = default);
}
