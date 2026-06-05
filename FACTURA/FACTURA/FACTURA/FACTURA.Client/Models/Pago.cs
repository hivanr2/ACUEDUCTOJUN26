namespace FACTURA.Client.Models;

public class Pago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FacturaId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    public decimal Monto { get; set; }
    public string Metodo { get; set; } = "Efectivo"; // Efectivo, Transferencia, Tarjeta
    public Guid? CuentaBancariaId { get; set; } // para Transferencia
    public string? Banco { get; set; } // nombre/alias opcional para mostrar
    public string? Referencia { get; set; }
    public string? Observacion { get; set; }
}
