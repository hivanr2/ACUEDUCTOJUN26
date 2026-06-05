using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Data
{
    public class ApplicationRole : IdentityRole
    {
        [Key]
        public new string Id
        {
            get => base.Id;
            set => base.Id = value;
        }
    }
}
