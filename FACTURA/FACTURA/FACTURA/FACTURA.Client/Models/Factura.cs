using System.ComponentModel.DataAnnotations;

namespace FACTURA.Client.Models;

public class Factura
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string Estado { get; set; } = "Borrador"; // Borrador, Emitida, Anulada
    public List<ItemFactura> Items { get; set; } = [];

    public decimal Subtotal => Decimal.Round(Items.Sum(i => i.Subtotal), 2);
    public decimal Impuestos => Decimal.Round(Items.Sum(i => i.Impuesto), 2);
    public decimal Total => Subtotal + Impuestos;
}
