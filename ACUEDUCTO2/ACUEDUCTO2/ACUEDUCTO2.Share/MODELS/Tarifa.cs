using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public partial class Tarifa
    {
        [Key]
        public string Guid { get; set; }
        public string IdPredio { get; set; }
        public string Descripcion { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
        public int ValorM3 { get; set; }
    }
}
