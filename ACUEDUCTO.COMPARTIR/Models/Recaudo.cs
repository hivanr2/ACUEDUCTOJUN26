using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Recaudo
{
    public string Idrecaudo { get; set; } = null!;

    public int? Numero { get; set; }

    public DateTime? FechaHora { get; set; }

    public string? Idfactura { get; set; }

    public string? Idusuario { get; set; }

    public string? Concepto { get; set; }

    public string? Lugar { get; set; }

    public double? Valor { get; set; }
}
