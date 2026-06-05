using System;

namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public partial class Predio
    {
        public string IdPredio { get; set; } = null;

        public string  ApellidosNombresUsuario { get; set; }

        public string  CodigoUsuario { get; set; }

        public string  IdentificacionFacturacion { get; set; }

        public int? Estrato { get; set; }

        public double? Ruta { get; set; }

        public string Direccion { get; set; }

        public string NumeroCatastral { get; set; }

        public string InformacionCatastral { get; set; }

        public int? ExtensionM2 { get; set; }

        public int? M2construidos { get; set; }

        public int? PuntosFuturos { get; set; }

        public string NumeroMedidor { get; set; }

        public string MarcaMedidor { get; set; }

        public string EstadoMedidor { get; set; }

        public double? UltimaMedicion { get; set; }

        public double? ConsumoM3promedio { get; set; }

        public double? UltimoConsumoM3 { get; set; }

        public double? ValorTotalCobros { get; set; }

        public int? CantidadCoutasCobros { get; set; }

        public int? CantidadCuotasPagadas { get; set; }

        public double? ValorTotalPagos { get; set; }

        public double? ValorTotalDeuda { get; set; }

        public bool? TieneSubsidio { get; set; } = false;

        public bool? ImprimeFactura { get; set; } = true;

        public bool? EnviaWhatsapp { get; set; } = false;

        public bool? EnviaCorreo { get; set; } = false;

        public bool? ContadorEnSeguimiento { get; set; }

        public double? Latitud { get; set; }

        public double? Longitud { get; set; }

        public bool? Activo { get; set; } = false;

        public DateTime? FechaHoraRegistro { get; set; }

        public int? TipoConsumo { get; set; }


        public bool? PUEAA_TanqueReservaAcueducto { get; set; } = false;
        public string PUEAA_TanqueReservaAcueductoObservaciones { get; set; }
        public bool? PUEAA_TanqueReservaLluvia { get; set; }
        public string PUEAA_TanqueReservaLluviaObservaciones { get; set; }
        public bool? PUEAA_ReciclajeAguas { get; set; }
        public string PUEAA_ReciclajeAguasObservaciones { get; set; }
        public bool? PUEAA_RecirculacionAguas { get; set; }
        public string PUEAA_RecirculacionAguasObservaciones { get; set; }
        public bool? PUEAA_ControlPerdidas { get; set; }
        public string PUEAA_ControlPerdidasObservaciones { get; set; }
        public bool? PUEAA_MantenimientoPiscina { get; set; }
        public string PUEAA_MantenimientoPiscinaObservaciones { get; set; }
        public bool? PUEAA_OtrosProcesos { get; set; }
        public string PUEAA_OtrosProcesosObservaciones { get; set; }

        public string Observaciones { get; set; }


        public virtual Estrato EstratoNavigation { get; set; }

        //public virtual ICollection<Facturacion> Facturacions { get; set; } = new List<Facturacion>();

        public virtual Usuario IdentificacionFacturacionNavigation { get; set; }

        public virtual TipoConsumo TipoConsumoNavigation { get; set; }
    }
}