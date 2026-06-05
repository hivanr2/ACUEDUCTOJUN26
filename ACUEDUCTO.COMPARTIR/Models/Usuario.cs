using System.Text.Json.Serialization;

namespace ACUEDUCTO.Models;

public partial class Usuario
{
    public string Identificacion { get; set; } = null!;

    public string? ApellidosNombres { get; set; }

    public string? Celular { get; set; }

    public string? CorreoElectronico { get; set; }

    public string? NivelEducativo { get; set; }

    public string? Observaciones { get; set; }

    // Autenticación: hash de contraseña (256-bit) y salt
    public byte[]? PasswordHash { get; set; }

    public byte[]? PasswordSalt { get; set; }

    [JsonIgnore]
    public virtual ICollection<CuentaPorCobrar?> CuentaPorCobrars { get; set; } = new List<CuentaPorCobrar?>();
    [JsonIgnore]
    public virtual ICollection<CuentaPorPagar?> CuentaPorPagars { get; set; } = new List<CuentaPorPagar>();
    [JsonIgnore]
    public virtual ICollection<Personal>? Personals { get; set; } = new List<Personal>();
    [JsonIgnore]
    public virtual ICollection<Predio>? Predios { get; set; } = new List<Predio>();
    [JsonIgnore]
    public virtual ICollection<Solicitud>? Solicitudes { get; set; } = new List<Solicitud>();
}
