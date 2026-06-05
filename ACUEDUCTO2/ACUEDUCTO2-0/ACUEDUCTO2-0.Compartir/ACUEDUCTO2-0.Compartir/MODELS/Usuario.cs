using System;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public class Usuario
    {
        public string Guid { get; set; }
        public string Identificacion { get; set; }
        public string ApellidosNombres { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string Observaciones { get; set; }
        public byte[] Imagen { get; set; }
    }
}
