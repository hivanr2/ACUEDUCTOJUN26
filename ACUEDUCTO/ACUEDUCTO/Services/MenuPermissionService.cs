using System.Security.Claims;
using ACUEDUCTO.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO.Services;

public sealed class MenuPermissionService(IDbContextFactory<AcueductoContext> dbFactory) : IMenuPermissionService
{
    private readonly IDbContextFactory<AcueductoContext> _dbFactory = dbFactory;

    public async Task<IReadOnlyDictionary<string, MenuPermission>> GetPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        var name = user.Identity?.Name ?? string.Empty;
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();

        // Prefer explicit per-user permissions; then fallback to role-based, take the most permissive
        var list = await db.MenuPermissions
            .Where(p => (p.UserName != null && p.UserName == name) || (p.Role != null && roles.Contains(p.Role)))
            .AsNoTracking()
            .ToListAsync(ct);

        var dict = new Dictionary<string, MenuPermission>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in list)
        {
            if (!dict.TryGetValue(p.MenuKey, out var existing))
            {
                dict[p.MenuKey] = p;
            }
            else
            {
                // union permissive
                existing.CanView |= p.CanView;
                existing.CanCreate |= p.CanCreate;
                existing.CanEdit |= p.CanEdit;
                existing.CanDelete |= p.CanDelete;
            }
        }
        return dict;
    }

    public async Task UpsertAsync(MenuPermission permission, CancellationToken ct = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        var existing = await db.MenuPermissions.FirstOrDefaultAsync(p =>
            p.MenuKey == permission.MenuKey &&
            ((p.UserName != null && p.UserName == permission.UserName) || (p.Role != null && p.Role == permission.Role)), ct);

        if (existing is null)
        {
            db.MenuPermissions.Add(permission);
        }
        else
        {
            existing.CanView = permission.CanView;
            existing.CanCreate = permission.CanCreate;
            existing.CanEdit = permission.CanEdit;
            existing.CanDelete = permission.CanDelete;
            db.MenuPermissions.Update(existing);
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MenuPermission>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.MenuPermissions.AsNoTracking().OrderBy(p => p.MenuKey).ToListAsync(ct);
    }
}
