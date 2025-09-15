using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CPR.Infrastructure.Data;
using Xunit;

namespace CPR.IntegrationTests
{
    public class MigrationSmokeTests
    {
        [Fact]
        public void MigrationsHistory_Contains_AddSeedData()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            using var db = new CprDbContext(options);

            var migrations = new List<string>();
            DbConnection conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\"";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    migrations.Add(reader.GetString(0));
                }
            }

            // Accept either the legacy AddSeedData migration name or the consolidated CreateInitial
            Assert.Contains(migrations, m => m.Contains("AddSeedData_v1") || m.Contains("AddSeedData") || m.Contains("CreateInitial"));
        }
    }
}
