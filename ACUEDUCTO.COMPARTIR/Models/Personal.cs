namespace ACUEDUCTO.Models;

public partial class Personal
{
    public int IdPersonal { get; set; }

    public string Nombre { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public string Usuario { get; set; } = null!;

    //[JsonIgnore]
    //public virtual ICollection<TrabajosPersonal> TrabajosPersonals { get; set; } = new List<TrabajosPersonal>();

    //public virtual Usuario UsuarioNavigation { get; set; } = null!;
}
