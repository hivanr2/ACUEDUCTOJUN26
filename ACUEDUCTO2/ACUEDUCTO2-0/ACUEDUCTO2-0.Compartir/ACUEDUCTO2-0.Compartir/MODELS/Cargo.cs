using System;

namespace ACUEDUCTO2.Compartir.MODELS
{
    public class Cargo
    {
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
