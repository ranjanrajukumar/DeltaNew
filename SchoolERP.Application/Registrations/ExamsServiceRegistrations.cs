using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Modules.Exams;

namespace SchoolERP.Application.Registrations;

/// <summary>Exams, marks, results, grading, progress cards, promotion.</summary>
internal static class ExamsServiceRegistrations
{
    public static IServiceCollection AddExamsServices(this IServiceCollection services)
    {
        services.AddScoped<IGradingService, GradingService>();
        services.AddScoped<IExamsService, ExamsService>();
        services.AddScoped<IExamExtrasService, ExamExtrasService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IPromotionService, PromotionService>();

        return services;
    }
}
