namespace ACUEDUCTO.Models;

public partial class TrabajosPersonal
{
    public int IdTrabajo { get; set; }

    public int IdPersonal { get; set; }

    public decimal HorasTrabajadas { get; set; }

    //[JsonIgnore]
    //public virtual Personal IdPersonalNavigation { get; set; } = null!;

    //[JsonIgnore]
    //public virtual Trabajo IdTrabajoNavigation { get; set; } = null!;
}
