using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ACUEDUCTO2.Share.Models
{
    public class TipoTrabajo
    {
        [Key]
        public int IdTipoTrabajo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public virtual ICollection<Trabajos> Trabajo { get; set; }
    }
}
