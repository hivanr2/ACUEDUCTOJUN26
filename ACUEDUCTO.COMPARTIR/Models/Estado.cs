using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO.Models;

public partial class Estado
{
    public int Id { get; set; }
    public string? Nombre { get; set; }

    [Column("Descripcion")] // map explicit to avoid any mismatch
    public string? Descripcion { get; set; }
    public virtual ICollection<Solicitud>? Solicitudes { get; set; }
}
