using AdventureTime.Application.Config;
using AdventureTime.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using AdventureTime.Infrastructure.Data;
using AdventureTime.Infrastructure.Repositories;
using AdventureTime.Infrastructure.Services;

namespace AdventureTime.Infrastructure;

/// <summary>
/// This class contains extension methods for configuring Infrastructure services.
/// It's like a master electrician who knows how to wire up all the infrastructure components.
/// By putting this configuration in the Infrastructure layer, we keep all the technical
/// details about databases and external services in one place.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all Infrastructure services to the dependency injection container.
    /// This method is called from Program.cs during application startup.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Entity Framework with PostgreSQL
        // This is where we tell EF Core exactly how to connect to our database
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    // This tells EF Core where to find migrations
                    // Since we're in a separate assembly, we need to specify it explicitly
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            
            // In development, you might want to see the SQL being generated
            // if (configuration.GetValue<bool>("EnableSensitiveDataLogging"))
            // {
            //     options.EnableSensitiveDataLogging();
            //     options.EnableDetailedErrors();
            // }
        });
        
        // Register our services
        if (configuration.GetValue<bool>("Claude"))
        {
            services.AddHttpClient<IDeepAnalysisService, ClaudeDeepAnalysisService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5); // Claude can take time for deep analysis
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            });
        }
        else
        {
            services.AddHttpClient<IDeepAnalysisService, Gpt5DeepAnalysisService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
            });
        }
        
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IEpisodeAnalysisRepository, EpisodeAnalysisRepository>();
        services.Configure<AnthropicConfig>(configuration.GetSection("Anthropic"));

        // Add configuration for OpenAI
        services.Configure<OpenAiConfig>(
            configuration.GetSection("OpenAi"));


        
        // If we had other infrastructure services, we'd register them here too
        // For example:
        // services.AddScoped<IEmailService, SendGridEmailService>();
        // services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        // services.AddScoped<ICacheService, RedisCacheService>();
        
        return services;
    }
}