using System.Text.Json.Serialization;

namespace ACUEDUCTO.Models;

public partial class SerieDocumentacion
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    [JsonIgnore]
    public virtual ICollection<Documentacion>? Documentacions { get; set; }
}
