namespace ACUEDUCTO.Models;

public partial class Planilla
{
    public string Idplanilla { get; set; }

    public string? Descripcion { get; set; }

    public int? Idperiodo { get; set; }

    public DateTime? FechaHoraRegistroConsumo { get; set; }

    public double? PorcentajeMora { get; set; }

    public double? PorcentajeSubsidio { get; set; }

    public DateTime? FechaHoraRegistroConsumos { get; set; }

    public string? EncargadoRegistroConsumos { get; set; }

    public DateTime? FechaHoraFacturacion { get; set; }

    public string? EncargadoFacturacion { get; set; }

    public string? UltimaFacturaRegistrada { get; set; }

    public DateOnly? FechaLimitePago { get; set; }

    public DateOnly? FechaSuspension { get; set; }

    public int? TotalPredios { get; set; }

    public double? CantidadConsumoM3total { get; set; }

    public double? ValorTotalConsumoBasico { get; set; }

    public double? ValorTotalConsumoAdicional { get; set; }

    public double? ValorTotalConsumoM3 { get; set; }

    public double? ValorTotalMora { get; set; }

    public double? ValorTotalSobreTasa { get; set; }

    public double? ValorTotalSubsidio { get; set; }

    public double? ValorTotalOtros { get; set; }

    public double? ValorTotalPlanilla { get; set; }

    public double? ValorTotalRecaudo { get; set; }

    public string? Observaciones { get; set; }
    public bool Abierta { get; set; } = false;
    public virtual ICollection<Facturacion> Facturacions { get; set; }

    public virtual Periodo? IdperiodoNavigation { get; set; }

//    public virtual ICollection<Trabajo> Trabajos { get; set; }
}
