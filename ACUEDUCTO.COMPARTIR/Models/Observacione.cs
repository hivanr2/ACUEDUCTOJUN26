using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Observacione
{
    public int IdObservacion { get; set; }

    public int IdTrabajo { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Trabajo IdTrabajoNavigation { get; set; } = null!;
}
