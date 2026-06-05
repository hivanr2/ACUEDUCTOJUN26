using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2-0_0.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Identificacion { get; set; } = string.Empty;
        // Nombre del cliente: Apellidos y nombres
        public string ApellidosNombres { get; set; } = string.Empty;
    }

}
