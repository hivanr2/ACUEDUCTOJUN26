using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO2.Share.Models
{
    public partial class Predio
    {
        [NotMapped]
        public string Idpredio
        {
            get => IdPredio;
            set => IdPredio = value;
        }

        [NotMapped]
        public Usuario IdentificacionFacturacionNavigation { get; set; }
    }

    public partial class Usuario
    {
        [NotMapped]
        public string Celular
        {
            get => Telefono;
            set => Telefono = value;
        }
    }

    public partial class Tarifa
    {
        [NotMapped]
        public int? ValorUnidad
        {
            get => ValorM3;
            set => ValorM3 = value ?? 0;
        }
    }

    public partial class Periodo
    {
        [NotMapped]
        public string Idplanilla
        {
            get => Guid;
            set => Guid = value;
        }

        [NotMapped]
        public Periodo IdperiodoNavigation
        {
            get => this;
            set { }
        }

        [NotMapped]
        public string DescripcionCorta
        {
            get => NombreCortoPeriodo;
            set => NombreCortoPeriodo = value;
        }

        [NotMapped]
        public string DescripcionLarga
        {
            get => NombreLargoPeriodo;
            set => NombreLargoPeriodo = value;
        }

        [NotMapped]
        public DateTime? FechaHoraRegistroConsumo
        {
            get => FechaHoraIniciaPeriodo;
            set
            {
                if (value.HasValue)
                {
                    FechaHoraIniciaPeriodo = value.Value;
                }
            }
        }

        [NotMapped]
        public string Encargado
        {
            get => EncargadoRegistroConsumos;
            set => EncargadoRegistroConsumos = value;
        }

        [NotMapped]
        public double? CantidadConsumoM3total
        {
            get => CantidadConsumoM3Total;
            set => CantidadConsumoM3Total = value;
        }
    }

    public partial class Recaudo
    {
        [NotMapped]
        public string Idfactura
        {
            get => IdFactura;
            set => IdFactura = value;
        }

        [NotMapped]
        public int? Valor
        {
            get => ValorRecaudo;
            set => ValorRecaudo = value ?? 0;
        }

        [NotMapped]
        public int Numero { get; set; }
    }
}
