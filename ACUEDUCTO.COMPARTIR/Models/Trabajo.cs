using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO.Models;

public partial class Trabajo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTrabajo { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public string? Descripcion { get; set; }

    public int TipoTrabajo { get; set; }
    public string? Estado { get; set; }
    public string? Idplanilla { get; set; }
    public string? Materiales { get; set; }
    public string? Personal { get; set; }
    [StringLength(50)]
    public string? IdPredio { get; set; }

    [StringLength(50)]
    public string? IdInventario { get; set; }
    public string? Observaciones { get; set; }

    //public int IdEquipo { get; set; }
    ////public virtual Equipo Equipo { get; set; }
    //[JsonIgnore]
    //public virtual Planilla? IdplanillaNavigation { get; set; }
    //[JsonIgnore]
    //public virtual TiposTrabajo? tiposTrabajoNavigation { get; set; }
    //[JsonIgnore]
    //public virtual ICollection<TrabajosMateriales> TrabajosMateriales { get; set; } = new List<TrabajosMateriales>();
    //[JsonIgnore]
    //public virtual ICollection<TrabajosPersonal> TrabajosPersonals { get; set; } = new List<TrabajosPersonal>();
    //[JsonIgnore]
    //public virtual ICollection<Equipo> IdEquipos { get; set; } = new List<Equipo>();
}
