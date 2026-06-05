namespace ACUEDUCTO.Models;

public partial class Tipo
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Solicitud?> Solicitudes { get; set; } = new List<Solicitud>();
}
