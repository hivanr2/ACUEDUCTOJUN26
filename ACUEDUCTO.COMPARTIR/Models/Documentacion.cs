using System;
using System.Collections.Generic;

namespace ACUEDUCTO.Models;

public partial class Documentacion
{
    public string Id { get; set; } = null!;

    public int? Idserie { get; set; }

    public int? Idtipo { get; set; }

    public string? Codigo { get; set; }

    public double? TiempoArchivoGestion { get; set; }

    public double? TiempoArchivoCentral { get; set; }

    public string? DisposicionFinal { get; set; }

    public string? Observaciones { get; set; }

    public string? Usuario { get; set; }

    public virtual SerieDocumentacion? IdserieNavigation { get; set; }

    public virtual TipoDocumentacion? IdtipoNavigation { get; set; }
}
