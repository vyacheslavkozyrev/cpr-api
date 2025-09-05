using Microsoft.AspNetCore.Mvc;

namespace CPR.Api.Controllers;

/// <summary>
/// MeController to manage user profile operations.
/// </summary>
[ApiController]
[Route("[controller]")]
public class MeController : ControllerBase
{
    /// <summary>
    /// Get the current user's profile (sample data for now).
    /// </summary>
    /// <returns>User profile object with basic details and position.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        var sample = new
        {
            employee_id = "00000000-0000-0000-0000-000000000000",
            user_name = "jane.smith",
            display_name = "Jane Smith",
            position = new { id = "00000000-0000-0000-0000-000000000001", title = "Senior Software Engineer" }
        };
        return Ok(sample);
    }
}
