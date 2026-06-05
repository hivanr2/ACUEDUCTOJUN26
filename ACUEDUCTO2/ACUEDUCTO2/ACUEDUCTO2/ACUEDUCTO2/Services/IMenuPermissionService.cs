using System.Security.Claims;
using ACUEDUCTO2.Share.Models;

namespace ACUEDUCTO2.Services;

public interface IMenuPermissionService
{
    Task<IReadOnlyDictionary<string, MenuPermission>> GetPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default);
    Task UpsertAsync(MenuPermission permission, CancellationToken ct = default);
    Task<IReadOnlyList<MenuPermission>> GetAllAsync(CancellationToken ct = default);
}
