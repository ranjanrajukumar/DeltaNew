using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

namespace SchoolERP.Infrastructure.Registrations;

/// <summary>Staff, HR, payroll, recruitment, reviews, files.</summary>
internal static class HrRepositoryRegistrations
{
    public static IServiceCollection AddHrRepositories(this IServiceCollection services)
    {
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IStaffAttendanceRepository, StaffAttendanceRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IStaffSalaryRepository, StaffSalaryRepository>();
        services.AddScoped<IPayslipRepository, PayslipRepository>();
        services.AddScoped<IJobOpeningRepository, JobOpeningRepository>();
        services.AddScoped<IJobApplicantRepository, JobApplicantRepository>();
        services.AddScoped<IPerformanceReviewRepository, PerformanceReviewRepository>();
        services.AddScoped<IStudentFileRepository, StudentFileRepository>();
        services.AddScoped<IStaffFileRepository, StaffFileRepository>();

        services.AddScoped<IDepartmentQueryRepository, DepartmentQueryRepository>();
        services.AddScoped<IStaffAttendanceQueryRepository, StaffAttendanceQueryRepository>();
        services.AddScoped<IJobOpeningQueryRepository, JobOpeningQueryRepository>();

        return services;
    }
}
