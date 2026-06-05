using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class TrabajoMaterial
    {
        [Key]
        public int Id { get; set; }
        public int IdTrabajo { get; set; }
        public string Material { get; set; }
        public double? Cantidad { get; set; }
    }
}
