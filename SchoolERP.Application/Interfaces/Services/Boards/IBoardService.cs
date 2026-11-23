using System.Text.Json.Nodes;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Boards and their versioned rules (the Node app's boards/boardRules.service.js).</summary>
public interface IBoardService
{
    /// <summary>A board by id or by name (the name is what a class shows).</summary>
    Task<Board?> FindAsync(JsonNode? reference, CancellationToken ct = default);

    /// <summary>The board of a class, or the first board when the class has none yet.</summary>
    Task<Board?> OfClassAsync(SchoolClass? cls, CancellationToken ct = default);

    /// <summary>The rule version in force for a board in an academic session (default: the current one); null when none.</summary>
    Task<RuleHit?> ResolveRuleAsync(Board? board, string ruleType, string scopeKey = "default", int? yearId = null, CancellationToken ct = default);

    /// <summary>Like <see cref="ResolveRuleAsync"/>, but a missing rule is a 409 telling the administrator what to add.</summary>
    Task<RuleHit> RequireRuleAsync(Board board, string ruleType, string scopeKey = "default", int? yearId = null, CancellationToken ct = default);

    /// <summary>The affiliation sentence a printed document may carry, or null.</summary>
    Task<string?> AffiliationStatementAsync(Board board, int? branchId = null, CancellationToken ct = default);

    /// <summary>Saves a new version of a rule and retires the one it replaces. Returns the new id.</summary>
    Task<int> SaveRuleAsync(JsonNode? boardRef, string? ruleType, string scopeKey, int? effectiveYearId, JsonNode? payload,
        string? sourceRef, string? changeNote, string? userName, CancellationToken ct = default);

    /// <summary>Marks a rule version as checked against the board's own publication.</summary>
    Task VerifyRuleAsync(int id, string? sourceRef, string? userName, CancellationToken ct = default);

    /// <summary>Every rule version of a board, for the admin screen and its history.</summary>
    Task<List<object>> ListRulesAsync(JsonNode? boardRef, CancellationToken ct = default);

    /// <summary>Refuses to move a student into a class of another board unless that is intended.</summary>
    Task AssertSameBoardAsync(int? fromClassId, int? toClassId, bool allowCross, CancellationToken ct = default);
}
