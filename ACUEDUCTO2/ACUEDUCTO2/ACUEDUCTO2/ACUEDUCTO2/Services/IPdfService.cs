using System.Text;
using ACUEDUCTO2.Share.Models;

namespace ACUEDUCTO2.Services;

public interface IPdfService
{
    byte[] FromHtml(string html);
    byte[] FromFactura(Factura factura, IEnumerable<Tarifa> tarifas, string logoDataUri, string chartDataUri);
}

public sealed class HtmlPdfService : IPdfService
{
    public byte[] FromHtml(string html)
    {
        return Encoding.UTF8.GetBytes(html);
    }

    public byte[] FromFactura(Factura factura, IEnumerable<Tarifa> tarifas, string logoDataUri, string chartDataUri)
    {
        var html = $"""
        <!doctype html>
        <html>
        <head><meta charset="utf-8"><title>{factura.CodigoFactura}</title></head>
        <body>
        <h1>Factura {factura.CodigoFactura}</h1>
        <p>{factura.IdPredioNavigation?.ApellidosNombresUsuario}</p>
        <p>Valor a pagar: {factura.ValorPeriodo:N0}</p>
        </body>
        </html>
        """;
        return FromHtml(html);
    }
}
