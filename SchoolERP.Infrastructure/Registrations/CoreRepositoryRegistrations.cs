using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Infrastructure.Repositories;
using SchoolERP.Infrastructure.Repositories.Queries;

namespace SchoolERP.Infrastructure.Registrations;

/// <summary>Settings, boards, auth, rights, students, messaging, fees, push subscriptions, audit log.</summary>
internal static class CoreRepositoryRegistrations
{
    public static IServiceCollection AddCoreRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<IRecordExtraRepository, RecordExtraRepository>();
        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IRoleRightRepository, RoleRightRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILoginEventRepository, LoginEventRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentQueryRepository, StudentQueryRepository>();

        return services;
    }
}
