using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Tarifa
{
    public string Tipo { get; set; } = null!;

    public int? Desde { get; set; }

    public int? Hasta { get; set; }

    public double? ValorUnidad { get; set; }
}
