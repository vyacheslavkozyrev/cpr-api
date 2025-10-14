using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Domain.Entities;
using CPR.Infrastructure.Data;
using CPR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CPR.UnitTests;

public class UserSyncServiceTests
{
    private CprDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CprDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CprDbContext(options);
    }

    private ClaimsPrincipal CreateClaimsPrincipal(Dictionary<string, string> claims)
    {
        var claimsList = new List<Claim>();
        foreach (var kvp in claims)
        {
            claimsList.Add(new Claim(kvp.Key, kvp.Value));
        }

        var identity = new ClaimsIdentity(claimsList, "Bearer");
        return new ClaimsPrincipal(identity);
    }

    #region ExtractEntraExternalId Tests

    [Fact]
    public void ExtractEntraExternalId_NullClaims_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = service.ExtractEntraExternalId(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractEntraExternalId_WithOidClaim_ReturnsValue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var oid = Guid.NewGuid().ToString();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid }
        });

        // Act
        var result = service.ExtractEntraExternalId(principal);

        // Assert
        Assert.Equal(oid, result);
    }

    [Fact]
    public void ExtractEntraExternalId_WithObjectIdentifierClaim_ReturnsValue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var objectId = Guid.NewGuid().ToString();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "http://schemas.microsoft.com/identity/claims/objectidentifier", objectId }
        });

        // Act
        var result = service.ExtractEntraExternalId(principal);

        // Assert
        Assert.Equal(objectId, result);
    }

    [Fact]
    public void ExtractEntraExternalId_PrefersOidOverObjectIdentifier()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var oid = Guid.NewGuid().ToString();
        var objectId = Guid.NewGuid().ToString();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid },
            { "http://schemas.microsoft.com/identity/claims/objectidentifier", objectId }
        });

        // Act
        var result = service.ExtractEntraExternalId(principal);

        // Assert
        Assert.Equal(oid, result);
    }

    [Fact]
    public void ExtractEntraExternalId_NoOidClaim_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "name", "Test User" },
            { "email", "test@example.com" }
        });

        // Act
        var result = service.ExtractEntraExternalId(principal);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ExtractDisplayName Tests

    [Fact]
    public void ExtractDisplayName_NullClaims_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = service.ExtractDisplayName(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractDisplayName_WithClaimTypesName_ReturnsValue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var displayName = "John Doe";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, displayName)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = service.ExtractDisplayName(principal);

        // Assert
        Assert.Equal(displayName, result);
    }

    [Fact]
    public void ExtractDisplayName_WithNameClaim_ReturnsValue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var displayName = "Jane Smith";
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "name", displayName }
        });

        // Act
        var result = service.ExtractDisplayName(principal);

        // Assert
        Assert.Equal(displayName, result);
    }

    [Fact]
    public void ExtractDisplayName_WithPreferredUsername_ReturnsValue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var username = "jsmith@example.com";
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "preferred_username", username }
        });

        // Act
        var result = service.ExtractDisplayName(principal);

        // Assert
        Assert.Equal(username, result);
    }

    [Fact]
    public void ExtractDisplayName_WithEmail_ReturnsValue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var email = "user@example.com";
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "email", email }
        });

        // Act
        var result = service.ExtractDisplayName(principal);

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void ExtractDisplayName_PrecedenceOrder_ClaimTypesNameFirst()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Primary Name"),
            new Claim("name", "Secondary Name"),
            new Claim("preferred_username", "username@example.com"),
            new Claim("email", "email@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = service.ExtractDisplayName(principal);

        // Assert
        Assert.Equal("Primary Name", result);
    }

    [Fact]
    public void ExtractDisplayName_NoDisplayNameClaims_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", Guid.NewGuid().ToString() }
        });

        // Act
        var result = service.ExtractDisplayName(principal);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetUserByEntraExternalIdAsync Tests

    [Fact]
    public async Task GetUserByEntraExternalIdAsync_NullEntraExternalId_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = await service.GetUserByEntraExternalIdAsync(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEntraExternalIdAsync_EmptyEntraExternalId_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = await service.GetUserByEntraExternalIdAsync(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEntraExternalIdAsync_UserExists_ReturnsUser()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var entraExternalId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraExternalId = entraExternalId,
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = await service.GetUserByEntraExternalIdAsync(entraExternalId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(entraExternalId, result.EntraExternalId);
    }

    [Fact]
    public async Task GetUserByEntraExternalIdAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = await service.GetUserByEntraExternalIdAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEntraExternalIdAsync_DeletedUser_ReturnsNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var entraExternalId = Guid.NewGuid().ToString();
        var deletedUser = new User
        {
            Id = Guid.NewGuid(),
            EntraExternalId = entraExternalId,
            UserName = "deleteduser",
            DisplayName = "Deleted User",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow,
            DeletedBy = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        db.Users.Add(deletedUser);
        await db.SaveChangesAsync();

        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act
        var result = await service.GetUserByEntraExternalIdAsync(entraExternalId);

        // Assert
        Assert.Null(result); // Should not return deleted users
    }

    #endregion

    #region SyncUserFromClaimsAsync Tests

    [Fact]
    public async Task SyncUserFromClaimsAsync_NullClaims_ThrowsArgumentNullException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.SyncUserFromClaimsAsync(null, Guid.NewGuid()));
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_MissingOidClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "name", "Test User" },
            { "email", "test@example.com" }
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncUserFromClaimsAsync(principal, Guid.NewGuid()));
        Assert.Contains("Missing required 'oid' claim", exception.Message);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_MissingDisplayNameClaims_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        // Note: ExtractDisplayName falls back to email, so we need to omit email too
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", Guid.NewGuid().ToString() }
            // No name, preferred_username, or email claims
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SyncUserFromClaimsAsync(principal, Guid.NewGuid()));
        Assert.Contains("Missing required 'name' or 'preferred_username' claim", exception.Message);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_NewUser_CreatesUser()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var oid = Guid.NewGuid().ToString();
        var syncedBy = Guid.NewGuid();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid },
            { "name", "John Doe" },
            { "email", "john.doe@example.com" },
            { "preferred_username", "johndoe@example.com" }
        });

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal, syncedBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(oid, result.EntraExternalId);
        Assert.Equal("John Doe", result.DisplayName);
        Assert.Equal("johndoe@example.com", result.UserName);
        Assert.Equal(syncedBy, result.CreatedBy);
        Assert.False(result.IsDeleted);

        // Verify user was saved to database
        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.EntraExternalId == oid);
        Assert.NotNull(userInDb);
        Assert.Equal("John Doe", userInDb.DisplayName);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_ExistingUser_UpdatesUser()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var oid = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        // Create existing user
        var existingUser = new User
        {
            Id = userId,
            EntraExternalId = oid,
            UserName = "oldusername",
            DisplayName = "Old Name",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
            CreatedBy = createdBy
        };
        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var syncedBy = Guid.NewGuid();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid },
            { "name", "New Name" },
            { "email", "new@example.com" }
        });

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal, syncedBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id); // Same user ID
        Assert.Equal(oid, result.EntraExternalId);
        Assert.Equal("New Name", result.DisplayName); // Updated display name
        Assert.Equal("oldusername", result.UserName); // Username should NOT change
        Assert.Equal(createdBy, result.CreatedBy); // Original creator unchanged
        Assert.Equal(syncedBy, result.ModifiedBy); // Modified by the sync
        Assert.NotNull(result.ModifiedAt);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_UsernameFromEmail_WhenNoPreferredUsername()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var oid = Guid.NewGuid().ToString();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid },
            { "name", "Test User" },
            { "email", "testuser@example.com" }
            // No preferred_username
        });

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.UserName); // Should extract from email
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_UsernameFromOid_WhenNoEmailOrPreferredUsername()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var oid = Guid.NewGuid().ToString();
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid },
            { "name", "Test User" }
            // No email or preferred_username
        });

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("user_", result.UserName); // Should generate from oid
        Assert.Contains(oid.Substring(0, 8), result.UserName);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_DeletedUser_NotUpdated()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var oid = Guid.NewGuid().ToString();

        // Create deleted user
        var deletedUser = new User
        {
            Id = Guid.NewGuid(),
            EntraExternalId = oid,
            UserName = "deleteduser",
            DisplayName = "Deleted User",
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow,
            DeletedBy = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-7),
            CreatedBy = Guid.NewGuid()
        };
        db.Users.Add(deletedUser);
        await db.SaveChangesAsync();

        var service = new UserSyncService(db, NullLogger<UserSyncService>.Instance);
        var principal = CreateClaimsPrincipal(new Dictionary<string, string>
        {
            { "oid", oid },
            { "name", "New Name" },
            { "email", "new@example.com" }
        });

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(deletedUser.Id, result.Id); // Should create new user, not update deleted one
        Assert.Equal(oid, result.EntraExternalId);
        Assert.Equal("New Name", result.DisplayName);
        Assert.False(result.IsDeleted);
    }

    #endregion
}
