using CPR.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CPR.Api.Middleware;

/// <summary>
/// Middleware to enforce rate limiting on feedback request creation.
/// Prevents requestors from creating more than 50 requests in a 24-hour period.
/// </summary>
public class FeedbackRequestRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private const int MaxRequestsPerDay = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackRequestRateLimitMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public FeedbackRequestRateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to check rate limits for feedback request creation.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="repository">The feedback request repository for querying request counts.</param>
    public async Task InvokeAsync(HttpContext context, IFeedbackRequestRepository repository)
    {
        // Only check POST requests to /api/feedback/request
        if (context.Request.Method == HttpMethods.Post &&
            context.Request.Path.StartsWithSegments("/api/feedback/request", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("DEBUG RateLimitMiddleware: Processing feedback request");
            Console.WriteLine($"DEBUG RateLimitMiddleware: User.Identity.IsAuthenticated = {context.User?.Identity?.IsAuthenticated}");
            Console.WriteLine($"DEBUG RateLimitMiddleware: Total claims = {context.User?.Claims?.Count()}");

            // Debug: print all claim types
            if (context.User?.Claims != null)
            {
                foreach (var claim in context.User.Claims.Take(5))
                {
                    Console.WriteLine($"DEBUG RateLimitMiddleware:   {claim.Type} = {claim.Value}");
                }
            }

            var db = context.RequestServices.GetRequiredService<CPR.Infrastructure.Data.CprDbContext>();
            Guid employeeId;

            // Try Entra External ID authentication first (oid claim)
            var oidClaim = context.User.FindFirst("oid")?.Value
                          ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            Console.WriteLine($"DEBUG RateLimitMiddleware: oid claim = {oidClaim}");

            if (!string.IsNullOrEmpty(oidClaim) && Guid.TryParse(oidClaim, out var oid))
            {
                Console.WriteLine($"DEBUG RateLimitMiddleware: Looking up user by oid = {oid}");
                // Look up employee by entra_external_id (stored as string in database)
                var oidString = oid.ToString();
                var user = await db.Users
                    .Where(u => u.EntraExternalId == oidString && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"DEBUG RateLimitMiddleware: User found = {user != null}, UserId = {user?.Id}");

                if (user?.Id == null)
                {
                    Console.WriteLine("DEBUG RateLimitMiddleware: Returning 401 - User not found");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "User record not found for Entra OID"
                    });
                    return;
                }

                // Now get the employee record
                var employee = await db.Employees
                    .Where(e => e.UserId == user.Id && !e.IsDeleted)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"DEBUG RateLimitMiddleware: Employee found = {employee != null}, EmployeeId = {employee?.Id}");

                if (employee == null)
                {
                    Console.WriteLine("DEBUG RateLimitMiddleware: Returning 401 - Employee not found");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Employee record not found for user"
                    });
                    return;
                }

                employeeId = employee.Id;
                Console.WriteLine($"DEBUG RateLimitMiddleware: Rate limit check for employee {employeeId}");
            }
            else
            {
                // Fallback to stub authentication - use UserId from NameIdentifier claim
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? context.User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Valid user authentication required"
                    });
                    return;
                }

                // Look up employee by UserId for stub authentication
                var employee = await db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && !e.IsDeleted);

                if (employee == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Employee record not found for stub user"
                    });
                    return;
                }

                employeeId = employee.Id;
            }

            // Check requests created in last 24 hours
            var since = DateTimeOffset.UtcNow.AddDays(-1);
            var recentRequestCount = await repository.CountRequestsByRequestorSinceAsync(employeeId, since);

            if (recentRequestCount >= MaxRequestsPerDay)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "RateLimitExceeded",
                    message = $"You have exceeded the limit of {MaxRequestsPerDay} feedback requests per 24 hours. Please try again later.",
                    retry_after_seconds = CalculateRetryAfter(since)
                });
                return;
            }
        }

        await _next(context);
    }

    private static int CalculateRetryAfter(DateTimeOffset oldestRequestTime)
    {
        var retryAfter = oldestRequestTime.AddDays(1) - DateTimeOffset.UtcNow;
        return Math.Max(0, (int)retryAfter.TotalSeconds);
    }
}
