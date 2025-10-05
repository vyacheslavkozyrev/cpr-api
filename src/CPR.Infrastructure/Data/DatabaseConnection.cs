using System;
using Microsoft.Extensions.Configuration;

namespace CPR.Infrastructure.Data
{
    public static class DatabaseConnection
    {
        public static string BuildConnectionString(string defaultHost = "localhost", string defaultPort = "5432", string defaultDatabase = "cpr_dev")
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? defaultHost;
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? defaultPort;
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? defaultDatabase;
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";

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