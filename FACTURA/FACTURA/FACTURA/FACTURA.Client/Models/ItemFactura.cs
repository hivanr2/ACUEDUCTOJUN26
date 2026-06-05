namespace FACTURA.Client.Models;

public class ItemFactura
{
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal ImpuestoPorcentaje { get; set; }

    public decimal Subtotal => Decimal.Round(Cantidad * PrecioUnitario, 2);
    public decimal Impuesto => Decimal.Round(Subtotal * (ImpuestoPorcentaje / 100m), 2);
    public decimal Total => Subtotal + Impuesto;
}
