using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CPR.Api.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CPR.UnitTests
{
    public class JwtStubAuthenticationHandlerTests
    {
        [Fact]
        public async Task ValidToken_IsAuthenticated()
        {
            var signingKey = "test-signing-key";
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", signingKey);

            var userId = Guid.NewGuid().ToString();
            var token = TokenGenerator.CreateToken(userId, signingKey);

            var services = new ServiceCollection();
            services.AddLogging();
            var provider = services.BuildServiceProvider();

            var options = new OptionsMonitorStub<AuthenticationSchemeOptions>(new AuthenticationSchemeOptions());
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer " + token;

            var handler = new JwtStubAuthenticationHandler(options, NullLoggerFactory.Instance, System.Text.Encodings.Web.UrlEncoder.Default, provider);
            await handler.InitializeAsync(new AuthenticationScheme("Stub", "Stub", typeof(JwtStubAuthenticationHandler)), context);

            var result = await handler.AuthenticateAsync();
            Assert.True(result.Succeeded);
            Assert.Equal(userId, result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        [Fact]
        public async Task InvalidToken_IsRejected()
        {
            Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", "test-signing-key");

            var badToken = "not.a.valid.token";

            var services = new ServiceCollection();
            services.AddLogging();
            var provider = services.BuildServiceProvider();

            var options = new OptionsMonitorStub<AuthenticationSchemeOptions>(new AuthenticationSchemeOptions());
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer " + badToken;

            var handler = new JwtStubAuthenticationHandler(options, NullLoggerFactory.Instance, System.Text.Encodings.Web.UrlEncoder.Default, provider);
            await handler.InitializeAsync(new AuthenticationScheme("Stub", "Stub", typeof(JwtStubAuthenticationHandler)), context);

            var result = await handler.AuthenticateAsync();
            Assert.False(result.Succeeded);
        }
    }

    // Minimal IOptionsMonitor stub for tests
    internal class OptionsMonitorStub<T> : IOptionsMonitor<T> where T : class, new()
    {
        private T _current;
        public OptionsMonitorStub(T current) => _current = current;
        public T CurrentValue => _current;
        public T Get(string name) => _current;
        public IDisposable OnChange(Action<T, string> listener) => null;
    }
}
