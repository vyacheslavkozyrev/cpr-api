using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Services;
// ...existing usings...

namespace CPR.Api.Controllers;

/// <summary>
/// MeController to manage user profile operations.
/// </summary>
[ApiController]
[Route("[controller]")]
public class MeController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Creates a new instance of <see cref="MeController"/>.
    /// </summary>
    /// <param name="userService">Service to read the current user's profile.</param>
    public MeController(IUserService userService)
    {
        _userService = userService;
    }
    /// <summary>
    /// Get the current user's profile (sample data for now).
    /// </summary>
    /// <returns>User profile object with basic details and position.</returns>
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
        var profile = _userService.GetCurrentUserProfile(User);
        if (profile == null) return Unauthorized();

        return Ok(profile);
    }
}
