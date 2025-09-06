using System;
using System.Security.Cryptography;
using System.Text;
using CPR.Api.Auth;
using Xunit;

namespace CPR.UnitTests
{
    public class TokenGeneratorTests
    {
        [Fact]
        public void CreateToken_ProducesExpectedFormatAndSignature()
        {
            var signingKey = "unit-test-key-98765";
            var userId = "00000000-0000-0000-0000-000000000123";

            var token = TokenGenerator.CreateToken(userId, signingKey);
            Assert.False(string.IsNullOrEmpty(token));

            var parts = token.Split('.', 2);
            Assert.Equal(2, parts.Length);
            Assert.Equal(userId, parts[0]);

            // compute expected signature
            var keyBytes = Encoding.UTF8.GetBytes(signingKey);
            var dataBytes = Encoding.UTF8.GetBytes(userId);
            using var h = new HMACSHA256(keyBytes);
            var expectedSig = Convert.ToBase64String(h.ComputeHash(dataBytes));

            Assert.Equal(expectedSig, parts[1]);
        }
    }
}
