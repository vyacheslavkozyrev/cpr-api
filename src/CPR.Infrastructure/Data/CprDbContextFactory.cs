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

            var connectionString = DatabaseConnection.GetConnectionString(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<CprDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new CprDbContext(optionsBuilder.Options);
        }
    }
}