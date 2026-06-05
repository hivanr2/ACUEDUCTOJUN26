namespace ACUEDUCTO.Services;

public interface IPdfService
{
 /// <summary>
 /// Generates a PDF document from the provided HTML content and returns the PDF bytes.
 /// Note: This implementation renders the HTML as plain text using QuestPDF. If you need
 /// pixel-perfect HTML rendering, integrate a dedicated HTML-to-PDF engine.
 /// </summary>
 byte[] FromHtml(string html);

 /// <summary>
 /// Generates a structured invoice PDF (58mm receipt) using the provided billing information and rates.
 /// Note: chartBase64 should only contain the Base64 part without the data: prefix.
 /// </summary>
 byte[] FromFactura(ACUEDUCTO.Models.Facturacion factura, System.Collections.Generic.IList<ACUEDUCTO.Models.Tarifa> tarifas, System.Collections.Generic.IEnumerable<(string Label, string Total)>? desglose, string? logoBase64, string? chartBase64, string? consumoAlertNote = null);
}
