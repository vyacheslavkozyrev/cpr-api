using System;
using System.Security.Cryptography;
using System.Text;

namespace CPR.Api.Auth
{
    /// <summary>
    /// Helper to create simple HMAC-signed stub tokens for local testing.
    /// </summary>
    public static class TokenGenerator
    {
        /// <summary>
        /// Create a stub token for the given <paramref name="userId"/> using <paramref name="signingKey"/>.
        /// Token format: "{userId}.{base64Signature}" where signature = HMACSHA256(signingKey, userId).
        /// </summary>
        public static string CreateToken(string userId, string signingKey)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(signingKey)) throw new ArgumentNullException(nameof(signingKey));

            var keyBytes = Encoding.UTF8.GetBytes(signingKey);
            var dataBytes = Encoding.UTF8.GetBytes(userId);
            using var h = new HMACSHA256(keyBytes);
            var sig = h.ComputeHash(dataBytes);
            return userId + "." + Convert.ToBase64String(sig);
        }
    }
}
