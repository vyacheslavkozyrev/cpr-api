using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CPR.Application.Services;
using CPR.Application.Repositories;
using CPR.Application.Contracts;
using CPR.Domain.Entities;
using CPR.Infrastructure.Services;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Repositories;

namespace CPR.UnitTests.Services
{
    public class TeamServiceTests : IDisposable
    {
        private readonly CprDbContext _dbContext;
        private readonly ITeamRepository _repo;
        private readonly Mock<IClassificationService> _mockClassificationService;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            var options = new DbContextOptionsBuilder<CprDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CprDbContext(options);
            _repo = new TeamRepository(_dbContext);
            _mockClassificationService = new Mock<IClassificationService>();
            _teamService = new TeamService(_dbContext, _repo, _mockClassificationService.Object);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetTeamMembersAsync_ReturnsTeamMembers_WhenManagerHasDirectReports()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var employee1Id = Guid.NewGuid();
            var employee2Id = Guid.NewGuid();

            // Create test data in the in-memory database
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var employee1User = new User { Id = Guid.NewGuid(), UserName = "employee1", DisplayName = "Employee 1", PasswordHash = "hashedpassword", IsDeleted = false };
            var employee2User = new User { Id = Guid.NewGuid(), UserName = "employee2", DisplayName = "Employee 2", PasswordHash = "hashedpassword", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var employee1 = new Employee { Id = employee1Id, UserId = employee1User.Id, Title = "Developer", ManagerId = managerId, IsDeleted = false };
            var employee2 = new Employee { Id = employee2Id, UserId = employee2User.Id, Title = "Designer", ManagerId = managerId, IsDeleted = false };

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(employee1User);
            _dbContext.Users.Add(employee2User);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(employee1);
            _dbContext.Employees.Add(employee2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.GetTeamMembersAsync(managerId);

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Contains(result, r => r.Id == employee1Id && r.Title == "Developer");
            Assert.Contains(result, r => r.Id == employee2Id && r.Title == "Designer");
        }

        [Fact]
        public async Task GetTeamMembersAsync_ReturnsEmptyArray_WhenManagerHasNoDirectReports()
        {
            // Arrange
            var managerId = Guid.NewGuid();

            // Create test data - manager with no direct reports
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };

            _dbContext.Users.Add(managerUser);
            _dbContext.Employees.Add(manager);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.GetTeamMembersAsync(managerId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTeamMemberProfileAsync_ReturnsProfile_WhenValidManagerAndMember()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            // Create test data in the in-memory database
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var memberUser = new User { Id = Guid.NewGuid(), UserName = "member", DisplayName = "Member User", PasswordHash = "hashedpassword", IsDeleted = false };

            var engineeringDept = new Department { Id = new Guid("fff11111-1111-1111-1111-111111111111"), Name = "Engineering", Code = "ENG", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var member = new Employee { Id = memberId, UserId = memberUser.Id, Title = "Developer", DepartmentId = engineeringDept.Id, ManagerId = managerId, IsDeleted = false };

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(memberUser);
            _dbContext.Departments.Add(engineeringDept);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(member);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.GetTeamMemberProfileAsync(managerId, memberId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(memberId, result.Id);
            Assert.Equal("Developer", result.EmployeeInfo.Title);
            Assert.Equal("Engineering", result.EmployeeInfo.Department);
        }

        [Fact]
        public async Task GetTeamMemberProfileAsync_ReturnsNull_WhenMemberIsNotDirectReport()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            // Create test data - manager and member not related
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var memberUser = new User { Id = Guid.NewGuid(), UserName = "member", DisplayName = "Member User", PasswordHash = "hashedpassword", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var member = new Employee { Id = memberId, UserId = memberUser.Id, Title = "Developer", ManagerId = Guid.NewGuid(), IsDeleted = false }; // Different manager

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(memberUser);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(member);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.GetTeamMemberProfileAsync(managerId, memberId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTeamGoalsAsync_ReturnsAggregatedGoals_WhenManagerHasTeam()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var employee1Id = Guid.NewGuid();
            var employee2Id = Guid.NewGuid();

            // Create test data in the in-memory database
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var employee1User = new User { Id = Guid.NewGuid(), UserName = "employee1", DisplayName = "Employee 1", PasswordHash = "hashedpassword", IsDeleted = false };
            var employee2User = new User { Id = Guid.NewGuid(), UserName = "employee2", DisplayName = "Employee 2", PasswordHash = "hashedpassword", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var employee1 = new Employee { Id = employee1Id, UserId = employee1User.Id, Title = "Developer", ManagerId = managerId, IsDeleted = false };
            var employee2 = new Employee { Id = employee2Id, UserId = employee2User.Id, Title = "Designer", ManagerId = managerId, IsDeleted = false };

            var goal1 = new Goal { Id = Guid.NewGuid(), EmployeeId = employee1Id, Title = "Complete project", IsCompleted = false, IsDeleted = false };
            var goal2 = new Goal { Id = Guid.NewGuid(), EmployeeId = employee2Id, Title = "Learn new technology", IsCompleted = true, IsDeleted = false };

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(employee1User);
            _dbContext.Users.Add(employee2User);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(employee1);
            _dbContext.Employees.Add(employee2);
            _dbContext.Goals.Add(goal1);
            _dbContext.Goals.Add(goal2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.GetTeamGoalsAsync(managerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalGoals);
            Assert.Equal(1, result.ActiveGoals);
            Assert.Equal(1, result.CompletedGoals);
            Assert.Equal(2, result.MemberGoals.Count);
        }

        [Fact]
        public async Task IsManagerAsync_ReturnsTrue_WhenEmployeeHasDirectReports()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();

            // Create test data in the in-memory database
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var employeeUser = new User { Id = Guid.NewGuid(), UserName = "employee", DisplayName = "Employee User", PasswordHash = "hashedpassword", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var employee = new Employee { Id = employeeId, UserId = employeeUser.Id, Title = "Developer", ManagerId = managerId, IsDeleted = false };

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(employeeUser);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.IsManagerAsync(managerId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsManagerAsync_ReturnsFalse_WhenEmployeeHasNoDirectReports()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            // Create test data in the in-memory database
            var employeeUser = new User { Id = Guid.NewGuid(), UserName = "employee", DisplayName = "Employee User", PasswordHash = "hashedpassword", IsDeleted = false };
            var employee = new Employee { Id = employeeId, UserId = employeeUser.Id, Title = "Developer", IsDeleted = false };

            _dbContext.Users.Add(employeeUser);
            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.IsManagerAsync(employeeId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateManagerRelationshipAsync_ReturnsTrue_WhenValidRelationship()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            // Create test data - manager and direct report
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var memberUser = new User { Id = Guid.NewGuid(), UserName = "member", DisplayName = "Member User", PasswordHash = "hashedpassword", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var member = new Employee { Id = memberId, UserId = memberUser.Id, Title = "Developer", ManagerId = managerId, IsDeleted = false };

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(memberUser);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(member);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.ValidateManagerRelationshipAsync(managerId, memberId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateManagerRelationshipAsync_ReturnsFalse_WhenInvalidRelationship()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            // Create test data - manager and member not related
            var managerUser = new User { Id = Guid.NewGuid(), UserName = "manager", DisplayName = "Manager User", PasswordHash = "hashedpassword", IsDeleted = false };
            var memberUser = new User { Id = Guid.NewGuid(), UserName = "member", DisplayName = "Member User", PasswordHash = "hashedpassword", IsDeleted = false };

            var manager = new Employee { Id = managerId, UserId = managerUser.Id, Title = "Manager", IsDeleted = false };
            var member = new Employee { Id = memberId, UserId = memberUser.Id, Title = "Developer", ManagerId = Guid.NewGuid(), IsDeleted = false }; // Different manager

            _dbContext.Users.Add(managerUser);
            _dbContext.Users.Add(memberUser);
            _dbContext.Employees.Add(manager);
            _dbContext.Employees.Add(member);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _teamService.ValidateManagerRelationshipAsync(managerId, memberId);

            // Assert
            Assert.False(result);
        }
    }
}