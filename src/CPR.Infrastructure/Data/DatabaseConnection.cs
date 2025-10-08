using System;
using Microsoft.Extensions.Configuration;

namespace CPR.Infrastructure.Data
{
    public static class DatabaseConnection
    {
        public static string BuildConnectionString(string defaultHost = "localhost", string defaultPort = "5432", string defaultDatabase = "cpr_dev")
        {
            var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? defaultHost;
            var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? defaultPort;
            var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? defaultDatabase;
            var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

            return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
        }

        public static string GetConnectionString(IConfiguration configuration, string defaultHost = "localhost", string defaultPort = "5432", string defaultDatabase = "cpr_dev")
        {
            return configuration.GetConnectionString("Default") ??
                   configuration["DATABASE_URL"] ??
                   BuildConnectionString(defaultHost, defaultPort, defaultDatabase);
        }
    }
}