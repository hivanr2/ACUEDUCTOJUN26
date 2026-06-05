namespace Acueducto.Share;

public sealed class FacturacionDto
{
    public string Idfactura { get; set; } = string.Empty;
    public string Idplanilla { get; set; } = string.Empty;
    public string Idpredio { get; set; } = string.Empty;
    public string? CodigoUsuario { get; set; }
    public double? MedicionAnterior { get; set; }
    public double? MedicionActual { get; set; }
    public double? ConsumoM3 { get; set; }
    public double? ValorConsumo { get; set; }
    public double? CargoFijo { get; set; }
    public double? ValorPeriodo { get; set; }
    public string? NotasFactura { get; set; }
    public string? PredioNombre { get; set; }
    public string? Direccion { get; set; }
    public double? Ruta { get; set; }
    public string? NumeroMedidor { get; set; }
    public string? MarcaMedidor { get; set; }
    public string? EstadoMedidor { get; set; }
}

public sealed class PlanillaDto
{
    public string Idplanilla { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? PeriodoCorto { get; set; }
    public string? PeriodoLargo { get; set; }
    public DateTime? FechaHoraRegistroConsumo { get; set; }
    public DateOnly? FechaLimitePago { get; set; }
    public DateOnly? FechaSuspension { get; set; }
    public bool Abierta { get; set; }
}
