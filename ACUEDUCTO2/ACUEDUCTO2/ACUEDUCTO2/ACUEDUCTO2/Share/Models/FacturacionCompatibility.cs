using System;

namespace ACUEDUCTO2.Share.Models
{
    // Compatibility shims to support legacy ACUEDUCTO UI code that expects
    // lowercase property names (Idpredio, Idfactura, Idplanilla, etc.).
    public partial class Facturacion : Factura
    {
    }
}
