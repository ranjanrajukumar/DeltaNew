using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Admissions;
using SchoolERP.Application.Modules.Authentication;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Modules.Fees;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;

namespace SchoolERP.Application.Registrations;

/// <summary>Shared helpers, authentication, students, messaging and generic CRUD.</summary>
internal static class CoreServiceRegistrations
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ISettingsReader, SettingsReader>();
        services.AddScoped<IRecordExtras, RecordExtras>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IFeeBilling, FeeBilling>();

        // Authentication and access
        services.AddSingleton<LoginLimiter>();
        services.AddScoped<IRightsService, RightsService>();
        services.AddScoped<IRequestAuthenticator, RequestAuthenticator>();
        services.AddScoped<IAuthService, AuthService>();

        // Generic list/create/update/delete (the Node app's crudRouter)
        services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));

        // Students
        services.AddScoped<IStudentsService, StudentsService>();

        // Messaging
        services.AddScoped<IMessagingService, MessagingService>();

        return services;
    }
}
