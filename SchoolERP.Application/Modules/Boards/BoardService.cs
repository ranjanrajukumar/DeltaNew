using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Boards;

/// <summary>Boards and their versioned rules (the Node app's boards/boardRules.service.js).</summary>
public sealed class BoardService(IBoardRepository boards, IAcademicYearRepository years, IClassRepository classes, IUnitOfWork unitOfWork) : IBoardService
{
    public async Task<Board?> FindAsync(JsonNode? reference, CancellationToken ct = default)
    {
        var text = Body.AsString(reference)?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        return Body.AsNumber(reference) is { } n && n == Math.Floor(n)
            ? await boards.GetAsync((int)n, ct)
            : await boards.GetByNameAsync(text, ct);
    }

    public async Task<Board?> OfClassAsync(SchoolClass? cls, CancellationToken ct = default)
    {
        Board? board = null;
        if (cls?.BoardId is { } id) board = await boards.GetAsync(id, ct);
        else if (cls is not null) board = await FindAsync(cls.Board, ct);
        return board ?? await boards.FirstAsync(ct);
    }

    /// <summary>
    /// The active version whose effective session has started, taking the latest start and then the highest version. A rule
    /// with no effective session applies from the beginning of time.
    /// </summary>
    public async Task<RuleHit?> ResolveRuleAsync(Board? board, string ruleType, string scopeKey = "default", int? yearId = null, CancellationToken ct = default)
    {
        if (board is null) return null;
        var rules = await boards.ActiveRulesAsync(board.Id, ruleType, scopeKey, ct);
        var year = yearId is { } y ? await years.GetAsync(y, ct) : await years.CurrentAsync(ct);
        DateOnly? start = year?.StartDate;
        var starts = await years.StartDatesAsync(ct);
        var hit = rules
            .Select(r => new { Rule = r, From = r.EffectiveYearId is { } e ? (starts.TryGetValue(e, out var s) ? s : (DateOnly?)null) : DateOnly.MinValue })
            .Where(x => x.From is not null && (start is null || x.From <= start))
            .OrderByDescending(x => x.From).ThenByDescending(x => x.Rule.Version)
            .Select(x => x.Rule)
            .FirstOrDefault();
        return hit is null ? null : new RuleHit(hit.Id, board.Id, hit.Version, hit.Verified, hit.SourceRef, hit.EffectiveYearId, JsonNode.Parse(hit.Payload));
    }

    public async Task<RuleHit> RequireRuleAsync(Board board, string ruleType, string scopeKey = "default", int? yearId = null, CancellationToken ct = default)
    {
        var rule = await ResolveRuleAsync(board, ruleType, scopeKey, yearId, ct);
        if (rule is not null) return rule;
        var scope = scopeKey != "default" ? $" ({scopeKey})" : "";
        throw HttpException.Conflict($"No {ReplaceFirst(ruleType, "_", " ")} rule is set for {board.Name}{scope}. Add one under Board rules");
    }

    /// <summary>Only a record marked verified, with its number and document, and still in date, produces the sentence.</summary>
    public async Task<string?> AffiliationStatementAsync(Board board, int? branchId = null, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rows = (await boards.VerifiedAffiliationsAsync(board.Id, ct))
            .Where(a => !string.IsNullOrEmpty(a.AffiliationNo) && !string.IsNullOrEmpty(a.SourceRef)
                && (a.ValidTo is null || a.ValidTo >= today) && (a.ValidFrom is null || a.ValidFrom <= today))
            .ToList();
        var hit = rows.FirstOrDefault(a => branchId is not null && a.BranchId == branchId) ?? rows.FirstOrDefault(a => a.BranchId is null);
        return hit is null ? null : $"Affiliated to {board.Name}, affiliation no. {hit.AffiliationNo}";
    }

    /// <summary>
    /// A new version of a rule; the version it replaces is retired and never edited. A rule is saved unverified; it becomes
    /// verified only through <see cref="VerifyRuleAsync"/>, which needs a source.
    /// </summary>
    public async Task<int> SaveRuleAsync(JsonNode? boardRef, string? ruleType, string scopeKey, int? effectiveYearId, JsonNode? payload,
        string? sourceRef, string? changeNote, string? userName, CancellationToken ct = default)
    {
        if (ruleType is null || !BoardRuleValidator.RuleTypes.Contains(ruleType)) throw HttpException.BadRequest("Unknown rule type");
        var board = await FindAsync(boardRef, ct) ?? throw HttpException.NotFound("Board not found");
        if (!board.IsEnabled) throw HttpException.BadRequest($"{board.Name} is switched off");
        var clean = BoardRuleValidator.ValidatePayload(ruleType, scopeKey, payload);
        if (effectiveYearId is { } y && !await years.ExistsAsync(y, ct)) throw HttpException.BadRequest("Academic session not found");
        static string? Cut(string? s) => (s ?? "").Trim() is { Length: > 0 } t ? (t.Length > 500 ? t[..500] : t) : null;
        return await unitOfWork.InTransactionAsync(async () =>
        {
            var versions = await boards.RuleVersionsAsync(board.Id, ruleType, scopeKey, effectiveYearId, ct);
            foreach (var r in versions.Where(r => r.Status == "active")) r.Status = "retired";
            var row = new BoardRule
            {
                BoardId = board.Id, RuleType = ruleType, ScopeKey = scopeKey, EffectiveYearId = effectiveYearId,
                Version = (versions.Count == 0 ? 0 : versions.Max(r => r.Version)) + 1,
                Payload = clean.ToJsonString(), SourceRef = Cut(sourceRef), ChangeNote = Cut(changeNote), CreatedByName = userName,
            };
            boards.AddRule(row);
            await unitOfWork.SaveChangesAsync(ct);
            return row.Id;
        }, ct);
    }

    public async Task VerifyRuleAsync(int id, string? sourceRef, string? userName, CancellationToken ct = default)
    {
        var row = await boards.FindRuleAsync(id, ct) ?? throw HttpException.NotFound("Rule not found");
        var reference = (sourceRef ?? row.SourceRef ?? "").Trim();
        if (reference == "") throw HttpException.BadRequest("Name the board publication or circular this was checked against");
        row.Verified = true;
        row.SourceRef = reference.Length > 500 ? reference[..500] : reference;
        row.VerifiedBy = userName;
        row.VerifiedOn = Clock.Today();
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<List<object>> ListRulesAsync(JsonNode? boardRef, CancellationToken ct = default)
    {
        var board = await FindAsync(boardRef, ct) ?? throw HttpException.NotFound("Board not found");
        var rows = await boards.RulesOfBoardAsync(board.Id, ct);
        var names = await years.NamesAsync(ct);
        return rows.Select(r => (object)new
        {
            id = r.Id,
            ruleType = r.RuleType,
            scopeKey = r.ScopeKey,
            effectiveYearId = r.EffectiveYearId,
            effectiveFrom = r.EffectiveYearId is { } y ? names.GetValueOrDefault(y) : null,
            version = r.Version,
            status = r.Status,
            verified = r.Verified,
            sourceRef = r.SourceRef,
            changeNote = r.ChangeNote,
            createdByName = r.CreatedByName,
            verifiedBy = r.VerifiedBy,
            verifiedOn = r.VerifiedOn,
            editedAt = r.Addondt,
            payload = JsonNode.Parse(r.Payload),
        }).ToList();
    }

    public async Task AssertSameBoardAsync(int? fromClassId, int? toClassId, bool allowCross, CancellationToken ct = default)
    {
        if (fromClassId is not { } fromId || toClassId is not { } toId || allowCross) return;
        var a = await classes.GetAsync(fromId, ct);
        var b = await classes.GetAsync(toId, ct);
        if (a is null || b is null) return;
        var ba = await OfClassAsync(a, ct);
        var bb = await OfClassAsync(b, ct);
        if (ba is not null && bb is not null && ba.Id != bb.Id)
            throw HttpException.Conflict(
                $"{a.Name}-{a.Section} follows {ba.Name} and {b.Name}-{b.Section} follows {bb.Name}. Moving a student between boards must be confirmed (allowCrossBoard)");
    }

    /// <summary>JavaScript's String.replace with a string pattern: only the first match.</summary>
    private static string ReplaceFirst(string text, string find, string with)
    {
        var i = text.IndexOf(find, StringComparison.Ordinal);
        return i < 0 ? text : text[..i] + with + text[(i + find.Length)..];
    }
}
