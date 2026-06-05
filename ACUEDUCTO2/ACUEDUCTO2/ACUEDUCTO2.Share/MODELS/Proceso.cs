using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public class Proceso
    {
        [Key]
        public string Id { get; set; }
        public string Objeto { get; set; }
        public string Nombre { get; set; }
        public int IdTipoProceso { get; set; }

        // Concurrency token to support optimistic concurrency control.
        // Mapped via Fluent API in ApplicationDbContext (RowVersion / ROWVERSION)
        public byte[] RowVersion { get; set; }
    }
}
