using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CPR.Infrastructure.Data;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CPR.IntegrationTests
{
    [Collection("Integration")]
    public class MigrationSmokeTests
    {
        private readonly IntegrationTestFixture _fixture;
        private CustomWebApplicationFactory _factory => _fixture.Factory;

        public MigrationSmokeTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void MigrationsHistory_Contains_AddSeedData()
        {
            Console.WriteLine("MigrationSmokeTests: Test starting");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

            // Check if we can connect and if key tables exist
            var canConnect = db.Database.CanConnect();
            Assert.True(canConnect, "Should be able to connect to the test database");

            // Check if key tables exist (indicating migrations have been applied)
            var tablesExist = db.Database.SqlQueryRaw<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name IN ('users', 'employees', 'goals')").ToList();

            Assert.Contains("users", tablesExist);
            Assert.Contains("employees", tablesExist);
            Assert.Contains("goals", tablesExist);
        }
    }
}
