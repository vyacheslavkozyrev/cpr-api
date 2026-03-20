using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Api.Auth;
using CPR.Api.Services;
using CPR.Application.Contracts;
using CPR.Application.DTOs.GapAnalysis;
using CPR.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace CPR.Api.Controllers;

/// <summary>
/// Controller for employee search and directory operations
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IGapAnalysisService _gapAnalysisService;

    /// <summary>
    /// Creates a new instance of <see cref="EmployeesController"/>.
    /// </summary>
    /// <param name="userService">Service to resolve the current user profile.</param>
    /// <param name="roleService">Service to resolve the caller's role titles.</param>
    /// <param name="gapAnalysisService">Service to compute skills gap analysis.</param>
    public EmployeesController(IUserService userService, IRoleService roleService, IGapAnalysisService gapAnalysisService)
    {
        _userService = userService;
        _roleService = roleService;
        _gapAnalysisService = gapAnalysisService;
    }

    /// <summary>
    /// Search for employees by name, email, or job title with optional filters
    /// </summary>
    /// <param name="query">Search query for name, email, or job title</param>
    /// <param name="department">Filter by department</param>
    /// <param name="location">Filter by location</param>
    /// <param name="role">Filter by job title/role</param>
    /// <returns>List of matching employees</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<EmployeeSummaryDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SearchEmployees(
        [FromQuery] string? query = null,
        [FromQuery] string? department = null,
        [FromQuery] string? location = null,
        [FromQuery] string? role = null)
    {
        // TODO: Replace with actual database query
        // For now, return sample data for development
        var sampleEmployees = GetSampleEmployees();

        var results = sampleEmployees.AsQueryable();

        // Apply search query filter
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            results = results.Where(e =>
                e.DisplayName.ToLower().Contains(lowerQuery) ||
                (e.Email != null && e.Email.ToLower().Contains(lowerQuery)) ||
                (e.JobTitle != null && e.JobTitle.ToLower().Contains(lowerQuery)));
        }

        // Apply department filter
        if (!string.IsNullOrWhiteSpace(department))
        {
            results = results.Where(e => e.Department != null && e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
        }

        // Apply location filter (NOTE: EmployeeSummaryDto doesn't have location field yet)
        // Would need to extend DTO or use a different approach

        // Apply role/job title filter
        if (!string.IsNullOrWhiteSpace(role))
        {
            results = results.Where(e => e.JobTitle != null && e.JobTitle.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        var filteredResults = results.Take(50).ToList(); // Limit to 50 results

        await Task.CompletedTask; // Simulate async operation

        return Ok(filteredResults);
    }

    /// <summary>
    /// Get direct reports for the current user (employees where manager_id = current user's employee_id)
    /// </summary>
    /// <returns>List of direct report employees</returns>
    [HttpGet("direct-reports")]
    [ProducesResponseType(typeof(List<EmployeeSummaryDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetDirectReports()
    {
        // TODO: Get current user's employee ID from JWT
        // TODO: Query database for employees where manager_id = current user's employee_id
        // For now, return sample data for development
        var sampleDirectReports = new List<EmployeeSummaryDto>
        {
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                DisplayName = "Jane Smith",
                Email = "jane.smith@example.com",
                JobTitle = "Product Manager",
                Department = "Product"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                DisplayName = "Bob Johnson",
                Email = "bob.johnson@example.com",
                JobTitle = "Senior Engineer",
                Department = "Engineering"
            }
        };

        await Task.CompletedTask; // Simulate async operation

        return Ok(sampleDirectReports);
    }

    /// <summary>
    /// Get sample employee data for development
    /// TODO: Replace with actual database query from Employees table
    /// </summary>
    private List<EmployeeSummaryDto> GetSampleEmployees()
    {
        return new List<EmployeeSummaryDto>
        {
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DisplayName = "John Doe",
                Email = "john.doe@example.com",
                JobTitle = "Software Engineer",
                Department = "Engineering"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                DisplayName = "Jane Smith",
                Email = "jane.smith@example.com",
                JobTitle = "Product Manager",
                Department = "Product"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                DisplayName = "Bob Johnson",
                Email = "bob.johnson@example.com",
                JobTitle = "Senior Engineer",
                Department = "Engineering"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                DisplayName = "Alice Williams",
                Email = "alice.williams@example.com",
                JobTitle = "UX Designer",
                Department = "Design"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                DisplayName = "Charlie Brown",
                Email = "charlie.brown@example.com",
                JobTitle = "Engineering Manager",
                Department = "Engineering"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
                DisplayName = "Diana Prince",
                Email = "diana.prince@example.com",
                JobTitle = "Senior Product Manager",
                Department = "Product"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
                DisplayName = "Ethan Hunt",
                Email = "ethan.hunt@example.com",
                JobTitle = "DevOps Engineer",
                Department = "Engineering"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
                DisplayName = "Fiona Gallagher",
                Email = "fiona.gallagher@example.com",
                JobTitle = "UX Researcher",
                Department = "Design"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
                DisplayName = "George Miller",
                Email = "george.miller@example.com",
                JobTitle = "Director of Engineering",
                Department = "Engineering"
            },
            new EmployeeSummaryDto
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
                DisplayName = "Hannah Montana",
                Email = "hannah.montana@example.com",
                JobTitle = "Product Designer",
                Department = "Design"
            }
        };
    }

    /// <summary>
    /// Returns a live skills gap analysis for the specified employee.
    /// Accessible by PeopleManagers (direct reports only), Directors (same department only),
    /// and Administrators (unrestricted).
    /// </summary>
    /// <param name="id">Target employee UUID.</param>
    /// <returns>Gap analysis including radar chart data and skill-by-skill breakdown.</returns>
    [HttpGet("{id:guid}/gap-analysis")]
    [RequireRole("People Manager", "Director", "Administrator")]
    [ProducesResponseType(typeof(GapAnalysisDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> GetEmployeeGapAnalysis(Guid id, CancellationToken ct)
    {
        var profile = await _userService.GetCurrentUserProfileAsync(User);
        if (profile == null) return Unauthorized();

        if (!Guid.TryParse(profile.EmployeeId, out var callerEmployeeId) ||
            !Guid.TryParse(profile.UserId, out var callerUserId))
            return Unauthorized();

        // Resolve the caller's highest-privilege role.
        var roles = (await _roleService.GetUserRoleTitlesAsync(callerUserId)).ToList();
        string callerRole;
        if (roles.Contains("Administrator")) callerRole = "Administrator";
        else if (roles.Contains("Director")) callerRole = "Director";
        else if (roles.Contains("People Manager")) callerRole = "People Manager";
        else return Forbid();

        try
        {
            var result = await _gapAnalysisService.GetEmployeeGapAnalysisAsync(id, callerEmployeeId, callerRole, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Unprocessable Entity",
                detail: ex.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}
