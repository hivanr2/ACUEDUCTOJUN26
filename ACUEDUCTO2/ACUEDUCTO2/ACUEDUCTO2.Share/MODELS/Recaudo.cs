using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public partial class Recaudo
    {
        [Key]
        public string Guid { get; set; }
        public string IdFactura { get; set; }
        public string IdUsuario { get; set; }
        public int ValorRecaudo { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
