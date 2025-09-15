using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CPR.IntegrationTests
{
    [CollectionDefinition("IntegrationTestCollection")]
    public class IntegrationTestCollection : ICollectionFixture<DatabaseCleanupFixture>
    {
        // Collection fixture glue - no code here
    }

    public class DatabaseCleanupFixture : IAsyncLifetime
    {
        private readonly string _connString = "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres";

        public async Task InitializeAsync()
        {
            // best-effort cleanup before tests run
            try
            {
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(_connString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                // Remove rows commonly inserted by tests (identified by title/description markers)
                db.Goals.RemoveRange(db.Goals.Where(g => g.Title.StartsWith("Integration test") || g.Description == "verify persistence"));
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // don't fail initialization solely because cleanup couldn't run; tests should still execute
            }
        }

        public async Task DisposeAsync()
        {
            // best-effort cleanup after tests run
            try
            {
                var options = new DbContextOptionsBuilder<CPR.Infrastructure.Data.CprDbContext>()
                    .UseNpgsql(_connString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .Options;

                using var db = new CPR.Infrastructure.Data.CprDbContext(options);
                db.Goals.RemoveRange(db.Goals.Where(g => g.Title.StartsWith("Integration test") || g.Description == "verify persistence"));
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // swallow - cleanup is best-effort
            }
        }
    }
}
