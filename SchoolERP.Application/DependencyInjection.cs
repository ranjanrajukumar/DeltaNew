using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Registrations;

namespace SchoolERP.Application;

/// <summary>
/// Registers the application layer: the services that hold the business rules. Each module lists its own services in
/// Registrations/&lt;Module&gt;ServiceRegistrations.cs. Called from Program.cs as <c>builder.Services.AddApplication()</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCoreServices();       // settings, boards, auth, rights, students, messaging, generic CRUD
        services.AddAcademicsServices();  // classes, subjects, calendar, homework, timetable, scheduling, promotion
        services.AddAttendanceServices(); // attendance, monthly register, student leave, leave types
        services.AddExamsServices();      // exams, grading, progress cards
        services.AddHrServices();         // staff, HR, payroll, recruitment, reviews, files

        return services;
    }
}
