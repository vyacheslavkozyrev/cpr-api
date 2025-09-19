using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CPR.Infrastructure.Data
{
    public class CprDbContextFactory : IDesignTimeDbContextFactory<CprDbContext>
    {
        public CprDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("Default") ??
                configuration["DATABASE_URL"] ??
                "Host=localhost;Port=5432;Database=cpr_dev;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<CprDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new CprDbContext(optionsBuilder.Options);
        }
    }
}