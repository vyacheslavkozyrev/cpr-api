using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CPR.Infrastructure.Data;
using Xunit;

namespace CPR.IntegrationTests
{
    public class SeedVerificationTests
    {
        [Fact]
        public void SeedRows_Exist_InDatabase()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                    .UseNpgsql("Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres")
                .Options;

            using var db = new CprDbContext(options);

            Assert.True(db.CareerPaths.Any(cp => cp.Title == "Technology"));
            Assert.True(db.SkillCategories.Any(sc => sc.Title == "Technical"));
            Assert.True(db.Positions.Any(p => p.Title == "Senior Software Engineer"));
            Assert.True(db.Projects.Any(p => p.Code == "PRJ-001"));
        }
    }
}
