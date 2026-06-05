using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ACUEDUCTO.Security;

// Auth handler that trusts upstream (web server) headers for authenticated user and roles
public sealed class HeaderAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "HeaderAuth";
    private const string UserHeader = "X-Remote-User";
    private const string RolesHeader = "X-Remote-Roles"; // optional: comma-separated

    public HeaderAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Expect upstream (IIS/Apache/reverse proxy) to inject headers after validating the user
        if (!Request.Headers.TryGetValue(UserHeader, out var userValues) || string.IsNullOrWhiteSpace(userValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userName = userValues.ToString().Trim();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName)
        };

        if (Request.Headers.TryGetValue(RolesHeader, out var rolesValues) && !string.IsNullOrWhiteSpace(rolesValues))
        {
            var roles = rolesValues.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
