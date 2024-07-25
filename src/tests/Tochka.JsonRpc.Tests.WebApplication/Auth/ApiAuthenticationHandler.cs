using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Tochka.JsonRpc.Tests.WebApplication.Auth;

internal class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
{
    public ApiAuthenticationHandler(IOptionsMonitor<ApiAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult((string) Request.Headers[AuthConstants.Header] == AuthConstants.Key
            ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), Scheme.Name)), Scheme.Name))
            : AuthenticateResult.Fail("auth failed"));
}
