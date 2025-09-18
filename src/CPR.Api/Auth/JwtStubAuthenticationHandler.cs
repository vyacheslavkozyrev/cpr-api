using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System;

namespace CPR.Api.Auth
{
    // Simple HMAC-signed stub token scheme: token format = "{userId}.{signatureBase64}"
    // signature = HMACSHA256(key, userId)
    /// <summary>
    /// Simple stub authentication handler supporting HMAC-signed tokens for local/dev testing.
    /// Token format: {userId}.{base64Signature}
    /// </summary>
#pragma warning disable CS0618 // ISystemClock is obsolete; suppress for local stub compatibility
    public class JwtStubAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JwtStubAuthenticationHandler"/>.
        /// Uses the <see cref="AuthenticationSchemeOptions.TimeProvider"/> for clock access.
        /// </summary>
        public JwtStubAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, System.IServiceProvider serviceProvider)
            : base(options, logger, encoder, AuthenticationSchemeOptionsFallbackTimeProvider(options.CurrentValue))
        {
        }

        /// <summary>
        /// Authenticate the incoming request using the stub token from the Authorization header.
        /// </summary>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.NoResult());

            var auth = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.NoResult());

            var token = auth.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
                return Task.FromResult(AuthenticateResult.Fail("Empty token"));

            Logger.LogInformation("JWT Stub Handler: Received token: {Token}", token);

            var parts = token.Split('.', 2);
            if (parts.Length != 2)
                return Task.FromResult(AuthenticateResult.Fail("Invalid token format"));

            var userId = parts[0];
            var sig = parts[1];

            Logger.LogInformation("JWT Stub Handler: Parsed userId: {UserId}, sig: {Sig}", userId, sig);

            var key = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? "test-key";
            if (string.IsNullOrEmpty(key))
                return Task.FromResult(AuthenticateResult.Fail("Signing key not configured"));

            Logger.LogInformation("JWT Stub Handler: Using signing key: {Key}", key);

            try
            {
                var expected = ComputeSignature(key, userId);
                Logger.LogInformation("JWT Stub Handler: Expected signature: {Expected}, Received: {Received}", expected, sig);

                if (!CryptographicEquals(expected, sig))
                {
                    Logger.LogWarning("JWT Stub Handler: Signature validation failed");
                    return Task.FromResult(AuthenticateResult.Fail("Invalid token signature"));
                }

                Logger.LogInformation("JWT Stub Handler: Authentication successful for user: {UserId}", userId);

                var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, userId) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating stub token");
                return Task.FromResult(AuthenticateResult.Fail("Token validation error"));
            }
        }

        private static string ComputeSignature(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var h = new HMACSHA256(keyBytes);
            var sig = h.ComputeHash(dataBytes);
            return Convert.ToBase64String(sig);
        }

        private static bool CryptographicEquals(string a, string b)
        {
            var aBytes = Convert.FromBase64String(a);
            var bBytes = Convert.FromBase64String(b);
            if (aBytes.Length != bBytes.Length) return false;
            var diff = 0;
            for (int i = 0; i < aBytes.Length; i++) diff |= aBytes[i] ^ bBytes[i];
            return diff == 0;
        }

        private static Microsoft.AspNetCore.Authentication.ISystemClock AuthenticationSchemeOptionsFallbackTimeProvider(AuthenticationSchemeOptions options)
        {
            // older API compatibility: return a simple ISystemClock implementation that uses DateTimeOffset.UtcNow
            return new SimpleSystemClock();
        }

        private class SimpleSystemClock : Microsoft.AspNetCore.Authentication.ISystemClock
        {
            public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        }
#pragma warning restore CS0618
    }
}
