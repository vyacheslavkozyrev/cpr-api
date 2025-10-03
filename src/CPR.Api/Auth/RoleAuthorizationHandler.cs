using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using CPR.Application.Services;

namespace CPR.Api.Auth
{
    /// <summary>
    /// Authorization handler that processes RequireRoleAttribute policies.
    /// Checks if the authenticated user has any of the required roles.
    /// </summary>
    public class RoleAuthorizationHandler : AuthorizationHandler<RequireRoleRequirement>
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleAuthorizationHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="roleService">The role service used to check user roles.</param>
        /// <param name="logger">The logger for authorization events.</param>
        public RoleAuthorizationHandler(IRoleService roleService, ILogger<RoleAuthorizationHandler> logger)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the authorization requirement by checking if the user has any of the required roles.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="requirement">The role requirement containing the allowed roles.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RequireRoleRequirement requirement)
        {
            // Get the user ID from the claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Authorization failed: User ID claim not found or invalid");
                context.Fail();
                return;
            }

            _logger.LogInformation("Checking role authorization for user {UserId} requiring roles: {Roles}",
                userId, string.Join(", ", requirement.Roles));

            // Check if the user has any of the required roles
            var hasRequiredRole = await _roleService.UserHasAnyRoleAsync(userId, requirement.Roles);

            if (hasRequiredRole)
            {
                _logger.LogInformation("Authorization succeeded for user {UserId}", userId);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Authorization failed for user {UserId}: Required roles not found. User roles: {UserRoles}",
                    userId, string.Join(", ", await _roleService.GetUserRoleTitlesAsync(userId)));
                context.Fail();
            }
        }
    }

    /// <summary>
    /// Authorization requirement for role-based access control.
    /// </summary>
    public class RequireRoleRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireRoleRequirement"/> class.
        /// </summary>
        /// <param name="roles">The roles required for authorization.</param>
        public RequireRoleRequirement(params string[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                throw new ArgumentException("At least one role must be specified.", nameof(roles));
            }

            Roles = roles;
        }

        /// <summary>
        /// Gets the roles required for authorization.
        /// </summary>
        public string[] Roles { get; }
    }
}