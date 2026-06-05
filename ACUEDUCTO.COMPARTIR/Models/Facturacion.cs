using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO.Models;

public partial class Facturacion
{
    public string Idfactura { get; set; } = null!;

    public string CodigoFactura { get; set; } = null!;

    public string Idplanilla { get; set; } = null!;

    public string Idpredio { get; set; } = null!;

    public string? CodigoUsuario { get; set; }

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

    public string? NotasFactura { get; set; }

    public string? NotasRecaudo { get; set; }

    // Removido [NotMapped] para permitir carga via Include y evitar null en navegación al imprimir PDF
    public virtual Planilla? IdplanillaNavigation { get; set; }
    public virtual Predio? IdpredioNavigation { get; set; }
}
