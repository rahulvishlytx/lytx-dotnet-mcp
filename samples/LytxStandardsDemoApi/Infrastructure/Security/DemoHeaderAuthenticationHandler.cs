using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LytxStandardsDemoApi.Infrastructure.Security;

public sealed class DemoHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DemoHeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.TryGetValue("x-demo-user-id", out var userIdHeader) && Guid.TryParse(userIdHeader, out var parsedUserId)
            ? parsedUserId
            : Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var rootGroupId = Request.Headers.TryGetValue("x-demo-root-group-id", out var rootGroupHeader) && Guid.TryParse(rootGroupHeader, out var parsedRootGroupId)
            ? parsedRootGroupId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");

        var companyId = Request.Headers.TryGetValue("x-demo-company-id", out var companyHeader) && int.TryParse(companyHeader, out var parsedCompanyId)
            ? parsedCompanyId
            : 101;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("root_group_id", rootGroupId.ToString()),
            new Claim("company_id", companyId.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
