using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Persistence.Interceptors;

namespace SchoolERP.Infrastructure.Persistence;

/// <summary>
/// The school database (SQL Server). Table and column names are the Node app's own (snake_case), so the data and any
/// SQL written against it carry over. Every save stamps the audit columns and turns deletes into soft deletes; see <see cref="AuditSaveRules"/>.
/// </summary>
public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser)
    : DbContext(options)
{
    internal string AuditUserName => currentUser.IsAuthenticated ? currentUser.Name ?? "system" : "system";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Stored as UTC; read back marked as UTC so JSON carries the "Z" the Node API sent.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcConverter>();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        ModelValidation.Check(ChangeTracker); // the Sequelize models' validate: rules
        var rules = new AuditSaveRules(this);
        if (!rules.NeedsSqlWork())
        {
            rules.StampOnly();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        return await InTransactionAsync(async () =>
        {
            await rules.ApplyAsync(cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }, cancellationToken);
    }

    public async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) return await work();
        await using var tx = await Database.BeginTransactionAsync(cancellationToken);
        var result = await work();
        await tx.CommitAsync(cancellationToken);
        return result;
    }

    private sealed class UtcConverter() : ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private sealed class NullableUtcConverter() : ValueConverter<DateTime?, DateTime?>(
        v => v.HasValue && v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
}
