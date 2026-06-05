using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACUEDUCTO2.Share.Models
{
    /// <summary>
    /// Representa los periodos operativos utilizando los datos existentes en la tabla legado Planilla.
    /// </summary>
    public partial class Periodo
    {
        [Key]
        public string Guid { get; set; } = string.Empty;

        // Columnas propias de v2
        public int Orden { get; set; }
        public string NombreCortoPeriodo { get; set; } = string.Empty;
        public string NombreLargoPeriodo { get; set; } = string.Empty;
        public DateTime FechaHoraIniciaPeriodo { get; set; }
        public DateTime FechaHoraTerminaPeriodo { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public DateTime FechaHoraIniciaRecaudo { get; set; }
        public DateTime FechaHoraTerminaRecaudo { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; } = string.Empty;

        // Columnas heredadas de Planilla v1
        public string Descripcion { get; set; } = string.Empty;
        [NotMapped]
        public int? PeriodoReferenciaId { get; set; }
        // Estas columnas provienen de la tabla Planilla v1 y se usan para
        // representar el intervalo de registro de consumos.
        // La columna física antigua 'FWECHAHORAREGISTRO' fue un error y se
        // elimina del esquema. Mantener la propiedad en memoria (no mapeada)
        // para compatibilidad con la lógica del negocio.
        [NotMapped]
        public DateTime? FechaHoraIniciaRegistroConsumo { get; set; }
        [NotMapped]
        public DateTime? FechaHoraTerminaRegistroConsumo { get; set; }
        public double? PorcentajeMora { get; set; }
        public double? PorcentajeSubsidio { get; set; }
        public string EncargadoRegistroConsumos { get; set; } = string.Empty;
        public DateTime? FechaHoraFacturacion { get; set; }
        public string EncargadoFacturacion { get; set; } = string.Empty;
        public string UltimaFacturaRegistrada { get; set; } = string.Empty;
        public DateTime? FechaLimitePago { get; set; }
        public DateTime? FechaSuspension { get; set; }
        public int? TotalPredios { get; set; }
        public double? CantidadConsumoM3Total { get; set; }
        public double? ValorTotalConsumoBasico { get; set; }
        public double? ValorTotalConsumoAdicional { get; set; }
        public double? ValorTotalConsumoM3 { get; set; }
        public double? ValorTotalMora { get; set; }
        public double? ValorTotalSobreTasa { get; set; }
        public double? ValorTotalSubsidio { get; set; }
        public double? ValorTotalOtros { get; set; }
        public double? ValorTotalPlanilla { get; set; }
        public double? ValorTotalRecaudo { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public bool? Abierta { get; set; }
    }
}
