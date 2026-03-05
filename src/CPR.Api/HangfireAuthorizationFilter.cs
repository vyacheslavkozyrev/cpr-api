using Hangfire.Dashboard;

namespace CPR.Api
{
    /// <summary>
    /// Simple authorization filter for Hangfire dashboard
    /// In development, allows all access. In production, this should check user roles.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In development, allow all access
            // In production, you should check if the user has admin role:
            // var httpContext = context.GetHttpContext();
            // return httpContext.User.IsInRole("Administrator");

            return true; // Allow all in development
        }
    }
}
