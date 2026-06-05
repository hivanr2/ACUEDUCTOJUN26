using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO2.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Key]
        public new string Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string Identificacion { get; set; } = string.Empty;
        // Nombre del cliente: Apellidos y nombres
        // This property is stored in the separate `Usuario` table in the legacy schema.
        // Prevent EF Core from expecting a column on AspNetUsers that may not exist
        // by marking this property as not mapped. Code should populate it from the
        // `Usuario` table when required.
        public string ApellidosNombres { get; set; } = string.Empty;

        /// <summary>
        /// Alias para PhoneNumber de IdentityUser. Usa la columna PhoneNumber de AspNetUsers.
        /// </summary>
        [NotMapped]
        public string Telefono
        {
            get => PhoneNumber ?? string.Empty;
            set => PhoneNumber = value;
        }
    }
}
