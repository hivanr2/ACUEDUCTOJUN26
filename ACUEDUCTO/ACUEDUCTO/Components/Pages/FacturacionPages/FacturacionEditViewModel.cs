public class FacturacionEditViewModel
{
    public string Idfactura { get; set; }
    public string Idplanilla { get; set; }
    public string Idpredio { get; set; }
    public string? CodigoFactura { get; set; }
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
}