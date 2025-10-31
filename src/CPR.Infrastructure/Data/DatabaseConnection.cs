using System;
using Microsoft.Extensions.Configuration;

namespace CPR.Infrastructure.Data
{
    public static class DatabaseConnection
    {
        public static string BuildConnectionString(string? defaultHost = null, string? defaultPort = null, string? defaultDatabase = null)
        {
            var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? defaultHost ?? "localhost";
            var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? defaultPort ?? "5432";
            var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? defaultDatabase ?? "cpr_dev";
            var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

            return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
        }

        public static string GetConnectionString(IConfiguration configuration, string? defaultHost = null, string? defaultPort = null, string? defaultDatabase = null)
        {
            // If no default database is specified, determine it based on environment
            if (defaultDatabase == null)
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                                 configuration["Environment"] ??
                                 "Development";

                defaultDatabase = environment switch
                {
                    "Test" => "cpr_test",
                    "Development" => "cpr_dev",
                    "Production" => "cpr_prod",
                    "QA" => "cpr_qa",
                    _ => "cpr_dev"
                };
            }

            return configuration.GetConnectionString("Default") ??
                   configuration["DATABASE_URL"] ??
                   BuildConnectionString(defaultHost, defaultPort, defaultDatabase);
        }
    }
}