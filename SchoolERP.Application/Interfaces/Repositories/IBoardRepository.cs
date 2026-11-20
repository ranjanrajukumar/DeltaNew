using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Boards, their versioned rules and affiliation records.</summary>
public interface IBoardRepository : IRepository<Board>
{
    Task<Board?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>The board with the lowest id (what a class without a board follows).</summary>
    Task<Board?> FirstAsync(CancellationToken ct = default);

    /// <summary>The active versions of one rule (board, type, scope), read-only.</summary>
    Task<List<BoardRule>> ActiveRulesAsync(int boardId, string ruleType, string scopeKey, CancellationToken ct = default);

    /// <summary>Every version of one rule for one effective session, tracked (for retiring the active one).</summary>
    Task<List<BoardRule>> RuleVersionsAsync(int boardId, string ruleType, string scopeKey, int? effectiveYearId, CancellationToken ct = default);

    /// <summary>A rule version by id, tracked.</summary>
    Task<BoardRule?> FindRuleAsync(int id, CancellationToken ct = default);

    /// <summary>Every rule version of a board, by type, scope, session and then newest version first.</summary>
    Task<List<BoardRule>> RulesOfBoardAsync(int boardId, CancellationToken ct = default);

    void AddRule(BoardRule rule);

    /// <summary>A board's affiliation records marked verified.</summary>
    Task<List<BoardAffiliation>> VerifiedAffiliationsAsync(int boardId, CancellationToken ct = default);
}
