using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO2.Share.Models
{
    public partial class Factura
    {
        [NotMapped]
        public string Idfactura
        {
            get => Guid;
            set => Guid = value;
        }

        [NotMapped]
        public string Idpredio
        {
            get => IdPredio;
            set => IdPredio = value;
        }

        [NotMapped]
        public Predio IdpredioNavigation
        {
            get => IdPredioNavigation;
            set => IdPredioNavigation = value;
        }

        [NotMapped]
        public string Idplanilla
        {
            get => IdPeriodo;
            set => IdPeriodo = value;
        }

        [NotMapped]
        public Periodo IdplanillaNavigation
        {
            get => IdPeriodoNavigation;
            set => IdPeriodoNavigation = value;
        }

        [NotMapped]
        public string NotasFactura
        {
            get => NotaDeFactura;
            set => NotaDeFactura = value;
        }

        [NotMapped]
        public string NotasRecaudo
        {
            get => NotaDeRecaudo;
            set => NotaDeRecaudo = value;
        }
    }
}
