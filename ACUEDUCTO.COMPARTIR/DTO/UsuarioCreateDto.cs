namespace ACUEDUCTO.COMPARTIR.DTO
{

    public class UsuarioCreateDto
    {
        public string Identificacion { get; set; } = null!;
        public string? ApellidosNombres { get; set; }
        public string? Celular { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? NivelEducativo { get; set; }
        public string? Observaciones { get; set; }
    }
}