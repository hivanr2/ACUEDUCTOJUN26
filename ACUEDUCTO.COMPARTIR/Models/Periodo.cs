using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Periodo
{
    public int Idperiodo { get; set; }

    public string? DescripcionCorta { get; set; }

    public string? DescripcionLarga { get; set; }

    public virtual ICollection<Planilla> Planillas { get; set; } = new List<Planilla>();
}
