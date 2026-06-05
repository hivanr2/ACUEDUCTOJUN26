namespace ACUEDUCTO.Models;

public partial class TrabajosMateriales
{
    public int IdTrabajo { get; set; }

    public int IdMaterial { get; set; }

    public int Cantidad { get; set; }
    //[JsonIgnore]
    //public virtual Material IdMaterialNavigation { get; set; } = null!;
    //[JsonIgnore]
    //public virtual Trabajo IdTrabajoNavigation { get; set; } = null!;
}
