using ACUEDUCTO.Models;
using System.ComponentModel.DataAnnotations.Schema;

public partial class CuentaPorCobrar
{
    public string IdcuentaxC { get; set; } = null!;

    public int? Numero { get; set; }

    public string? Identificacion { get; set; }

    [Column("Descripcion")]
    public string? Descripcion { get; set; }

    public DateTime? FechaHora { get; set; }

    public DateTime? Vencimiento { get; set; }

    public double? ValorDebe { get; set; }

    public double? ValorPagado { get; set; }

    public int? CuotasTotales { get; set; }

    public int? CuotasPagadas { get; set; }

    public bool? EnFactura { get; set; } = false;

    public string? IdFactura { get; set; }

    public DateTime? FechaCreacion { get; set; } = DateTime.Now;

    public virtual Usuario? IdentificacionNavigation { get; set; }
}