using Serilog;
using Serilog.Events;

namespace SchoolERP.API.Extensions;

/// <summary>Global logging: console plus a daily file in logs/, so startup failures can be read after the window closes.</summary>
public static class LoggingExtensions
{
    public static IHostBuilder UseSchoolErpLogging(this IHostBuilder host)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "logs", "api-.log"),
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
            .CreateLogger();
        return host.UseSerilog();
    }
}
