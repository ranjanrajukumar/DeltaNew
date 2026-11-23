using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.FileStorage;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.Notifications;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Persistence.Dapper;
using SchoolERP.Infrastructure.Persistence.Seed;
using SchoolERP.Infrastructure.Registrations;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

namespace SchoolERP.Infrastructure;

/// <summary>
/// Registers the infrastructure layer: EF Core (writes, tracking, transactions), Dapper (reports and complex reads),
/// the repositories of every module, and the outside world (files, e-mail/SMS, push, tokens).
/// Each module lists its repositories in Registrations/&lt;Module&gt;RepositoryRegistrations.cs.
/// Called from Program.cs as <c>builder.Services.AddInfrastructure(builder.Configuration)</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not set.");

        // ---------------------- Entity Framework Core (writes) ----------------------
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.MigrationsHistoryTable("__EFMigrationsHistory");
                })
                // Hard-delete tables (photos, logs, push subscriptions) have no "not deleted" filter while the rows they point at do;
                // that is the Node app's design, not a mistake, so the warning is noise.
                .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbErrorTranslator>();

        // ---------------------- Dapper (reads, reports) ----------------------
        DapperSetup.Configure();
        services.AddScoped<IDapperContext, DapperContext>();

        // ---------------------- Repositories ----------------------
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // generic CRUD for simple lookup tables
        services.AddCoreRepositories();
        services.AddAcademicsRepositories();
        services.AddAttendanceRepositories();
        services.AddExamsRepositories();
        services.AddHrRepositories();

        // ---------------------- Seeding ----------------------
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.Section));
        services.AddScoped<DatabaseSeeder>();

        // ---------------------- Identity ----------------------
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        var jwtSettings = configuration.GetSection(JwtSettings.Section).Get<JwtSettings>() ?? new JwtSettings();
        services.AddSingleton(jwtSettings);
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
        services.AddScoped<ICurrentUserSetter>(sp => sp.GetRequiredService<CurrentUser>());

        // ---------------------- Files, messaging, push ----------------------
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddHttpClient();
        services.AddScoped<IMessageDelivery, MessageDelivery>();
        services.AddScoped<IPushService, PushService>();

        return services;
    }
}
