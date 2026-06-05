using System;

namespace ACUEDUCTO2.Compartir.MODELS
{
    public class Autorizado
    {
        public string Guid { get; set; }
        public string QuienAutoriza { get; set; }
        public string AQueAutoriza { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public string IdUsuarioRegistra { get; set; }
        public string Observaciones { get; set; }
    }
}
