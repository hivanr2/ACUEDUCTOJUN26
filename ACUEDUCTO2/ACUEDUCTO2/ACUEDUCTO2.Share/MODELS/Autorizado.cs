using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Autorizado
    {
        [Key]
        public string Guid { get; set; }
        public string QuienAutoriza { get; set; }
        public string AQueAutoriza { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public string IdUsuarioRegistra { get; set; }
        public string Observaciones { get; set; }
    }
}
