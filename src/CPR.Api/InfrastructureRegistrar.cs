using Microsoft.Extensions.DependencyInjection;

namespace CPR.Api
{
    /// <summary>
    /// Helper to register infrastructure services into the DI container.
    /// This is a placeholder while the real infrastructure registration is implemented.
    /// </summary>
    public static class InfrastructureRegistrar
    {
        /// <summary>
        /// Register infrastructure services (EF Core, repositories) with DI.
        /// Placeholder until real wiring is implemented in CPR.Infrastructure.
        /// </summary>
        /// <param name="services">The service collection to modify.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register infrastructure services (repositories and EF-backed services)
            services.AddScoped<CPR.Infrastructure.Repositories.GoalsRepository>();
            services.AddScoped<CPR.Application.Services.IGoalService, CPR.Infrastructure.Services.GoalService>();
            services.AddScoped<CPR.Application.Services.IClassificationService, CPR.Infrastructure.Services.ClassificationService>();
            return services;
        }
    }
}

// Note: real Infrastructure wiring will live in CPR.Infrastructure project and be registered via DI in CPR.Api
