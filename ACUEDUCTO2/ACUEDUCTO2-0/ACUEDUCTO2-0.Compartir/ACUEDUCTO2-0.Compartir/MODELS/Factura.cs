using System;

namespace ACUEDUCTO2.Compartir.MODELS
{
    public class Factura
    {
        public string Guid { get; set; }
        public string CodigoFactura { get; set; }
        public string IdPeriodo { get; set; }
        public string IdPredio { get; set; }
        public string IdConsumo { get; set; }
        public int ValorConsumo { get; set; }
        public float PorcentajeSobretasa { get; set; }
        public int ValorSobreTasa { get; set; }
        public float PorcentajeMora { get; set; }
        public int ValorMora { get; set; }
        public float PorcentajeSubsidio { get; set; }
        public int ValorSubsidio { get; set; }
        public int ValorCuotaExtraordinaria { get; set; }
        public int NumeroCuotaActual { get; set; }
        public int NumeroCuotasTotal { get; set; }
        public int OtrosValores { get; set; }
        public int ValorFacturaBimestre { get; set; }
        public int ValorDeudaAnterior { get; set; }
        public string IdRecaudo { get; set; }
        public int ValorRecaudo { get; set; }
        public int ValorDeudaPasa { get; set; }
        public string NotaDeFactura { get; set; }
        public string NotaDeRecaudo { get; set; }
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; }
    }
}
