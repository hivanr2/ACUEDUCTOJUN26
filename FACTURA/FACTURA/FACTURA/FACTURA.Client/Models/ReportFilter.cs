namespace FACTURA.Client.Models;

public class ReportFilter
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public Guid? ClienteId { get; set; }
    public string? Estado { get; set; } // null o "Todos", "Borrador", "Emitida", "Anulada"
}
