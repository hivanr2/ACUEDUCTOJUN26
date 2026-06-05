namespace ACUEDUCTO.COMPARTIR.DTO;

public sealed class ConsumoOcrScanResponse
{
    public string? PredioId { get; set; }
    public string? CodigoUsuario { get; set; }
    public string? NumeroMedidor { get; set; }

    public double? LecturaActual { get; set; }

    public string? OcrText { get; set; }
    public double? Confianza { get; set; }

    public DateTime FechaRecibidaUtc { get; set; }
}

public sealed class ConsumoOcrAutorizarRequest
{
    public string PredioId { get; set; } = string.Empty;
    public double LecturaActual { get; set; }

    public DateTime FechaRecibidaUtc { get; set; }

    public string? NotasFactura { get; set; }
    public string? NotasRecaudo { get; set; }
}

public sealed class ConsumoOcrAutorizarResponse
{
    public string PredioId { get; set; } = string.Empty;
    public string PlanillaIdAplicada { get; set; } = string.Empty;
    public string FacturaId { get; set; } = string.Empty;

    public double? MedicionAnterior { get; set; }
    public double MedicionActual { get; set; }
    public double? ConsumoM3 { get; set; }

    public bool Actualizo { get; set; }
}
