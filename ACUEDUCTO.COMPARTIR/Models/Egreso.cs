using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Egreso
{
    public string Idegreso { get; set; } = null!;

    public int Numero { get; set; }

    public DateTime? FechaHora { get; set; }

    public string? Idproveedor { get; set; }

    public string? Idfactura { get; set; }

    public string? Idcxp { get; set; }

    public string? Concepto { get; set; }

    public double? Valor { get; set; }

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
