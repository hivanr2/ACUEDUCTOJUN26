namespace FACTURA.Client.Models;

public class EmpresaConfig
{
    public string Nombre { get; set; } = string.Empty;
    public string NIT { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public decimal ImpuestoPorDefecto { get; set; } = 19m; // %

    public string ConsecutivoFacturaPrefijo { get; set; } = "FAC-";
    public int ConsecutivoFacturaSiguiente { get; set; } = 1;

    public string ConsecutivoCotizacionPrefijo { get; set; } = "COT-";
    public int ConsecutivoCotizacionSiguiente { get; set; } = 1;
}
