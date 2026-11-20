using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Serilog;

using SchoolERP.API.BackgroundJobs;
using SchoolERP.API.Extensions;
using SchoolERP.API.Middleware;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Modules.Admissions;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Authentication;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Modules.Fees;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Infrastructure.FileStorage;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.Notifications;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Persistence.Dapper;
using SchoolERP.Infrastructure.Persistence.Seed;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------------------- GLOBAL LOGGING ----------------------
    builder.Host.UseSchoolErpLogging();

    // ---------------------- Controllers ----------------------
    // The JSON the Node API sent: camelCase, model-named nested objects, { "error": "..." } for failures.
    builder.Services.AddControllers()
        .AddJsonOptions(options => JsonDefaults.Configure(options.JsonSerializerOptions));
    builder.Services.Configure<ApiBehaviorOptions>(options =>
        options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(new { error = "Invalid request body" }));

    // ---------------------- Swagger ----------------------
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSchoolErpOpenApi();

    // ---------------------- DbContext ----------------------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not set.");
    builder.Services.AddDbContext<SchoolErpDbContext>(options =>
        options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(SchoolErpDbContext).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory");
            })
            // Hard-delete tables (photos, logs, push subscriptions) have no "not deleted" filter while the rows they point at do;
            // that is the Node app's design, not a mistake, so the warning is noise.
            .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
    builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SchoolErpDbContext>()); // being retired: modules move to repositories
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<DbErrorTranslator>();

    // Dapper
    builder.Services.AddScoped<IDapperContext, DapperContext>();

    // Seeding (create/upgrade the database and the first admin on start)
    builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.Section));
    builder.Services.AddScoped<DatabaseSeeder>();

    // ---------------------- Dependency Injection ----------------------

    // Repositories (database operations)
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IBoardRepository, BoardRepository>();
    builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
    builder.Services.AddScoped<IClassRepository, ClassRepository>();
    builder.Services.AddScoped<ISubjectMapRepository, SubjectMapRepository>();
    builder.Services.AddScoped<IStaffRepository, StaffRepository>();
    builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();
    builder.Services.AddScoped<IHomeworkRepository, HomeworkRepository>();
    builder.Services.AddScoped<IStudyMaterialRepository, StudyMaterialRepository>();
    builder.Services.AddScoped<ISyllabusRepository, SyllabusRepository>();
    builder.Services.AddScoped<IStudentLookupRepository, StudentLookupRepository>();

    // Query repositories (Dapper: complex reads, lists with joins and counts, reports)
    DapperSetup.Configure();
    builder.Services.AddScoped<IClassQueryRepository, ClassQueryRepository>();
    builder.Services.AddScoped<ISubjectMapQueryRepository, SubjectMapQueryRepository>();
    builder.Services.AddScoped<IHomeworkQueryRepository, HomeworkQueryRepository>();
    builder.Services.AddScoped<ISyllabusQueryRepository, SyllabusQueryRepository>();

    // Infrastructure
    builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
    builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();

    // Shared helpers
    builder.Services.AddScoped<SettingsReader>();
    builder.Services.AddScoped<RecordExtras>();
    builder.Services.AddScoped<IBoardService, BoardService>();
    builder.Services.AddScoped<FeeBilling>();

    // Authentication and access
    builder.Services.AddSingleton<LoginLimiter>();
    builder.Services.AddScoped<RightsService>();
    builder.Services.AddScoped<RequestAuthenticator>();
    builder.Services.AddScoped<AuthService>();

    // Generic list/create/update/delete (the Node app's crudRouter)
    builder.Services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));

    // Services (modules)
    // Academics
    builder.Services.AddScoped<ISubjectMapService, SubjectMapService>();
    builder.Services.AddScoped<IClassService, ClassService>();
    builder.Services.AddScoped<StudentsService>();
    builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
    builder.Services.AddScoped<ClassSubjectsService>();
    builder.Services.AddScoped<CalendarService>();
    builder.Services.AddScoped<HomeworkService>();
    builder.Services.AddScoped<StudyMaterialService>();
    builder.Services.AddScoped<SyllabusService>();
    builder.Services.AddScoped<OnlineClassesService>();
    builder.Services.AddScoped<MyClassService>();
    builder.Services.AddScoped<TimetableService>();
    builder.Services.AddScoped<SchedulingService>();
    builder.Services.AddScoped<PromotionService>();
    // Exams
    builder.Services.AddScoped<GradingService>();
    builder.Services.AddScoped<ExamsService>();
    builder.Services.AddScoped<ExamExtrasService>();
    builder.Services.AddScoped<ProgressService>();
    // Staff and HR
    builder.Services.AddScoped<StaffService>();
    builder.Services.AddScoped<HrService>();
    builder.Services.AddScoped<PayrollService>();
    builder.Services.AddScoped<RecruitmentService>();
    builder.Services.AddScoped<ReviewsService>();
    builder.Services.AddScoped<FilesService>();
    // Attendance and leave
    builder.Services.AddScoped<TeacherScope>();
    builder.Services.AddScoped<AttendanceService>();
    builder.Services.AddScoped<MonthlyRegisterService>();
    builder.Services.AddScoped<StudentLeaveService>();
    builder.Services.AddScoped<AttendanceExtrasService>();
    builder.Services.AddScoped<LeaveTypesService>();
    // Messaging and notifications
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<MessagingService>();
    builder.Services.AddScoped<IMessageDelivery, MessageDelivery>();
    builder.Services.AddScoped<IPushService, PushService>();

    // ---------------------- Background jobs ----------------------
    builder.Services.AddHostedService<AbsenceAlertTimer>();

    // User Context (Token + Claims)
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<CurrentUser>();
    builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
    builder.Services.AddScoped<ICurrentUserSetter>(sp => sp.GetRequiredService<CurrentUser>());

    // ---------------------- CORS ----------------------
    // Cors:ClientOrigins in appsettings; when empty, any origin.
    var origins = builder.Configuration.GetSection("Cors:ClientOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options => options.AddPolicy("ClientApp", policy =>
    {
        if (origins.Length > 0) policy.WithOrigins(origins);
        else policy.AllowAnyOrigin();
        policy.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Retry-After", "Content-Disposition");
    }));

    // ---------------------- JWT Settings ----------------------
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.Section).Get<JwtSettings>() ?? new JwtSettings();
    if (jwtSettings.SecretKey.Length < 32)
        throw new InvalidOperationException(
            "Set JwtSettings:SecretKey to a private random value of at least 32 characters (appsettings or environment variable JwtSettings__SecretKey).");

    // Token Service
    builder.Services.AddSingleton(jwtSettings);
    builder.Services.AddSingleton<ITokenService, JwtTokenService>();

    // ---------------------- JWT Authentication ----------------------
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // keep the token's own claim names: id, name, role
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = jwtSettings.SigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

            ClockSkew = TimeSpan.Zero,
        };
    });
    builder.Services.AddAuthorization();

    var app = builder.Build();

    // First Development run without migrations: write InitialCreate and stop, so the next build includes it.
    if (MigrationBootstrapper.ScaffoldInitialMigrationIfMissing(app.Services, app.Environment, app.Logger))
        return;

    // Create or upgrade the database, then the first-run data (the Node app did both on every start).
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().RunAsync();

    // ---------------------- Middleware Pipeline ----------------------
    app.UseMiddleware<ErrorHandlingMiddleware>(); // any exception → { "error": ... }

    //if (app.Environment.IsDevelopment())
    //{
        app.UseSchoolErpSwagger(); // /swagger and /openapi/v1.json
    //}

    app.UseHttpsRedirection();

    app.UseCors("ClientApp");

    app.UseAuthentication();
    app.UseMiddleware<UserContextMiddleware>(); // ✅ Token + Claims → current user
    app.UseAuthorization();
    app.UseMiddleware<ApiAccessMiddleware>();   // role rules per area + audit log

    app.MapControllers();
    app.MapFallback("/api/{**path}", () => Results.Json(new { error = "Not found" }, JsonDefaults.Options, statusCode: 404));

    Log.Information("SchoolERP API started");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException) // HostAbortedException: dotnet-ef stopping the host on purpose
{
    Log.Fatal(ex, "SchoolERP API failed to start");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
