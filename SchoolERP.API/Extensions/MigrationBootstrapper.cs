using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.API.Extensions;

/// <summary>
/// Development helper: when the Infrastructure project has no migrations yet, scaffolds InitialCreate into
/// SchoolERP.Infrastructure/Persistence/Migrations with the same EF Core code Add-Migration uses, so the first run needs no
/// command-line tools. The new files are compiled on the next build; restart once to apply them.
/// Later schema changes: Add-Migration &lt;Name&gt; -Project SchoolERP.Infrastructure -StartupProject SchoolERP.API -OutputDir Persistence/Migrations
/// </summary>
public static class MigrationBootstrapper
{
    /// <summary>True when a migration was just written and the app should be restarted to apply it.</summary>
    public static bool ScaffoldInitialMigrationIfMissing(IServiceProvider services, IWebHostEnvironment env, ILogger logger)
    {
        if (!env.IsDevelopment()) return false;
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.GetMigrations().Any()) return false;

        var projectDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "SchoolERP.Infrastructure"));
        if (!File.Exists(Path.Combine(projectDir, "SchoolERP.Infrastructure.csproj")))
        {
            logger.LogError("No migrations exist and the Infrastructure project was not found at {Dir}; run Add-Migration InitialCreate instead.", projectDir);
            return false;
        }

        var designServices = new ServiceCollection();
        designServices.AddEntityFrameworkDesignTimeServices();
        designServices.AddDbContextDesignTimeServices(db);
#pragma warning disable EF1001 // the SQL Server design-time services are what dotnet-ef loads too
        new SqlServerDesignTimeServices().ConfigureDesignTimeServices(designServices);
#pragma warning restore EF1001
        using var provider = designServices.BuildServiceProvider();
        var scaffolder = provider.GetRequiredService<IMigrationsScaffolder>();
        var migration = scaffolder.ScaffoldMigration("InitialCreate", "SchoolERP.Infrastructure", "Persistence.Migrations", "C#");
        var files = scaffolder.Save(projectDir, migration, Path.Combine(projectDir, "Persistence", "Migrations"));

        logger.LogWarning(
            "Created migration InitialCreate ({File}). Stop and start the API again (F5) to build it in and create the database.",
            files.MigrationFile);
        return true;
    }
}
