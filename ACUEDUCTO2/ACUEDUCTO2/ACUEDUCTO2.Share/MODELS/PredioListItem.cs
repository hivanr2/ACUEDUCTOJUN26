namespace ACUEDUCTO2.Share.Models
{
    public class PredioListItem
    {
        public string IdPredio { get; set; } = string.Empty;
        public string ApellidosNombresUsuario { get; set; } = string.Empty;
        public double? Ruta { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public bool? ContadorEnSeguimiento { get; set; }
        public bool? Activo { get; set; }
        public string TipoConsumoCategoria { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public int? IdTipoConsumo { get; set; }
    }
}