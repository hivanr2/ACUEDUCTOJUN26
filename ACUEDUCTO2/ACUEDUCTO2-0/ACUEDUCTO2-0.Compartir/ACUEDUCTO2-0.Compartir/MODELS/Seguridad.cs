using System;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public class Seguridad
    {
        public string Guid { get; set; }
        public string IdRol { get; set; }
        public string Proceso { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
