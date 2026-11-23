using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Academics;

namespace SchoolERP.Application.Registrations;

/// <summary>Academics: classes, subjects, calendar, homework, syllabus, online classes, timetable, scheduling, promotion.</summary>
internal static class AcademicsServiceRegistrations
{
    public static IServiceCollection AddAcademicsServices(this IServiceCollection services)
    {
        services.AddScoped<ISubjectMapService, SubjectMapService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<IClassSubjectService, ClassSubjectsService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IHomeworkService, HomeworkService>();
        services.AddScoped<IStudyMaterialService, StudyMaterialService>();
        services.AddScoped<ISyllabusService, SyllabusService>();
        services.AddScoped<IOnlineClassService, OnlineClassesService>();
        services.AddScoped<IMyClassService, MyClassService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<ISchedulingService, SchedulingService>();

        return services;
    }
}
