using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class TrabajoPersonal
    {
        [Key]
        public int Id { get; set; }
        public int IdTrabajo { get; set; }
        public string PersonalNombre { get; set; }
        public string Rol { get; set; }
    }
}
