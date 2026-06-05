using System;
using System.ComponentModel.DataAnnotations;

namespace ACUEDUCTO2.Share.Models
{
    /// <summary>
    /// Representa las facturas generadas a partir de los registros legados de la tabla Facturacion.
    /// </summary>
    public partial class Factura
    {
        [Key]
        public string Guid { get; set; } = string.Empty;
        public string CodigoFactura { get; set; } = string.Empty;
        public string IdPeriodo { get; set; } = string.Empty;
        public string IdPredio { get; set; } = string.Empty;
        public string CodigoUsuario { get; set; } = string.Empty;
        public DateTime? FechaHoraRegistro { get; set; }
        public double? MedicionAnterior { get; set; }
        public double? MedicionActual { get; set; }
        public double? ConsumoM3 { get; set; }
        public double? CargoFijo { get; set; }
        public double? ValorConsumo { get; set; }
        public double? PorcentajeSobreTasa { get; set; }
        public double? ValorSobreTasa { get; set; }
        public double? PorcentajeMora { get; set; }
        public double? ValorMora { get; set; }
        public double? PorcentajeSubsidio { get; set; }
        public double? ValorSubsidio { get; set; }
        public double? OtrosValores { get; set; }
        public double? ValorPeriodo { get; set; }
        public double? DeudaAnterior { get; set; }
        public double? RecaudoPeriodoTotal { get; set; }
        public double? DeudaPasa { get; set; }
        public string NotaDeFactura { get; set; } = string.Empty;
        public string NotaDeRecaudo { get; set; } = string.Empty;

        // Navigation properties
        public virtual Predio IdPredioNavigation { get; set; }
        public virtual Periodo IdPeriodoNavigation { get; set; }
    }
}
