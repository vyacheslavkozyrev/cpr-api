using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CPR.Application.Repositories;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;

namespace CPR.UnitTests.Repositories
{
    public class TeamRepositoryTests : IDisposable
    {
        private readonly CprDbContext _context;
        private readonly TeamRepository _repository;

        public TeamRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CprDbContext(options);
            _repository = new TeamRepository(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Create users
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword" };
            var employee1User = new User { Id = Guid.NewGuid(), UserName = "employee1", DisplayName = "Employee 1", PasswordHash = "hashedpassword" };
            var employee2User = new User { Id = Guid.NewGuid(), UserName = "employee2", DisplayName = "Employee 2", PasswordHash = "hashedpassword" };

            // Create positions
            var managerPosition = new Position { Id = Guid.NewGuid(), Title = "Manager", CareerTrackId = Guid.NewGuid() };
            var developerPosition = new Position { Id = Guid.NewGuid(), Title = "Developer", CareerTrackId = Guid.NewGuid() };
            var designerPosition = new Position { Id = Guid.NewGuid(), Title = "Designer", CareerTrackId = Guid.NewGuid() };

            // Create employees
            var manager = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = managerUser.Id,
                PositionId = managerPosition.Id,
                DepartmentId = new Guid("fff11111-1111-1111-1111-111111111111") // Engineering
            };

            var employee1 = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = employee1User.Id,
                ManagerId = manager.Id,
                PositionId = developerPosition.Id,
                DepartmentId = new Guid("fff11111-1111-1111-1111-111111111111") // Engineering
            };

            var employee2 = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = employee2User.Id,
                ManagerId = manager.Id,
                PositionId = designerPosition.Id,
                DepartmentId = new Guid("fff77777-7777-7777-7777-777777777777") // Product
            };

            _context.Positions.AddRange(managerPosition, developerPosition, designerPosition);
            _context.Users.AddRange(managerUser, employee1User, employee2User);
            _context.Employees.AddRange(manager, employee1, employee2);
            _context.SaveChanges();
        }

        [Fact]
        public void GetDirectReports_ReturnsCorrectEmployees()
        {
            // Arrange
            var manager = _context.Employees.Include(e => e.Position).First(e => e.Position.Title == "Manager");

            // Act
            var directReports = _repository.GetDirectReports(manager.Id).ToList();

            // Assert
            Assert.Equal(2, directReports.Count);
            Assert.All(directReports, dr => Assert.Equal(manager.Id, dr.ManagerId));
        }

        [Fact]
        public async Task GetEmployeeWithDetailsAsync_ReturnsEmployee_WhenExists()
        {
            // Arrange
            var employee = _context.Employees.Include(e => e.Position).First(e => e.Position.Title == "Developer");

            // Act
            var result = await _repository.GetEmployeeWithDetailsAsync(employee.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.Id, result.Id);
            Assert.Equal(employee.Position.Title, result.Position.Title);
            Assert.NotNull(result.User);
            Assert.Equal(employee.UserId, result.User.Id);
        }

        [Fact]
        public async Task GetEmployeeWithDetailsAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetEmployeeWithDetailsAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEmployeeByUserIdAsync_ReturnsEmployee_WhenExists()
        {
            // Arrange
            var user = _context.Users.First(u => u.UserName == "manager");

            // Act
            var result = await _repository.GetEmployeeByUserIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal("Manager", result.Position.Title);
        }

        [Fact]
        public async Task GetEmployeeByUserIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetEmployeeByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IsDirectReportAsync_ReturnsTrue_WhenValidRelationship()
        {
            // Arrange
            var manager = _context.Employees.Include(e => e.Position).First(e => e.Position.Title == "Manager");
            var employee = _context.Employees.Include(e => e.Position).First(e => e.Position.Title == "Developer");

            // Act
            var result = await _repository.IsDirectReportAsync(manager.Id, employee.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDirectReportAsync_ReturnsFalse_WhenInvalidRelationship()
        {
            // Arrange
            var manager = _context.Employees.Include(e => e.Position).First(e => e.Position.Title == "Manager");
            var otherManagerId = Guid.NewGuid();

            // Act
            var result = await _repository.IsDirectReportAsync(manager.Id, otherManagerId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsDirectReportAsync_ReturnsFalse_WhenEmployeeNotDirectReport()
        {
            // Arrange
            var manager1 = _context.Employees.Include(e => e.Position).First(e => e.Position.Title == "Manager");
            var manager2Id = Guid.NewGuid();

            // Act
            var result = await _repository.IsDirectReportAsync(manager1.Id, manager2Id);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}