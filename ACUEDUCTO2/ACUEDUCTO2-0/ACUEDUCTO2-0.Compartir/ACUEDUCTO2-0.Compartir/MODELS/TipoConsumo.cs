using System;
using System.Collections.Generic;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public partial class TipoConsumo
    {
        public int Id { get; set; }

        public string Categoria { get; set; }

        public string Rango { get; set; }

        public string Observaciones { get; set; }

        public virtual ICollection<Predio> Predios { get; set; } = new List<Predio>();
    }
}
