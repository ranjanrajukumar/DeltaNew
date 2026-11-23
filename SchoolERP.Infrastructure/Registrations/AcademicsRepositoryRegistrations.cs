using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

namespace SchoolERP.Infrastructure.Registrations;

/// <summary>Academics: EF Core repositories for writes, Dapper query repositories for lists and reports.</summary>
internal static class AcademicsRepositoryRegistrations
{
    public static IServiceCollection AddAcademicsRepositories(this IServiceCollection services)
    {
        // EF Core
        services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<ISubjectMapRepository, SubjectMapRepository>();
        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<IHomeworkRepository, HomeworkRepository>();
        services.AddScoped<IStudyMaterialRepository, StudyMaterialRepository>();
        services.AddScoped<ISyllabusRepository, SyllabusRepository>();
        services.AddScoped<IStudentLookupRepository, StudentLookupRepository>();
        services.AddScoped<IOnlineClassRepository, OnlineClassRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IClassSubjectRepository, ClassSubjectRepository>();
        services.AddScoped<ITimetableRepository, TimetableRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ISubstitutionRepository, SubstitutionRepository>();

        // Dapper
        services.AddScoped<IClassQueryRepository, ClassQueryRepository>();
        services.AddScoped<ISubjectMapQueryRepository, SubjectMapQueryRepository>();
        services.AddScoped<IHomeworkQueryRepository, HomeworkQueryRepository>();
        services.AddScoped<ISyllabusQueryRepository, SyllabusQueryRepository>();
        services.AddScoped<IClassSubjectQueryRepository, ClassSubjectQueryRepository>();
        services.AddScoped<IMyClassQueryRepository, MyClassQueryRepository>();
        services.AddScoped<ITimetableQueryRepository, TimetableQueryRepository>();
        services.AddScoped<ISchedulingQueryRepository, SchedulingQueryRepository>();

        return services;
    }
}
