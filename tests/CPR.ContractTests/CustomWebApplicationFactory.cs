using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Net.Http.Json;
using CPR.Application.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CPR.Infrastructure.Services;
using Microsoft.Extensions.Hosting;

namespace CPR.ContractTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    static CustomWebApplicationFactory()
    {
        // Set environment variables at the class level to ensure they're available
        Environment.SetEnvironmentVariable("POSTGRES_HOST", "localhost");
        Environment.SetEnvironmentVariable("POSTGRES_PORT", "5433");
        Environment.SetEnvironmentVariable("POSTGRES_DB", "cpr_test");
        Environment.SetEnvironmentVariable("POSTGRES_USER", "postgres");
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "postgres");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "local-test-key");
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Environment variables are already set in the static constructor
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Ensure the environment variables are available in configuration
            context.Configuration["JWT_SIGNING_KEY"] = "local-test-key";
            context.Configuration["POSTGRES_HOST"] = "localhost";
            context.Configuration["POSTGRES_PORT"] = "5433";
            context.Configuration["POSTGRES_DB"] = "cpr_test";
            context.Configuration["POSTGRES_USER"] = "postgres";
            context.Configuration["POSTGRES_PASSWORD"] = "postgres";
        });
    }

    private static readonly object _lock = new object();
    private static bool _databaseInitialized = false;

    protected override Microsoft.Extensions.Hosting.IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Use a lock to ensure only one test initializes the database
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: INITIALIZING DATABASE (FIRST TIME) ===");

                using (var scope = host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CprDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CPR.Infrastructure.Services.DatabaseSeeder>>();
                    var seeder = new CPR.Infrastructure.Services.DatabaseSeeder(dbContext, logger);

                    // Delete and recreate database
                    Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: ENSURING DATABASE DELETED ===");
                    try
                    {
                        dbContext.Database.EnsureDeleted();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database deletion warning: {ex.Message}");
                    }

                    Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: RUNNING MIGRATIONS ===");
                    dbContext.Database.Migrate();

                    // Ensure user_to_role table exists (temporary fix)
                    try
                    {
                        Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: CREATING USER_TO_ROLE TABLE ===");
                        dbContext.Database.ExecuteSqlRaw(@"
                            CREATE TABLE IF NOT EXISTS user_to_role (
                                id uuid NOT NULL,
                                user_id uuid NOT NULL,
                                role_id uuid NOT NULL,
                                created_by uuid,
                                created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                modified_by uuid,
                                modified_at timestamp with time zone,
                                is_deleted boolean NOT NULL DEFAULT false,
                                deleted_by uuid,
                                deleted_at timestamp with time zone,
                                CONSTRAINT ""PK_user_to_role"" PRIMARY KEY (id),
                                CONSTRAINT ""FK_user_to_role_roles_role_id"" FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
                                CONSTRAINT ""FK_user_to_role_users_user_id"" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
                            );

                            CREATE INDEX IF NOT EXISTS ""IX_user_to_role_user_id"" ON user_to_role (user_id);
                            CREATE INDEX IF NOT EXISTS ""IX_user_to_role_role_id"" ON user_to_role (role_id);
                            CREATE UNIQUE INDEX IF NOT EXISTS ""UX_user_to_role_user_id_role_id"" ON user_to_role (user_id, role_id) WHERE is_deleted = false;
                        ");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Table creation warning: {ex.Message}");
                    }

                    // Seed initial data for tests
                    Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: STARTING SEEDING ===");
                    seeder.SeedAsync().GetAwaiter().GetResult();
                    Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: SEEDING COMPLETED ===");

                    _databaseInitialized = true;
                }
            }
            else
            {
                Console.WriteLine("=== CUSTOM WEB APPLICATION FACTORY: DATABASE ALREADY INITIALIZED ===");
            }
        }

        return host;
    }
}