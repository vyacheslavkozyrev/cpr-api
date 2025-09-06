using System.Security.Claims;
using CPR.Api.Models;

namespace CPR.Api.Services;

/// <summary>
/// Service responsible for providing information about the current user/principal.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Returns a minimal user profile for the current principal
    /// </summary>
    UserProfile? GetCurrentUserProfile(ClaimsPrincipal user);
}
