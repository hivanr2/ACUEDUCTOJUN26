using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using ACUEDUCTO.Models;

namespace ACUEDUCTO.Security;

// Optional: enrich claims with roles pulled from DB based on username
public sealed class RoleClaimsTransformer : IClaimsTransformation
{
    private readonly IDbContextFactory<AcueductoContext> _dbFactory;

    public RoleClaimsTransformer(IDbContextFactory<AcueductoContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identities.FirstOrDefault(i => i.IsAuthenticated);
        if (identity is null) return principal;

        // If roles already present, skip
        if (identity.Claims.Any(c => c.Type == ClaimTypes.Role)) return principal;

        var userName = identity.Name;
        if (string.IsNullOrWhiteSpace(userName)) return principal;

        // TODO: Map username -> roles from your DB (Usuarios/AspNetUsers or a custom table)
        // Placeholder: assign Public by default, Admin by convention
        var roles = new List<string> { "Public" };
        if (userName.Equals("admin", StringComparison.OrdinalIgnoreCase) || userName.EndsWith("@xcursor.org", StringComparison.OrdinalIgnoreCase))
        {
            roles.Add("Admin");
        }

        foreach (var r in roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, r));
        }

        return await Task.FromResult(principal);
    }
}
