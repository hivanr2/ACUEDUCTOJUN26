using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Indicador
{
    public int Idindicador { get; set; }

    public string? Categoría { get; set; }

    public string? Responsable { get; set; }

    public string? Descripcion { get; set; }

    public string? FórmulaUnidad { get; set; }

    public string? Frecuencia { get; set; }

    public string? MetaUmbral { get; set; }

    public string? SemáforoGestión { get; set; }
}
