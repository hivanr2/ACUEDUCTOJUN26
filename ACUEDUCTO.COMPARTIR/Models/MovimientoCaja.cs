using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class MovimientoCaja
{
    public int Idmov { get; set; }

    public DateTime? FechaHora { get; set; }

    public string? Iddocumento { get; set; }

    public double? Entrada { get; set; }

    public double? Salida { get; set; }

    public double? Saldo { get; set; }
}
