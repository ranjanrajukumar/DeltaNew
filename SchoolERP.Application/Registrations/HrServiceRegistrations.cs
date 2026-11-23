using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Registrations;

/// <summary>Staff, HR, payroll, recruitment, performance reviews, staff and student files.</summary>
internal static class HrServiceRegistrations
{
    public static IServiceCollection AddHrServices(this IServiceCollection services)
    {
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IHrService, HrService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();
        services.AddScoped<IReviewsService, ReviewsService>();
        services.AddScoped<IFilesService, FilesService>();

        return services;
    }
}
