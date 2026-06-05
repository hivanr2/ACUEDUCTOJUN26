namespace ACUEDUCTO.Models;

public class MenuPermission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Target subject. Use one of them: either per-user or per-role
    public string? UserName { get; set; } // nombre de usuario autenticado (REMOTE_USER o ClaimTypes.Name)
    public string? Role { get; set; }     // Admin, Operator, Public (opcional)

    // Menu key to apply permissions to (e.g., Usuarios, Predios, Planillas, Caja, Inventario, Indicadores, Documentos, Trabajos, SGS, Footer-Consumos, Footer-Recaudo, Footer-Facturacion, Footer-Mantenimiento)
    public string MenuKey { get; set; } = string.Empty;

    // Permissions
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;

    // Timestamps (optional)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
