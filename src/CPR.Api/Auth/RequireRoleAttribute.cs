using Microsoft.AspNetCore.Authorization;

namespace CPR.Api.Auth
{
    /// <summary>
    /// Custom authorization attribute that requires the user to have one of the specified roles.
    /// This attribute extends the standard AuthorizeAttribute to work with role-based authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireRoleAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequireRoleAttribute"/> class.
        /// </summary>
        /// <param name="roles">The roles that are allowed to access the resource. Multiple roles can be specified as separate parameters.</param>
        public RequireRoleAttribute(params string[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                throw new ArgumentException("At least one role must be specified.", nameof(roles));
            }

            // Set the policy name that will be handled by our RoleAuthorizationHandler
            Policy = $"RequireRole:{string.Join(",", roles)}";

            // Store the roles for potential future use
            Roles = roles;
        }

        /// <summary>
        /// Gets the roles required for authorization.
        /// </summary>
        public new string[] Roles { get; }
    }
}