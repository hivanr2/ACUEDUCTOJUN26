using System;

namespace ACUEDUCTO2.Compartir.MODELS
{
    public class Consumo
    {
        public string Guid { get; set; }
        public string IdPeriodo { get; set; }
        public string IdUsuario { get; set; }
        public int M3Consumidos { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
