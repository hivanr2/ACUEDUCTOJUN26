using System;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public class Recaudo
    {
        public string Guid { get; set; }
        public string IdFactura { get; set; }
        public string IdUsuario { get; set; }
        public int ValorRecaudo { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
