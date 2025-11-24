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
            // Support both employee_id claim (Entra) and sub/NameIdentifier (Stub auth)
            var employeeIdClaim = context.User.FindFirst("employee_id")?.Value;
            Guid employeeId;

            if (string.IsNullOrEmpty(employeeIdClaim))
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
                var employee = await context.RequestServices.GetRequiredService<CPR.Infrastructure.Data.CprDbContext>()
                    .Employees.FirstOrDefaultAsync(e => e.UserId == userId && !e.IsDeleted);

                if (employee == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Employee record not found"
                    });
                    return;
                }

                employeeId = employee.Id;
            }
            else if (!Guid.TryParse(employeeIdClaim, out employeeId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "Invalid employee_id claim format"
                });
                return;
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
