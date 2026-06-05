using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Consumo
    {
        [Key]
        public string Guid { get; set; }
        public string IdFactura { get; set; }
        public int MedicionAntereiorM3 { get; set; }
        public int MedicionActualM3 { get; set; }
        public int M3Consumidos { get; set; }
        public bool EnSeguimiento { get; set; } = false;
        public string Observaciones { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
