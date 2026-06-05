namespace ACUEDUCTO2-0_0.Compartir.MODELS
{
    public class Tarifa
    {
        public string Guid { get; set; }
        public string IdPredio { get; set; }
        public string Descripcion { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
        public int ValorM3 { get; set; }
    }
}
