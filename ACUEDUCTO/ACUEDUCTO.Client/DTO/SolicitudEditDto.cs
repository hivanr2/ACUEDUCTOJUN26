using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO.Models;

public partial class SolicitudEditDto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? IdUsusario { get; set; }

    [Required]
    public string? Codigo { get; set; }

    [Required]
    public int? Tipo { get; set; }

    [Required]
    public DateTime? FechaHoraRecepcion { get; set; }

    [Required]
    public int? Estado { get; set; }

    [Required]
    public string? Descripción { get; set; }

    public string? Planear { get; set; }

    public int? UsuarioPlanear { get; set; } = null;

    public DateTime? FechaHoraPlanear { get; set; } = null;

    public string? Hacer { get; set; } = null;

    public int? UsuarioHacer { get; set; } = null;

    public DateTime? FechaHoraHacer { get; set; } = null;

    public string? Verificar { get; set; } = null;

    public int? UsuarioVerifica { get; set; } = null;

    public DateTime? FechaHoraVerifica { get; set; } = null;

    public string? ActuarMejora { get; set; } = null;

    public int? UsuarioActuaMejora { get; set; } = null;

    public DateTime? FechaHoraActuaMejora { get; set; } = null;

    public bool? Cerrada { get; set; } = false;

    public int? UsuarioCerrada { get; set; } = null;

    public DateTime? FechaHoraCerrada { get; set; } = null;

    public string? Observaciones { get; set; } = null;

}
