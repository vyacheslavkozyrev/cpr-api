using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using CPR.Application.Services;

namespace CPR.Api.Auth
{
    /// <summary>
    /// Authorization handler that processes RequireRoleAttribute policies.
    /// Checks if the authenticated user has any of the required roles.
    /// Supports both Stub authentication (using 'sub' claim) and Microsoft Entra External ID (using 'oid' claim).
    /// </summary>
    public class RoleAuthorizationHandler : AuthorizationHandler<RequireRoleRequirement>
    {
        private readonly IRoleService _roleService;
        private readonly IUserSyncService _userSyncService;
        private readonly ILogger<RoleAuthorizationHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="roleService">The role service used to check user roles.</param>
        /// <param name="userSyncService">The user sync service for syncing Entra users to local database.</param>
        /// <param name="logger">The logger for authorization events.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for storing user context.</param>
        public RoleAuthorizationHandler(
            IRoleService roleService, 
            IUserSyncService userSyncService,
            ILogger<RoleAuthorizationHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _userSyncService = userSyncService ?? throw new ArgumentNullException(nameof(userSyncService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Handles the authorization requirement by checking if the user has any of the required roles.
        /// Supports both Stub authentication (using 'sub' claim) and Microsoft Entra External ID (using 'oid' claim).
        /// For Entra users, automatically syncs the user to the local database if not already present.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="requirement">The role requirement containing the allowed roles.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RequireRoleRequirement requirement)
        {
            Guid userId;

            // Determine authentication scheme and extract user ID accordingly
            var authenticationType = context.User.Identity?.AuthenticationType;
            
            if (authenticationType == "Stub")
            {
                // Stub authentication: Use 'sub' claim (ClaimTypes.NameIdentifier)
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out userId))
                {
                    _logger.LogWarning("Authorization failed: User ID claim ('sub') not found or invalid in Stub authentication");
                    context.Fail();
                    return;
                }
                
                _logger.LogDebug("Using Stub authentication for user {UserId}", userId);
            }
            else
            {
                // Microsoft Entra External ID: Use 'oid' claim (object identifier)
                var entraExternalId = _userSyncService.ExtractEntraExternalId(context.User);
                
                if (string.IsNullOrEmpty(entraExternalId))
                {
                    _logger.LogWarning("Authorization failed: Entra External ID ('oid') claim not found");
                    context.Fail();
                    return;
                }

                _logger.LogDebug("Using Entra External ID authentication for user with oid: {EntraExternalId}", entraExternalId);

                // Sync user from Entra to local database (creates or updates user)
                try
                {
                    // Use a system user ID for the sync operation (Guid.Empty represents system operations)
                    var user = await _userSyncService.SyncUserFromClaimsAsync(context.User, Guid.Empty);
                    userId = user.Id;

                    // Store user in HttpContext for downstream use (e.g., controllers, services)
                    if (_httpContextAccessor.HttpContext != null)
                    {
                        _httpContextAccessor.HttpContext.Items["User"] = user;
                    }

                    _logger.LogInformation("Synced Entra user {EntraExternalId} to local user {UserId}", entraExternalId, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync Entra user {EntraExternalId} to local database", entraExternalId);
                    context.Fail();
                    return;
                }
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