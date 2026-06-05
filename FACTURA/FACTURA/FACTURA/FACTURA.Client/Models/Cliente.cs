namespace FACTURA.Client.Models;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty; // NIT/CC
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
