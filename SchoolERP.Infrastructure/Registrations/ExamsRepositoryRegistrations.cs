using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

namespace SchoolERP.Infrastructure.Registrations;

/// <summary>Exams, marks, grading, progress cards, promotion: EF Core repositories for writes, Dapper for lists and summaries.</summary>
internal static class ExamsRepositoryRegistrations
{
    public static IServiceCollection AddExamsRepositories(this IServiceCollection services)
    {
        // EF Core
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IMarkRepository, MarkRepository>();
        services.AddScoped<IExamGroupRepository, ExamGroupRepository>();
        services.AddScoped<IRevaluationRepository, RevaluationRepository>();
        services.AddScoped<ICoScholasticRepository, CoScholasticRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();

        // Dapper
        services.AddScoped<IExamQueryRepository, ExamQueryRepository>();

        return services;
    }
}
