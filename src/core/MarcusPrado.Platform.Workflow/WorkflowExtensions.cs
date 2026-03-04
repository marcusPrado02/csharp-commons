using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Workflow;

/// <summary>DI registration extensions for the workflow engine.</summary>
public static class WorkflowExtensions
{
    /// <summary>Registers <see cref="DefaultWorkflowEngine"/> as the singleton <see cref="IWorkflowEngine"/>.</summary>
    public static IServiceCollection AddWorkflowEngine(
        this IServiceCollection services,
        Action<DefaultWorkflowEngine>? configure = null)
    {
        services.AddSingleton<DefaultWorkflowEngine>();
        services.AddSingleton<IWorkflowEngine>(sp =>
        {
            var engine = sp.GetRequiredService<DefaultWorkflowEngine>();
            configure?.Invoke(engine);
            return engine;
        });
        return services;
    }
}
