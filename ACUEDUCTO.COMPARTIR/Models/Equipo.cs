namespace ACUEDUCTO.Models;

public partial class Equipo
{
    public int IdEquipo { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    //[JsonIgnore]
    //public virtual ICollection<Trabajo> IdTrabajos { get; set; } = new List<Trabajo>();
}
