using System;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public class Periodo
    {
        public string Guid { get; set; }
        public int Orden { get; set; }
        public string NombreCortoPeriodo { get; set; }
        public string NombreLargoPeriodo { get; set; }
        public DateTime FechaHoraDiaFacturacion { get; set; }
        public DateTime FechaHoraIniciaPeriodo { get; set; }
        public DateTime FechaHoraTerminaPeriodo { get; set; }
        public DateTime FechaHoraIniciaRecaudo { get; set; }
        public DateTime FechaHoraTerminaRecaudo { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
