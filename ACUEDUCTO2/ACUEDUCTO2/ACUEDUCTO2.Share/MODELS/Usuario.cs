using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public partial class Usuario
    {
        [Key]
        public string Guid { get; set; }
        public string Identificacion { get; set; }
        // Some legacy schemas do not include a column for the user's full name
        // in the `Usuario` table. Prevent EF Core from expecting this column by
        // marking it as not mapped. The UI should prefer the Identity user
        // profile (`ApplicationUser`) or other joined sources when available.
        public string ApellidosNombres { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string Observaciones { get; set; }
        public byte[] Imagen { get; set; }
    }
}
