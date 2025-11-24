using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CPR.Application.Contracts;
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
}
