using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Trabajos
    {
        [Key]
        public int IdTrabajo { get; set; }

        public string Descripcion { get; set; }

        // FK to TipoTrabajo
        public int TipoTrabajo { get; set; }

        public string Estado { get; set; }

        // Responsable(s)
        public string Personal { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFinalizacion { get; set; }

        // IdPredio links to Predio.IdPredio
        public string IdPredio { get; set; }

        // Navigation
        public virtual TipoTrabajo TipoTrabajoNavigation { get; set; }
        public virtual ICollection<TrabajoEquipo> TrabajosEquipos { get; set; } = new List<TrabajoEquipo>();
        public virtual ICollection<TrabajoMaterial> TrabajosMateriales { get; set; } = new List<TrabajoMaterial>();
        public virtual ICollection<TrabajoPersonal> TrabajosPersonales { get; set; } = new List<TrabajoPersonal>();
    }
}
