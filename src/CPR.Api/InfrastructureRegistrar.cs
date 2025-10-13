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
            services.AddScoped<CPR.Application.Repositories.IGoalsRepository, CPR.Infrastructure.Repositories.GoalsRepository>();
            services.AddScoped<CPR.Application.Repositories.IFeedbackRepository, CPR.Infrastructure.Repositories.FeedbackRepository>();
            services.AddScoped<CPR.Application.Repositories.ITeamRepository, CPR.Infrastructure.Repositories.TeamRepository>();
            services.AddScoped<CPR.Application.Repositories.IRoleRepository, CPR.Infrastructure.Repositories.RoleRepository>();
            services.AddScoped<CPR.Application.Repositories.IUserRoleRepository, CPR.Infrastructure.Repositories.UserRoleRepository>();
            services.AddScoped<CPR.Application.Repositories.IProjectRepository, CPR.Infrastructure.Repositories.ProjectRepository>();
            services.AddScoped<CPR.Application.Services.IGoalService, CPR.Infrastructure.Services.GoalService>();
            services.AddScoped<CPR.Application.Services.IClassificationService, CPR.Infrastructure.Services.ClassificationService>();
            services.AddScoped<CPR.Application.Services.IFeedbackService, CPR.Infrastructure.Services.FeedbackService>();
            services.AddScoped<CPR.Application.Services.ITeamService, CPR.Infrastructure.Services.TeamService>();
            services.AddScoped<CPR.Application.Services.IRoleService, CPR.Infrastructure.Services.RoleService>();
            services.AddScoped<CPR.Application.Services.IProjectService, CPR.Infrastructure.Services.ProjectService>();
            services.AddScoped<CPR.Application.Services.IUserSyncService, CPR.Infrastructure.Services.UserSyncService>();

            // Register database seeder
            services.AddScoped<CPR.Infrastructure.Services.DatabaseSeeder>();

            return services;
        }
    }
}

// Note: real Infrastructure wiring will live in CPR.Infrastructure project and be registered via DI in CPR.Api
