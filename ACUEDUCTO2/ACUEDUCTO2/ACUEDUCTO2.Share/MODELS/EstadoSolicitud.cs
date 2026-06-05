using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class EstadoSolicitud
    {
        [Key]
        public string Guid { get; set; }
        public string NombreEstadoSolicitud { get; set; }
        public string DescripcionEstadoSolicitud { get; set; }
        public string Observaciones { get; set; }
    }
}
