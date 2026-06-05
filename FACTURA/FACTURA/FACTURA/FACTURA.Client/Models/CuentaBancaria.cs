namespace FACTURA.Client.Models;

public class CuentaBancaria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Banco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty; // nombre corto para mostrar
    public bool Activa { get; set; } = true;
}
