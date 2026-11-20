using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Domain.Common;

// The SQL below is built only from generated table/column names (EntityMetadata) and integer ids; values are passed as parameters.
#pragma warning disable EF1002

namespace SchoolERP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// What every save does, ported from the Node app's models/auditSoftDelete.js:
///  - new rows get authadd/addondt, changed rows authedit/editondt (and created/updated timestamps where the table has them);
///  - a delete is a soft delete (delstatus = 1 with authdel/delondt), and everything the database would have deleted, refused
///    or detached is handled the same way: CASCADE children are soft-deleted too, SET NULL children are detached, and RESTRICT
///    children that are still live refuse the delete (409);
///  - a new row that needs a unique value only a soft-deleted row still holds removes that deleted row for good first;
///  - tables marked <see cref="IHardDelete"/> are really deleted; tables with their own deleted-at column only get it stamped.
/// The foreign keys in SQL Server are all NO ACTION, so these rules (not the database) decide what a delete does, as in Node.
/// </summary>
internal sealed class AuditSaveRules(SchoolErpDbContext db)
{
    private readonly List<EntityEntry<AuditableEntity>> _entries = db.ChangeTracker.Entries<AuditableEntity>()
        .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
        .ToList();

    private readonly DateTime _now = DateTime.UtcNow;
    private string User => db.AuditUserName;

    public bool NeedsSqlWork() => _entries.Any(e =>
        (e.State == EntityState.Deleted && e.Entity is not IHardDelete)
        || (e.State == EntityState.Added && EntityMetadata.UniqueSets.ContainsKey(e.Entity.GetType())));

    public void StampOnly()
    {
        foreach (var e in _entries) Stamp(e);
    }

    public async Task ApplyAsync(CancellationToken ct)
    {
        foreach (var e in _entries)
        {
            switch (e.State)
            {
                case EntityState.Added:
                    Stamp(e);
                    await PurgeDeletedHoldingAsync(e, ct);
                    break;
                case EntityState.Modified:
                    Stamp(e);
                    break;
                case EntityState.Deleted when e.Entity is IHardDelete:
                    break;
                case EntityState.Deleted when e.Entity is IHasDeletedAt own:
                    e.State = EntityState.Modified;
                    own.DeletedAt = _now;
                    MarkDeleted(e.Entity);
                    break;
                case EntityState.Deleted:
                    var id = PrimaryKeyValue(e);
                    if (id is int intId) await SoftDeleteChildrenAsync(e.Entity.GetType(), [intId], ct);
                    e.State = EntityState.Modified;
                    MarkDeleted(e.Entity);
                    break;
            }
        }
    }

    private void Stamp(EntityEntry<AuditableEntity> e)
    {
        var entity = e.Entity;
        if (e.State == EntityState.Added)
        {
            entity.Authadd ??= User;
            entity.Addondt ??= _now;
            if (entity is IHasCreatedAt c && c.CreatedAt == default) c.CreatedAt = _now;
            if (entity is IHasUpdatedAt u) u.UpdatedAt = _now;
        }
        else if (e.State == EntityState.Modified)
        {
            entity.Authedit = User;
            entity.Editondt = _now;
            if (entity is IHasUpdatedAt u) u.UpdatedAt = _now;
        }
    }

    private void MarkDeleted(AuditableEntity entity)
    {
        entity.Delstatus = true;
        entity.Delondt = _now;
        entity.Authdel = User;
    }

    private static object? PrimaryKeyValue(EntityEntry e)
    {
        var key = e.Metadata.FindPrimaryKey();
        return key is { Properties.Count: 1 } ? e.Property(key.Properties[0].Name).CurrentValue : null;
    }

    /// <summary>Soft-deletes, detaches or refuses for every child of the parent rows being deleted (recursively for CASCADE).</summary>
    private async Task SoftDeleteChildrenAsync(Type parentType, IReadOnlyList<int> parentIds, CancellationToken ct)
    {
        if (parentIds.Count == 0 || !EntityMetadata.DeleteRules.TryGetValue(parentType, out var rules)) return;
        var idList = string.Join(",", parentIds);
        foreach (var rule in rules)
        {
            var childType = db.Model.FindEntityType(rule.ChildType)!;
            var paranoid = typeof(IHasDeletedAt).IsAssignableFrom(rule.ChildType);
            var deletedAtColumn = paranoid ? childType.FindProperty(nameof(IHasDeletedAt.DeletedAt))?.GetColumnName() ?? "deleted_at" : null;
            var live = rule.ChildHardDeletes ? "" : paranoid ? $" AND [{deletedAtColumn}] IS NULL" : " AND [delstatus] = 0";
            var where = $"[{rule.ForeignKeyColumn}] IN ({idList}){live}";

            switch (rule.Rule)
            {
                case DeleteRule.CASCADE when rule.ChildHardDeletes:
                    await db.Database.ExecuteSqlRawAsync($"DELETE FROM [{rule.ChildTable}] WHERE {where}", ct);
                    break;
                case DeleteRule.CASCADE:
                {
                    var pk = childType.FindPrimaryKey()!.Properties[0].GetColumnName();
                    var childIds = await db.Database
                        .SqlQueryRaw<int>($"SELECT [{pk}] AS [Value] FROM [{rule.ChildTable}] WHERE {where}")
                        .ToListAsync(ct);
                    if (childIds.Count == 0) break;
                    await SoftDeleteChildrenAsync(rule.ChildType, childIds, ct);
                    var setDeletedAt = paranoid ? $", [{deletedAtColumn}] = {{0}}" : "";
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE [{rule.ChildTable}] SET [delstatus] = 1, [delondt] = {{0}}, [authdel] = {{1}}{setDeletedAt} WHERE [{pk}] IN ({string.Join(",", childIds)})",
                        [_now, User], ct);
                    break;
                }
                case DeleteRule.SET_NULL:
                    await db.Database.ExecuteSqlRawAsync($"UPDATE [{rule.ChildTable}] SET [{rule.ForeignKeyColumn}] = NULL WHERE {where}", ct);
                    break;
                case DeleteRule.RESTRICT:
                    var count = await db.Database
                        .SqlQueryRaw<int>($"SELECT COUNT(*) AS [Value] FROM [{rule.ChildTable}] WHERE {where}")
                        .SingleAsync(ct);
                    if (count > 0)
                        throw new StillReferencedException($"{rule.ChildTable}.{rule.ForeignKeyColumn} still refers to {EntityMetadata.TableNames[parentType]}");
                    break;
            }
        }
    }

    /// <summary>A deleted row still holds its unique values; when a new row needs one, that deleted row is removed for good first.</summary>
    private async Task PurgeDeletedHoldingAsync(EntityEntry<AuditableEntity> e, CancellationToken ct)
    {
        var type = e.Entity.GetType();
        if (!EntityMetadata.UniqueSets.TryGetValue(type, out var sets)) return;
        var table = EntityMetadata.TableNames[type];
        foreach (var columns in sets)
        {
            var values = new List<object>();
            foreach (var column in columns)
            {
                var prop = e.Properties.FirstOrDefault(p => p.Metadata.GetColumnName() == column);
                if (prop?.CurrentValue is null) { values.Clear(); break; }
                values.Add(prop.CurrentValue);
            }
            if (values.Count != columns.Length) continue;
            var conditions = string.Join(" AND ", columns.Select((c, i) => $"[{c}] = {{{i}}}"));
            var tx = db.Database.CurrentTransaction!;
            await tx.CreateSavepointAsync("purge", ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}] WHERE [delstatus] = 1 AND {conditions}", values, ct);
                await tx.ReleaseSavepointAsync("purge", ct);
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                // still referenced by something: the insert will report the duplicate
                await tx.RollbackToSavepointAsync("purge", ct);
            }
        }
    }
}
