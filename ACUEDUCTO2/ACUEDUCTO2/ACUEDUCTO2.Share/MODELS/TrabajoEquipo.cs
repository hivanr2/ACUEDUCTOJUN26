using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class TrabajoEquipo
    {
        [Key]
        public int Id { get; set; }
        public int IdTrabajo { get; set; }
        public string Equipo { get; set; }
        public int? Cantidad { get; set; }
    }
}
