using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Auth;
using CPR.Application.Services;
using CPR.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CPR.UnitTests;

public class RoleAuthorizationHandlerTests
{
    private ClaimsPrincipal CreateStubPrincipal(string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "Stub");
        return new ClaimsPrincipal(identity);
    }

    private ClaimsPrincipal CreateEntraPrincipal(string oid, string displayName = "Test User", string email = "test@example.com")
    {
        var claims = new List<Claim>
        {
            new Claim("oid", oid),
            new Claim("name", displayName),
            new Claim("email", email)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    }

    #region RequireRoleRequirement Tests

    [Fact]
    public void RequireRoleRequirement_WithValidRoles_CreatesRequirement()
    {
        // Arrange & Act
        var requirement = new RequireRoleRequirement("Admin", "Manager");

        // Assert
        Assert.NotNull(requirement);
        Assert.Equal(2, requirement.Roles.Length);
        Assert.Contains("Admin", requirement.Roles);
        Assert.Contains("Manager", requirement.Roles);
    }

    [Fact]
    public void RequireRoleRequirement_WithNullRoles_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RequireRoleRequirement(null));
        Assert.Contains("At least one role must be specified", exception.Message);
    }

    [Fact]
    public void RequireRoleRequirement_WithEmptyRoles_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new RequireRoleRequirement());
        Assert.Contains("At least one role must be specified", exception.Message);
    }

    #endregion

    #region Stub Authentication Tests

    [Fact]
    public async Task HandleRequirementAsync_StubAuth_ValidUserWithRole_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var principal = CreateStubPrincipal(userId.ToString());
        var requirement = new RequireRoleRequirement("Employee");

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(true);
        roleServiceMock.Setup(x => x.GetUserRoleTitlesAsync(userId))
            .ReturnsAsync(new[] { "Employee" });

        var userSyncServiceMock = new Mock<IUserSyncService>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.True(authContext.HasSucceeded);
        Assert.False(authContext.HasFailed);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(userId, It.Is<string[]>(r => r.Contains("Employee"))), Times.Once);
        userSyncServiceMock.Verify(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_StubAuth_ValidUserWithoutRole_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var principal = CreateStubPrincipal(userId.ToString());
        var requirement = new RequireRoleRequirement("Administrator");

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(false);
        roleServiceMock.Setup(x => x.GetUserRoleTitlesAsync(userId))
            .ReturnsAsync(new[] { "Employee" });

        var userSyncServiceMock = new Mock<IUserSyncService>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.False(authContext.HasSucceeded);
        Assert.True(authContext.HasFailed);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(userId, It.Is<string[]>(r => r.Contains("Administrator"))), Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_StubAuth_MissingUserIdClaim_Fails()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[] { }, "Stub");
        var principal = new ClaimsPrincipal(identity);
        var requirement = new RequireRoleRequirement("Employee");

        var roleServiceMock = new Mock<IRoleService>();
        var userSyncServiceMock = new Mock<IUserSyncService>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.False(authContext.HasSucceeded);
        Assert.True(authContext.HasFailed);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(It.IsAny<Guid>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_StubAuth_InvalidUserIdFormat_Fails()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims, "Stub");
        var principal = new ClaimsPrincipal(identity);
        var requirement = new RequireRoleRequirement("Employee");

        var roleServiceMock = new Mock<IRoleService>();
        var userSyncServiceMock = new Mock<IUserSyncService>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.False(authContext.HasSucceeded);
        Assert.True(authContext.HasFailed);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(It.IsAny<Guid>(), It.IsAny<string[]>()), Times.Never);
    }

    #endregion

    #region Entra Authentication Tests

    [Fact]
    public async Task HandleRequirementAsync_EntraAuth_ValidUserWithRole_Succeeds()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var principal = CreateEntraPrincipal(oid);
        var requirement = new RequireRoleRequirement("Employee");

        var user = new User
        {
            Id = userId,
            EntraExternalId = oid,
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.Empty
        };

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(true);

        var userSyncServiceMock = new Mock<IUserSyncService>();
        userSyncServiceMock.Setup(x => x.ExtractEntraExternalId(It.IsAny<ClaimsPrincipal>()))
            .Returns(oid);
        userSyncServiceMock.Setup(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), Guid.Empty))
            .ReturnsAsync(user);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Items).Returns(new Dictionary<object, object>());

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.True(authContext.HasSucceeded);
        Assert.False(authContext.HasFailed);
        userSyncServiceMock.Verify(x => x.ExtractEntraExternalId(principal), Times.Once);
        userSyncServiceMock.Verify(x => x.SyncUserFromClaimsAsync(principal, Guid.Empty), Times.Once);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(userId, It.Is<string[]>(r => r.Contains("Employee"))), Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_EntraAuth_StoresUserInHttpContext()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var principal = CreateEntraPrincipal(oid);
        var requirement = new RequireRoleRequirement("Employee");

        var user = new User
        {
            Id = userId,
            EntraExternalId = oid,
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.Empty
        };

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(true);

        var userSyncServiceMock = new Mock<IUserSyncService>();
        userSyncServiceMock.Setup(x => x.ExtractEntraExternalId(It.IsAny<ClaimsPrincipal>()))
            .Returns(oid);
        userSyncServiceMock.Setup(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), Guid.Empty))
            .ReturnsAsync(user);

        var httpContextItems = new Dictionary<object, object>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Items).Returns(httpContextItems);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.True(httpContextItems.ContainsKey("User"));
        Assert.Equal(user, httpContextItems["User"]);
    }

    [Fact]
    public async Task HandleRequirementAsync_EntraAuth_MissingOidClaim_Fails()
    {
        // Arrange
        var identity = new ClaimsIdentity(new Claim[]
        {
            new Claim("name", "Test User"),
            new Claim("email", "test@example.com")
        }, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        var requirement = new RequireRoleRequirement("Employee");

        var roleServiceMock = new Mock<IRoleService>();

        var userSyncServiceMock = new Mock<IUserSyncService>();
        userSyncServiceMock.Setup(x => x.ExtractEntraExternalId(It.IsAny<ClaimsPrincipal>()))
            .Returns((string)null);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.False(authContext.HasSucceeded);
        Assert.True(authContext.HasFailed);
        userSyncServiceMock.Verify(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()), Times.Never);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(It.IsAny<Guid>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_EntraAuth_SyncFails_Fails()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var principal = CreateEntraPrincipal(oid);
        var requirement = new RequireRoleRequirement("Employee");

        var roleServiceMock = new Mock<IRoleService>();

        var userSyncServiceMock = new Mock<IUserSyncService>();
        userSyncServiceMock.Setup(x => x.ExtractEntraExternalId(It.IsAny<ClaimsPrincipal>()))
            .Returns(oid);
        userSyncServiceMock.Setup(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), Guid.Empty))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.False(authContext.HasSucceeded);
        Assert.True(authContext.HasFailed);
        userSyncServiceMock.Verify(x => x.SyncUserFromClaimsAsync(principal, Guid.Empty), Times.Once);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(It.IsAny<Guid>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequirementAsync_EntraAuth_ValidUserWithoutRole_Fails()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var principal = CreateEntraPrincipal(oid);
        var requirement = new RequireRoleRequirement("Administrator");

        var user = new User
        {
            Id = userId,
            EntraExternalId = oid,
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.Empty
        };

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(false);
        roleServiceMock.Setup(x => x.GetUserRoleTitlesAsync(userId))
            .ReturnsAsync(new[] { "Employee" });

        var userSyncServiceMock = new Mock<IUserSyncService>();
        userSyncServiceMock.Setup(x => x.ExtractEntraExternalId(It.IsAny<ClaimsPrincipal>()))
            .Returns(oid);
        userSyncServiceMock.Setup(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), Guid.Empty))
            .ReturnsAsync(user);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Items).Returns(new Dictionary<object, object>());

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.False(authContext.HasSucceeded);
        Assert.True(authContext.HasFailed);
        userSyncServiceMock.Verify(x => x.SyncUserFromClaimsAsync(principal, Guid.Empty), Times.Once);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(userId, It.Is<string[]>(r => r.Contains("Administrator"))), Times.Once);
    }

    #endregion

    #region Multiple Roles Tests

    [Fact]
    public async Task HandleRequirementAsync_StubAuth_UserHasOneOfMultipleRoles_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var principal = CreateStubPrincipal(userId.ToString());
        var requirement = new RequireRoleRequirement("Administrator", "Manager", "Employee");

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(true); // User has at least one of the required roles
        roleServiceMock.Setup(x => x.GetUserRoleTitlesAsync(userId))
            .ReturnsAsync(new[] { "Employee" });

        var userSyncServiceMock = new Mock<IUserSyncService>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.True(authContext.HasSucceeded);
        Assert.False(authContext.HasFailed);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(userId,
            It.Is<string[]>(r => r.Contains("Administrator") && r.Contains("Manager") && r.Contains("Employee"))),
            Times.Once);
    }

    [Fact]
    public async Task HandleRequirementAsync_EntraAuth_UserHasOneOfMultipleRoles_Succeeds()
    {
        // Arrange
        var oid = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();
        var principal = CreateEntraPrincipal(oid);
        var requirement = new RequireRoleRequirement("Administrator", "Manager", "Employee");

        var user = new User
        {
            Id = userId,
            EntraExternalId = oid,
            UserName = "testuser",
            DisplayName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = Guid.Empty
        };

        var roleServiceMock = new Mock<IRoleService>();
        roleServiceMock.Setup(x => x.UserHasAnyRoleAsync(userId, It.IsAny<string[]>()))
            .ReturnsAsync(true);

        var userSyncServiceMock = new Mock<IUserSyncService>();
        userSyncServiceMock.Setup(x => x.ExtractEntraExternalId(It.IsAny<ClaimsPrincipal>()))
            .Returns(oid);
        userSyncServiceMock.Setup(x => x.SyncUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>(), Guid.Empty))
            .ReturnsAsync(user);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.Items).Returns(new Dictionary<object, object>());

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        var handler = new RoleAuthorizationHandler(
            roleServiceMock.Object,
            userSyncServiceMock.Object,
            NullLogger<RoleAuthorizationHandler>.Instance,
            httpContextAccessorMock.Object);

        var authContext = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await handler.HandleAsync(authContext);

        // Assert
        Assert.True(authContext.HasSucceeded);
        Assert.False(authContext.HasFailed);
        roleServiceMock.Verify(x => x.UserHasAnyRoleAsync(userId,
            It.Is<string[]>(r => r.Contains("Administrator") && r.Contains("Manager") && r.Contains("Employee"))),
            Times.Once);
    }

    #endregion
}
