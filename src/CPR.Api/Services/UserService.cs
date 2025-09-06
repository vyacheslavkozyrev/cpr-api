using System.Security.Claims;
using CPR.Api.Models;

namespace CPR.Api.Services;

/// <summary>
/// Default <see cref="IUserService"/> implementation that extracts a minimal profile from the current ClaimsPrincipal.
/// </summary>
public class UserService : IUserService
{
    /// <summary>
    /// Returns a simple <see cref="Models.UserProfile"/> built from claims. Returns null when the principal is unauthenticated.
    /// </summary>
    public UserProfile? GetCurrentUserProfile(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "00000000-0000-0000-0000-000000000000";
        var username = user.Identity?.Name ?? "unknown";
        return new UserProfile
        {
            EmployeeId = userId,
            UserName = username,
            DisplayName = username,
            Position = new Position { Id = "00000000-0000-0000-0000-000000000001", Title = "Developer" }
        };
    }
}
