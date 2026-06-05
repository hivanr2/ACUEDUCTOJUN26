using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class MovimientoInventario
{
    public string Idmov { get; set; } = null!;

    public string? Idegreso { get; set; }

    public string? Idingreso { get; set; }

    public DateTime? FechaHora { get; set; }

    public int? Numero { get; set; }

    public string? Idinventario { get; set; }

    public string? Idfactura { get; set; }

    public double? Cantidad { get; set; }

    public double? ValorUnitario { get; set; }

    public double? ValorTotal { get; set; }

    public virtual Egreso? IdegresoNavigation { get; set; }

    public virtual Ingreso? IdingresoNavigation { get; set; }

    public virtual Inventario? IdinventarioNavigation { get; set; }
}
