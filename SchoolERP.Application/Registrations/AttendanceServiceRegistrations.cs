using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;

namespace SchoolERP.Application.Registrations;

/// <summary>Attendance, monthly registers, student leave, period attendance, absence alerts, leave types.</summary>
internal static class AttendanceServiceRegistrations
{
    public static IServiceCollection AddAttendanceServices(this IServiceCollection services)
    {
        services.AddScoped<ITeacherScope, TeacherScope>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IMonthlyRegisterService, MonthlyRegisterService>();
        services.AddScoped<IStudentLeaveService, StudentLeaveService>();
        services.AddScoped<IAttendanceExtrasService, AttendanceExtrasService>();
        services.AddScoped<ILeaveTypesService, LeaveTypesService>();


        return services;
    }
}
