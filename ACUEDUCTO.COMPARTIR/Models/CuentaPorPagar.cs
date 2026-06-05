using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO.Models;

public partial class CuentaPorPagar
{
    public string IdcuentaxP { get; set; } = null!;

    public int? Numero { get; set; }

    public string? Identificacion { get; set; }

    [Column("Descripcion")]
    public string? Descripcion { get; set; }

    public DateTime? FechaHora { get; set; }

    public DateTime? Vencimiento { get; set; }

    public double? ValorDebe { get; set; }

    public double? ValorPagado { get; set; }

    public int? CuotasTotales { get; set; }

    public int? CuotasPagadas { get; set; }

    public virtual Usuario? IdentificacionNavigation { get; set; }
}
