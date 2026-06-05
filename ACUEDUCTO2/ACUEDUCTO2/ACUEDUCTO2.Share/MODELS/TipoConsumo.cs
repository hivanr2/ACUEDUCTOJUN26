using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    public partial class TipoConsumo
    {
        [Key]
        public int Id { get; set; }

        public string Categoria { get; set; }

        public string Rango { get; set; }

        public string Observaciones { get; set; }

        public virtual ICollection<Predio> Predios { get; set; } = new List<Predio>();
    }
}
