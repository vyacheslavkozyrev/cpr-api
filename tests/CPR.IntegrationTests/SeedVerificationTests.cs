using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CPR.Infrastructure.Data;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace CPR.IntegrationTests
{
    [Collection("IntegrationTestCollection")]
    public class SeedVerificationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SeedVerificationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public void SeedRows_Exist_InDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CprDbContext>();

            // Debug: Check what's actually in the database
            var careerPaths = db.CareerPaths.ToList();
            var skillCategories = db.SkillCategories.ToList();
            var positions = db.Positions.ToList();
            var projects = db.Projects.ToList();

            Console.WriteLine($"Found {careerPaths.Count} career paths");
            Console.WriteLine($"Found {skillCategories.Count} skill categories");
            Console.WriteLine($"Found {positions.Count} positions");
            Console.WriteLine($"Found {projects.Count} projects");

            Assert.True(db.CareerPaths.Any(cp => cp.Title == "Technology"), $"Technology career path not found. Available: {string.Join(", ", careerPaths.Select(cp => cp.Title))}");
            Assert.True(db.SkillCategories.Any(sc => sc.Title == "Technical"), $"Technical skill category not found. Available: {string.Join(", ", skillCategories.Select(sc => sc.Title))}");
            Assert.True(db.Positions.Any(p => p.Title.Contains("DevOps")), $"DevOps positions not found. Available: {string.Join(", ", positions.Select(p => p.Title).Take(5))}");
            Assert.True(db.Projects.Any(p => p.Code == "PRJ-001"), $"PRJ-001 project not found. Available: {string.Join(", ", projects.Select(p => p.Code))}");

            // Verify that Ryan King (user ID: 5950a2be-bdfb-4dcb-9913-1e3e0e022a5c) has Employee role
            var ryanKingUserId = new Guid("5950a2be-bdfb-4dcb-9913-1e3e0e022a5c");
            var employeeRoleId = db.Roles.First(r => r.Title == "Employee").Id;
            Assert.True(db.UserRoles.Any(ur => ur.UserId == ryanKingUserId && ur.RoleId == employeeRoleId), "Ryan King should have Employee role assigned");
        }
    }
}
