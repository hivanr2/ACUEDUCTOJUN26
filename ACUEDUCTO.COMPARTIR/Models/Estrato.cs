using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Estrato
{
    public int Idestrato { get; set; }

    public string? Descripcion { get; set; }

    public double? SobreTasa { get; set; }

    public double? CargoFijo { get; set; }

    public virtual ICollection<Predio> Predios { get; set; } = new List<Predio>();
}
