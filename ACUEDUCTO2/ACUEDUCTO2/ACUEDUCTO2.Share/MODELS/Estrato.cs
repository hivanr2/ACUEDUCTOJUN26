using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Estrato
    {
        [Key]
        public int IdEstrato { get; set; }
        public string Descripcion { get; set; }
        public float SobreTasa { get; set; }
        public float CargoFijo { get; set; }
    }
}
