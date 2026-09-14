using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Behaviors;

namespace NexusMail.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        });

        services.AddScoped<NexusMail.Application.Abstractions.Authorization.IWorkspaceAuthorizationService, NexusMail.Application.Authorization.WorkspaceAuthorizationService>();
        
        services.AddSingleton<NexusMail.Application.Abstractions.Context.IExecutionContextAccessor, NexusMail.Application.Abstractions.Context.ExecutionContextAccessor>();


        services.AddScoped<NexusMail.Application.Features.Search.Services.ISearchReindexerService, NexusMail.Application.Features.Search.Services.SearchReindexerService>();
        services.AddScoped<NexusMail.Application.Features.AI.Services.IAIWorkflowManager, NexusMail.Application.Features.AI.Services.AIWorkflowManager>();
        services.AddScoped<NexusMail.Application.Abstractions.Copilot.ICopilotOrchestrator, NexusMail.Application.Features.Copilot.CopilotOrchestrator>();

        return services;
    }
}
