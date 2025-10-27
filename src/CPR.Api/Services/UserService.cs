using System.Security.Claims;
using CPR.Api.Models;
using CPR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPR.Api.Services;

/// <summary>
/// Default <see cref="IUserService"/> implementation that extracts a minimal profile from the current ClaimsPrincipal.
/// </summary>
public class UserService(CprDbContext db) : IUserService
{
    private readonly CprDbContext _db = db;

    /// <summary>
    /// Returns a <see cref="Models.UserProfile"/> built from claims and database lookup. Returns null when the principal is unauthenticated.
    /// </summary>
    public async Task<UserProfile?> GetCurrentUserProfileAsync(ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Extract Entra External ID from token claims (oid claim)
        var entraExternalId = user.FindFirst("oid")?.Value
                             ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (string.IsNullOrWhiteSpace(entraExternalId))
        {
            // Fallback to other claim types for stub authentication compatibility
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdString, out var stubUserId))
            {
                // Handle stub authentication - lookup by UserId directly
                var stubEmployee = await _db.Employees
                    .Include(e => e.User)
                    .Include(e => e.Position)
                    .FirstOrDefaultAsync(e => e.UserId == stubUserId && !e.IsDeleted);

                if (stubEmployee != null)
                {
                    return new UserProfile
                    {
                        UserId = stubEmployee.UserId.ToString(),
                        EmployeeId = stubEmployee.Id.ToString(),
                        UserName = stubEmployee.User?.UserName ?? "unknown",
                        DisplayName = stubEmployee.User?.DisplayName ?? stubEmployee.User?.UserName ?? "unknown",
                        Email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value,
                        Position = stubEmployee.Position != null
                            ? new Position { Id = stubEmployee.Position.Id.ToString(), Title = stubEmployee.Position.Title }
                            : new Position { Id = "", Title = "" }
                    };
                }

                // No database record found - create fallback profile from claims
                var displayName = user.FindFirst(ClaimTypes.Name)?.Value
                                ?? user.FindFirst("name")?.Value
                                ?? user.FindFirst("preferred_username")?.Value
                                ?? userIdString;

                return new UserProfile
                {
                    UserId = userIdString,
                    EmployeeId = userIdString, // Fallback to userId
                    UserName = displayName,
                    DisplayName = displayName,
                    Email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value,
                    Position = new Position { Id = "", Title = "" }
                };
            }
            return null;
        }

        // Look up user by Entra External ID
        var dbUser = await _db.Users
            .FirstOrDefaultAsync(u => u.EntraExternalId == entraExternalId && !u.IsDeleted);

        if (dbUser == null)
        {
            // User not found in database - create fallback profile from claims
            var fallbackUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? entraExternalId;
            var displayName = user.FindFirst(ClaimTypes.Name)?.Value
                            ?? user.FindFirst("name")?.Value
                            ?? user.FindFirst("preferred_username")?.Value
                            ?? fallbackUserId;

            return new UserProfile
            {
                UserId = fallbackUserId,
                EmployeeId = fallbackUserId, // Fallback to userId
                UserName = displayName,
                DisplayName = displayName,
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value,
                Position = new Position { Id = "", Title = "" }
            };
        }

        // Look up the employee record for this user
        var employee = await _db.Employees
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.UserId == dbUser.Id && !e.IsDeleted);

        if (employee == null)
        {
            // User exists but is not an employee
            return new UserProfile
            {
                UserId = dbUser.Id.ToString(),
                EmployeeId = dbUser.Id.ToString(), // Fallback for non-employees
                UserName = dbUser.UserName,
                DisplayName = dbUser.DisplayName ?? dbUser.UserName,
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value,
                Position = new Position { Id = "", Title = "" }
            };
        }

        // Return employee profile with database values
        return new UserProfile
        {
            UserId = dbUser.Id.ToString(),
            EmployeeId = employee.Id.ToString(),
            UserName = dbUser.UserName,
            DisplayName = dbUser.DisplayName ?? dbUser.UserName,
            Email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value,
            Position = employee.Position != null
                ? new Position { Id = employee.Position.Id.ToString(), Title = employee.Position.Title }
                : new Position { Id = "", Title = "" }
        };
    }
}
