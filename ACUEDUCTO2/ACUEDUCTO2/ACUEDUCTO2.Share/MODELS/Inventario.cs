using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Inventario
    {
        [Key]
        public string Guid { get; set; }
        public string Referencia { get; set; }
        public string NombreCorto { get; set; }
        public string Descripcion { get; set; }
        public int ValorUnitario { get; set; }
        public int Cantidad { get; set; }
        public int ValorTotal { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
