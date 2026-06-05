using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class TipoConsumo
{
    public int Id { get; set; }

    public string? Categoria { get; set; }

    public string? Rango { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<Predio> Predios { get; set; } = new List<Predio>();
}
