using SchoolERP.Application.Interfaces.Services;

namespace SchoolERP.API.BackgroundJobs;

/// <summary>
/// Checks every five minutes whether today's automatic absence alerts are due (the Node app's startAbsenceAlertTimer).
/// The switch and the cut-off time are settings; nothing is sent while they are off or before the time.
/// </summary>
public sealed class AbsenceAlertTimer(IServiceScopeFactory scopes, ILogger<AbsenceAlertTimer> logger) : BackgroundService
{
    private static readonly TimeSpan Every = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Every);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IAttendanceExtrasService>().RunAutoAlertsAsync(ct: stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError("Absence alerts failed: {Message}", ex.Message);
            }
        }
    }
}
