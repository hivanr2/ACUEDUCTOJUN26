using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class TipoDocumentacion
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Documentacion> Documentacions { get; set; } = new List<Documentacion>();
}
