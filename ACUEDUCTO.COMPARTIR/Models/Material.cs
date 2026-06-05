namespace ACUEDUCTO.Models;

public partial class Material
{
    public int IdMaterial { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    //[JsonIgnore]
   // public virtual ICollection<TrabajosMateriales> TrabajosMateriales { get; set; } = new List<TrabajosMateriales>();
}
