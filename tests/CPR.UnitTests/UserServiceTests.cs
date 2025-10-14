using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Services;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CPR.UnitTests;

public class UserServiceTests
{
    private CprDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CprDbContext(options);
    }

    private ClaimsPrincipal CreateClaimsPrincipal(string userId, Dictionary<string, string> additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "testuser")
        };

        if (additionalClaims != null)
        {
            foreach (var kvp in additionalClaims)
            {
                claims.Add(new Claim(kvp.Key, kvp.Value));
            }
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_UnauthenticatedUser_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var unauthenticatedPrincipal = new ClaimsPrincipal();

        // Act
        var result = await service.GetCurrentUserProfileAsync(unauthenticatedPrincipal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_MissingUserIdClaim_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_InvalidUserIdFormat_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ExtractsEmail_FromClaimTypesEmail()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var email = "user@example.com";

        var principal = CreateClaimsPrincipal(userId, new Dictionary<string, string>
        {
            { ClaimTypes.Email, email }
        });

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ExtractsEmail_FromEmailClaim()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var email = "user@example.com";

        var principal = CreateClaimsPrincipal(userId, new Dictionary<string, string>
        {
            { "email", email }
        });

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_PrefersClaimTypesEmail_OverEmailClaim()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();

        var principal = CreateClaimsPrincipal(userId, new Dictionary<string, string>
        {
            { ClaimTypes.Email, "primary@example.com" },
            { "email", "secondary@example.com" }
        });

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("primary@example.com", result.Email);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_NoEmailClaim_EmailIsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var principal = CreateClaimsPrincipal(userId);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Email);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ExtractsDisplayName_FromClaimTypesName()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var displayName = "John Doe";

        // Create principal with ClaimTypes.Name set to the display name we want to test
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, displayName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(displayName, result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ExtractsDisplayName_FromNameClaim()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var displayName = "Jane Smith";

        // Create principal without ClaimTypes.Name, only "name"
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("name", displayName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(displayName, result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_ExtractsDisplayName_FromPreferredUsername()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var preferredUsername = "jsmith";

        // Create principal without ClaimTypes.Name or "name", only "preferred_username"
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("preferred_username", preferredUsername)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(preferredUsername, result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_DisplayNameFallback_UsesUsername()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();
        var username = "testuser";

        // Create principal with only NameIdentifier (no display name claims)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_DisplayNamePrecedence_ClaimTypesNameFirst()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();

        // Create principal with multiple display name claims to test precedence
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Primary Name"),
            new Claim("name", "Secondary Name"),
            new Claim("preferred_username", "username123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Primary Name", result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_NoEmployee_ReturnsFallbackProfile()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();

        // Create principal with email and name claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("name", "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(userId, result.EmployeeId); // Falls back to userId
        Assert.Equal("testuser", result.UserName);
        // ClaimTypes.Name takes precedence, so display name should be "testuser"
        Assert.Equal("testuser", result.DisplayName);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_WithEmployee_ReturnsEmployeeProfile()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();

        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        // Create user and employee in database
        var user = new User
        {
            Id = userId,
            UserName = "jdoe",
            DisplayName = "John Doe from DB",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        var employee = new Employee
        {
            Id = employeeId,
            UserId = userId,
            User = user,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        db.Users.Add(user);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var service = new UserService(db);
        var principal = CreateClaimsPrincipal(userId.ToString(), new Dictionary<string, string>
        {
            { ClaimTypes.Email, "jdoe@example.com" },
            { "name", "John Doe from Token" }
        });

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId.ToString(), result.UserId);
        Assert.Equal(employeeId.ToString(), result.EmployeeId);
        Assert.Equal("jdoe", result.UserName);
        Assert.Equal("John Doe from DB", result.DisplayName); // Should use DB value
        Assert.Equal("jdoe@example.com", result.Email); // Should use token value
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_DeletedEmployee_ReturnsFallbackProfile()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();

        var userId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        // Create user and deleted employee
        var user = new User
        {
            Id = userId,
            UserName = "deleted_user",
            DisplayName = "Deleted User",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        var employee = new Employee
        {
            Id = employeeId,
            UserId = userId,
            User = user,
            IsDeleted = true, // Deleted employee
            DeletedAt = DateTimeOffset.UtcNow,
            DeletedBy = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId
        };

        db.Users.Add(user);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var service = new UserService(db);
        var principal = CreateClaimsPrincipal(userId.ToString());

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId.ToString(), result.UserId);
        Assert.Equal(userId.ToString(), result.EmployeeId); // Should fall back to userId since employee is deleted
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_EntraStyleClaims_WorksCorrectly()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();

        // Simulate Microsoft Entra External ID token claims
        // ClaimTypes.Name takes precedence over "name" claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "entra_user"), // This will be used for DisplayName
            new Claim("oid", Guid.NewGuid().ToString()), // Object ID from Entra
            new Claim("email", "user@entraid.com"),
            new Claim("name", "Entra External User"),
            new Claim("preferred_username", "entra_user@entraid.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user@entraid.com", result.Email);
        // ClaimTypes.Name has precedence, so DisplayName will be "entra_user"
        Assert.Equal("entra_user", result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_StubStyleClaims_WorksCorrectly()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserService(db);
        var userId = Guid.NewGuid().ToString();

        // Simulate stub token claims (minimal)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "stub_user"),
            new Claim("sub", userId) // Subject claim from stub
        };
        var identity = new ClaimsIdentity(claims, "Stub");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUserProfileAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Email); // Stub tokens don't have email
        Assert.Equal("stub_user", result.DisplayName); // Falls back to username
    }
}
