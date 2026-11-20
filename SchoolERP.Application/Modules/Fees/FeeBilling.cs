using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Fees;

/// <summary>Billing students for fee items (billStudents in the Node app's finance/fees.service.js; the rest moves here with the Fees module).</summary>
public sealed class FeeBilling(IApplicationDbContext db)
{
    /// <summary>
    /// Bills the given students for one fee item. A student who already has a fee with the same title is skipped,
    /// so running a generation twice never double-bills.
    /// </summary>
    public async Task<(int Created, int Skipped)> BillStudentsAsync(IReadOnlyCollection<int> studentIds, string title, decimal amount, DateOnly? dueDate, CancellationToken ct = default)
    {
        var billed = await db.Set<Fee>()
            .Where(f => f.Title == title && studentIds.Contains(f.StudentId))
            .Select(f => f.StudentId)
            .ToListAsync(ct);
        var rows = studentIds.Where(id => !billed.Contains(id))
            .Select(id => new Fee { StudentId = id, Title = title, Amount = amount, DueDate = dueDate })
            .ToList();
        if (rows.Count > 0)
        {
            db.Set<Fee>().AddRange(rows);
            await db.SaveChangesAsync(ct);
        }
        return (rows.Count, studentIds.Count - rows.Count);
    }
}
