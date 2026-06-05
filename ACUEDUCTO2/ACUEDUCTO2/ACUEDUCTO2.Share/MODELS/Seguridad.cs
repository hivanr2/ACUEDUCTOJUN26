using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Seguridad
    {
        [Key]
        public string Guid { get; set; }
        public string IdRol { get; set; }
        public string Proceso { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
