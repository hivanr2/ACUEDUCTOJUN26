using System.Security.Claims;
using ACUEDUCTO2.Share.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ACUEDUCTO2.Data;

namespace ACUEDUCTO2.Services;

public sealed class MenuPermissionService : IMenuPermissionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogger<MenuPermissionService> _logger;

    public MenuPermissionService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<MenuPermissionService> logger)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyDictionary<string, MenuPermission>> GetPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        try
        {
            await using var db = _dbFactory.CreateDbContext();
            var name = user.Identity?.Name ?? string.Empty;
            var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();

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
                    existing.CanView |= p.CanView;
                    existing.CanCreate |= p.CanCreate;
                    existing.CanEdit |= p.CanEdit;
                    existing.CanDelete |= p.CanDelete;
                }
            }
            return dict;
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            _logger.LogWarning("La tabla MenuPermission no existe en la base de datos. Se usarán permisos por defecto.");
            return new Dictionary<string, MenuPermission>();
        }
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
        try
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.MenuPermissions.AsNoTracking().OrderBy(p => p.MenuKey).ToListAsync(ct);
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            _logger.LogWarning("La tabla MenuPermission no existe en la base de datos.");
            return [];
        }
    }
}
