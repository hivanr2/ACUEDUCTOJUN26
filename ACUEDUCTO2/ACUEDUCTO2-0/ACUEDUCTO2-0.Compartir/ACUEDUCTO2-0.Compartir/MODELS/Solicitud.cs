using System;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public class Solicitud
    {
        public string Guid { get; set; }

        // Codigo sequential visible to users (se asigna al guardar: max(CodigoSolicitud)+1)
        public int CodigoSolicitud { get; set; }

        public string IdTipo { get; set; }
        public string IdEstado { get; set; }
        public string DescripcionSolicitud { get; set; }
        public string ApellidosNombresRegistraSolicitud { get; set; }
        public string Planear { get; set; }
        public string IdUsuarioPlanear { get; set; }
        public DateTime? FechaHoraPlanear { get; set; }
        public string Hacer { get; set; }
        public string IdUsuarioHacer { get; set; }
        public DateTime? FechaHoraHacer { get; set; }
        public string Verificar { get; set; }
        public string IdUsuarioVerificar { get; set; }
        public DateTime? FechaHoraVerificar { get; set; }
        public string ActuarMejora { get; set; }
        public string IdUsuarioActuarMejora { get; set; }
        public DateTime? FechaHoraActuarMejora { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
