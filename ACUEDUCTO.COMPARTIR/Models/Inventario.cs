using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Inventario
{
    public string Idinventario { get; set; } = null!;

    public string Codigo { get; set; } = null!;

    public string? DescripcionCorta { get; set; }

    public string? DescripcionLarga { get; set; }

    public string? Unidad { get; set; }

    public string? Medida { get; set; }

    public double? Cantidad { get; set; }

    public double? ValorUnitario { get; set; }

    public double? ValorTotal { get; set; }
    public string? Observaciones { get; set; }

    public DateTime? FechaHoraRegistro { get; set; }

    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
