using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO.Models;

public partial class Solicitud
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? IdUsuario { get; set; }

    [Required]
    public string? Codigo { get; set; }

    [Required]
    public int? Tipo { get; set; }

    [Required]
    public DateTime? FechaHoraRecepcion { get; set; }

    [Required]
    public int? EstadoID { get; set; }

    [Required]
    public string? Descripción { get; set; }

    public string? Planear { get; set; }
    public int? UsuarioPlanear { get; set; }
    public DateTime? FechaHoraPlanear { get; set; }
    public string? Hacer { get; set; }
    public int? UsuarioHacer { get; set; }
    public DateTime? FechaHoraHacer { get; set; }
    public string? Verificar { get; set; }
    public int? UsuarioVerifica { get; set; }
    public DateTime? FechaHoraVerifica { get; set; }
    public string? ActuarMejora { get; set; }
    public int? UsuarioActuaMejora { get; set; }
    public DateTime? FechaHoraActuaMejora { get; set; }
    public bool? Cerrada { get; set; }
    public int? UsuarioCerrada { get; set; }
    public DateTime? FechaHoraCerrada { get; set; }
    public string? Observaciones { get; set; }

    public virtual Estado? EstadoNavigation { get; set; }
    public virtual Usuario? IdUsuarioNavigation { get; set; }
    public virtual Tipo? TipoNavigation { get; set; }
}
