using Microsoft.AspNetCore.Mvc;
using Serilog;

using SchoolERP.API.BackgroundJobs;
using SchoolERP.API.Extensions;
using SchoolERP.API.Middleware;
using SchoolERP.Application;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Infrastructure;
using SchoolERP.Infrastructure.Persistence.Seed;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------------------- Logging (Serilog) ----------------------
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

    // ---------------------- Application DI (services, business rules) ----------------------
    builder.Services.AddApplication();

    // ---------------------- Infrastructure DI (EF Core, Dapper, repositories, identity, messaging) ----------------------
    builder.Services.AddInfrastructure(builder.Configuration);

    // ---------------------- Background jobs ----------------------
    builder.Services.AddHostedService<AbsenceAlertTimer>();

    // ---------------------- User context, CORS, JWT ----------------------
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddClientCors(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    var app = builder.Build();

    // First Development run without migrations: write InitialCreate and stop, so the next build includes it.
    if (MigrationBootstrapper.ScaffoldInitialMigrationIfMissing(app.Services, app.Environment, app.Logger))
        return;

    // Create or upgrade the database, then the first-run data (the Node app did both on every start).
    using (var scope = app.Services.CreateScope())
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().RunAsync();

    // ---------------------- Middleware Pipeline ----------------------
    app.UseMiddleware<ErrorHandlingMiddleware>(); // any exception â†’ { "error": ... }

    //if (app.Environment.IsDevelopment())
    //{
        app.UseSchoolErpSwagger(); // /swagger and /openapi/v1.json
    //}

    app.UseHttpsRedirection();

    app.UseCors("ClientApp");

    app.UseAuthentication();
    app.UseMiddleware<UserContextMiddleware>(); // âœ… Token + Claims â†’ current user
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
