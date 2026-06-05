using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Cargo
    {
        [Key]
        public string IdCargo { get; set; }
        public string Descripcion { get; set; }
        public string Responsabilidades { get; set; }
        public DateTime FechaIniciaCargo { get; set; }
        public DateTime FechaTerminaCargo { get; set; }
        public int Cumplimiento { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public string IdUsuarioRegistra { get; set; }
        public string Observaciones { get; set; }
    }
}
