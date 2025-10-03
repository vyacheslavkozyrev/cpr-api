using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CPR.Api.Auth
{
    /// <summary>
    /// Authorization policy provider that handles RequireRole policies.
    /// Parses policy strings like "RequireRole:Employee,Manager" and creates appropriate requirements.
    /// </summary>
    public class RoleAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAuthorizationPolicyProvider"/> class.
        /// </summary>
        /// <param name="options">The authorization options.</param>
        public RoleAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// Gets the authorization policy for the specified policy name.
        /// </summary>
        /// <param name="policyName">The name of the policy to retrieve.</param>
        /// <returns>The authorization policy, or null if not found.</returns>
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Check if this is a RequireRole policy
            if (policyName.StartsWith("RequireRole:", StringComparison.OrdinalIgnoreCase))
            {
                var rolesPart = policyName.Substring("RequireRole:".Length);
                var roles = rolesPart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (roles.Length == 0)
                {
                    throw new InvalidOperationException($"Invalid RequireRole policy: {policyName}. No roles specified.");
                }

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new RequireRoleRequirement(roles))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            // Fall back to the default policy provider for other policies
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        /// <summary>
        /// Gets the default authorization policy.
        /// </summary>
        /// <returns>The default authorization policy.</returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        /// <summary>
        /// Gets the fallback authorization policy.
        /// </summary>
        /// <returns>The fallback authorization policy, or null if not available.</returns>
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }
    }
}