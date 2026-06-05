using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class TipoSolicitud
    {
        [Key]
        public string Guid { get; set; }
        public string NombreTipoSolicitud { get; set; }
        public string DescripcionTipoSolicitud { get; set; }
        public string Observaciones { get; set; }
    }
}
