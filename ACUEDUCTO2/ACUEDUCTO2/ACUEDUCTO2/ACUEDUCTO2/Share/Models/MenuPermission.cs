using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class MenuPermission
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string MenuKey { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        // Optional scoping by user or role (kept for compatibility)
        [MaxLength(100)]
        public string? UserName { get; set; }
        [MaxLength(100)]
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
