using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Ingreso
{
    public string Idingreso { get; set; } = null!;

    public int Numero { get; set; }

    public DateTime? FechaHora { get; set; }

    public string? Idcliente { get; set; }

    public string? Idfactura { get; set; }

    public string? Idcxc { get; set; }

    public string? Concepto { get; set; }

    public double? Valor { get; set; }

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
