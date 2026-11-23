using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

namespace SchoolERP.Infrastructure.Registrations;

/// <summary>Attendance, monthly registers, student leave, leave types.</summary>
internal static class AttendanceRepositoryRegistrations
{
    public static IServiceCollection AddAttendanceRepositories(this IServiceCollection services)
    {
        // EF Core
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IPeriodAttendanceRepository, PeriodAttendanceRepository>();
        services.AddScoped<IAbsenceAlertRepository, AbsenceAlertRepository>();
        services.AddScoped<IStudentLeaveRepository, StudentLeaveRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<ITeacherScopeRepository, TeacherScopeRepository>();

        // Dapper
        services.AddScoped<IAttendanceQueryRepository, AttendanceQueryRepository>();
        services.AddScoped<ILeaveBalanceQueryRepository, LeaveBalanceQueryRepository>();

        return services;
    }
}
