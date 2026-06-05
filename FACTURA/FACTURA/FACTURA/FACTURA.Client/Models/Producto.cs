namespace FACTURA.Client.Models;

public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = "UND";
    public decimal Precio { get; set; }
    public decimal Impuesto { get; set; } // porcentaje 0..100
    public bool Activo { get; set; } = true;
}
