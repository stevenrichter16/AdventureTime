using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AdventureTime.Application;

/// <summary>
/// This class configures all Application layer services.
/// Notice how it knows nothing about databases, Entity Framework, or any infrastructure concerns.
/// It only registers business logic components like command handlers and validators.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all Application services to the dependency injection container.
    /// This includes MediatR for CQRS and any other business logic services.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR and tell it where to find our handlers
        // Assembly.GetExecutingAssembly() means "look in this project for handlers"
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // You can add pipeline behaviors here for cross-cutting concerns
            // For example, logging, validation, or performance monitoring
            // cfg.AddBehavior<IPipelineBehavior<,>, LoggingBehavior<,>>();
        });
        
        // If you add FluentValidation later, you would register validators here:
        // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // If you have any Application-specific services (not infrastructure), register them here
        // For example, services that contain pure business logic without external dependencies
        
        return services;
    }
}