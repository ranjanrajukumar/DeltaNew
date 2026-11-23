using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class BoardRepository(ApplicationDbContext db) : Repository<Board>(db), IBoardRepository
{
    public Task<Board?> GetByNameAsync(string name, CancellationToken ct = default) => Read.FirstOrDefaultAsync(b => b.Name == name, ct);

    public Task<Board?> FirstAsync(CancellationToken ct = default) => Read.OrderBy(b => b.Id).FirstOrDefaultAsync(ct);

    public Task<List<BoardRule>> ActiveRulesAsync(int boardId, string ruleType, string scopeKey, CancellationToken ct = default) =>
        Db.Set<BoardRule>().AsNoTracking()
            .Where(r => r.BoardId == boardId && r.RuleType == ruleType && r.ScopeKey == scopeKey && r.Status == "active")
            .ToListAsync(ct);

    public Task<List<BoardRule>> RuleVersionsAsync(int boardId, string ruleType, string scopeKey, int? effectiveYearId, CancellationToken ct = default) =>
        Db.Set<BoardRule>()
            .Where(r => r.BoardId == boardId && r.RuleType == ruleType && r.ScopeKey == scopeKey && r.EffectiveYearId == effectiveYearId)
            .ToListAsync(ct);

    public Task<BoardRule?> FindRuleAsync(int id, CancellationToken ct = default) => Db.Set<BoardRule>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<BoardRule>> RulesOfBoardAsync(int boardId, CancellationToken ct = default) =>
        Db.Set<BoardRule>().AsNoTracking().Where(r => r.BoardId == boardId)
            .OrderBy(r => r.RuleType).ThenBy(r => r.ScopeKey).ThenBy(r => r.EffectiveYearId).ThenByDescending(r => r.Version)
            .ToListAsync(ct);

    public void AddRule(BoardRule rule) => Db.Set<BoardRule>().Add(rule);

    public Task<List<BoardAffiliation>> VerifiedAffiliationsAsync(int boardId, CancellationToken ct = default) =>
        Db.Set<BoardAffiliation>().AsNoTracking().Where(a => a.BoardId == boardId && a.Status == "verified").ToListAsync(ct);
}
