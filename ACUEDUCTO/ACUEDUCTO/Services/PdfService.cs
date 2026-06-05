using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using ACUEDUCTO.Models;
using Microsoft.AspNetCore.Hosting;

namespace ACUEDUCTO.Services;

public sealed class PdfService : IPdfService
{
    private readonly IWebHostEnvironment _env;
    public PdfService(IWebHostEnvironment env) => _env = env;

    public byte[] FromHtml(string html)
    {
        html ??= string.Empty;
        var plain = StripHtml(html);
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content().Text(plain).FontSize(11);
            });
        }).GeneratePdf();
    }

    // Ticket térmico ~58mm ancho => 165pt aprox. Alto extendido (se recorta por contenido)
    public byte[] FromFactura(Facturacion f, IList<Tarifa> tarifas, IEnumerable<(string Label, string Total)>? desglose, string? logoBase64, string? chartBase64, string? consumoAlertNote = null)
    {
        var culture = CultureInfo.GetCultureInfo("es-CO");
        string Cur(double? v) => v.HasValue ? v.Value.ToString("C0", culture) : "$0";
        string Num(double? v) => v?.ToString("0.##", CultureInfo.InvariantCulture) ?? "0";
        string Txt(string? v) => string.IsNullOrWhiteSpace(v) ? "-" : v.Trim();
        string Fdt(DateTime? v, string fmt) => v.HasValue ? v.Value.ToString(fmt, culture) : "-";
        string Fdd(DateOnly? v, string fmt) => v.HasValue ? v.Value.ToString(fmt, culture) : "-";

        var periodo = f.IdplanillaNavigation?.IdperiodoNavigation;
        var bimestreLargo = periodo?.DescripcionLarga ?? periodo?.DescripcionCorta ?? "BIMESTRE";
        var codigoFactura = !string.IsNullOrWhiteSpace(f.CodigoFactura) ? f.CodigoFactura : f.Idfactura;
        var p = f.IdpredioNavigation;

        // Pre-decodificar imágenes (si vienen en base64). No usar data: URI como path.
        byte[]? logoBytes = null;
        try { if (!string.IsNullOrWhiteSpace(logoBase64)) logoBytes = Convert.FromBase64String(logoBase64); } catch { logoBytes = null; }
        byte[]? chartBytes = null;
        try { if (!string.IsNullOrWhiteSpace(chartBase64)) chartBytes = Convert.FromBase64String(chartBase64); } catch { chartBytes = null; }

        // Valores (porcentaje integrado al valor)
        var rowsValores = new (string,string)[] {
            ("Valor Consumo", Cur(f.ValorConsumo)),
            ("Cargo Fijo", Cur(f.CargoFijo)),
            ("Val. SobreTasa", $"{Cur(f.ValorSobreTasa)} - ({Num(f.PorcentajeSobreTasa)}%)"),
            ("Val. Mora", $"{Cur(f.ValorMora)} - ({Num(f.PorcentajeMora)}%)"),
            ("Val. Subsidio", $"{Cur(f.ValorSubsidio)} - ({Num(f.PorcentajeSubsidio)}%)"),
            ("Otros Valores", Cur(f.OtrosValores)),
            ("VALOR BIMESTRE", Cur(f.ValorPeriodo))
        };

        // Reporte cuenta
        double deudaTotal = f.DeudaPasa ?? Math.Max(0, (f.DeudaAnterior ?? 0) + (f.ValorPeriodo ?? 0) - (f.RecaudoPeriodoTotal ?? 0));
        var rowsCuenta = new (string,string)[] {
            ("Deuda Bimestre Anterior", Cur(f.DeudaAnterior)),
            ("Pagos Bimestre", Cur(f.RecaudoPeriodoTotal)),
            ("Deuda Total", Cur(deudaTotal))
        };

        // Predio y mediciones (sin label Predio, nombre en cabecera aparte)
        var rowsPredio = new (string,string)[] {
            ("Dirección", Txt(p?.Direccion)),
            ("Ruta", Txt(p?.Ruta?.ToString())),
            ("Código Usuario", Txt(f.CodigoUsuario ?? p?.CodigoUsuario)),
            ("Medidor #", Txt(p?.NumeroMedidor)),
            ("Marca Medidor", Txt(p?.MarcaMedidor)),
            ("Estado Medidor", Txt(p?.EstadoMedidor)),
            ("M3 Anterior", Num(f.MedicionAnterior)),
            ("M3 Actual", Num(f.MedicionActual)),
            ("Consumo M3", Num(f.ConsumoM3))
        };

        // Tarifa por rangos: usar desglose si viene, si no, calcular como antes
        List<(string,string)> rowsTarifa = new();
        if (desglose != null)
        {
            rowsTarifa.AddRange(desglose);
        }
        else if (tarifas != null && tarifas.Count > 0)
        {
            double consumo = f.ConsumoM3 ?? 0;
            var t1 = tarifas.FirstOrDefault(t => t.Desde <= 1 && t.Hasta >= 40);
            var t2 = tarifas.FirstOrDefault(t => t.Desde <= 41 && t.Hasta >= 80);
            var t3 = tarifas.FirstOrDefault(t => t.Desde >= 81);
            double uso1 = Math.Min(consumo, 40);
            double uso2 = consumo > 40 ? Math.Min(consumo - 40, 40) : 0;
            double uso3 = consumo > 80 ? consumo - 80 : 0;
            rowsTarifa.Add(($"0-40: {uso1}M3 x {Cur(t1?.ValorUnidad)}", Cur((t1?.ValorUnidad ?? 0) * uso1)));
            rowsTarifa.Add(($"41-80: {uso2}M3 x {Cur(t2?.ValorUnidad)}", Cur((t2?.ValorUnidad ?? 0) * uso2)));
            rowsTarifa.Add(($">80: {uso3}M3 x {Cur(t3?.ValorUnidad)}", Cur((t3?.ValorUnidad ?? 0) * uso3)));
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize(165, 1200));
                page.Margin(6);
                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    // Cabecera
                    col.Item().AlignCenter().Text($"Factura: {Txt(codigoFactura)}").FontSize(11).SemiBold();
                    if (logoBytes != null)
                        col.Item().AlignCenter().Width(135).Image(logoBytes).FitWidth(); // logo más grande
                    col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter()
                        .Text("JUNTA DE ACCIÓN COMUNAL TOBIA CHICA\nMUNICIPIO DE NOCAIMA - CUNDINAMARCA\nNIT: 832.003.939-2").FontSize(7);
                    col.Item().LineHorizontal(0.5f);

                    // Periodo y fechas
                    col.Item().Text($"Bimestre: {Txt(bimestreLargo)}").FontSize(8).SemiBold();
                    col.Item().Text($"Reg. Consumo: {Fdt(f.IdplanillaNavigation?.FechaHoraRegistroConsumo, "d")} ").FontSize(7);
                    col.Item().Text($"Último pago: {Fdd(f.IdplanillaNavigation?.FechaLimitePago, "D")} ").FontSize(7);
                    col.Item().Text($"Suspensión: {Fdd(f.IdplanillaNavigation?.FechaSuspension, "D")} ").FontSize(7);
                    col.Item().LineHorizontal(0.5f);

                    // Predio y mediciones
                    col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text("DATOS DEL PREDIO").FontSize(8).SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(); });
                        t.Cell().Border(0.5f).Padding(3).Background(Colors.White)
                            .Text(Txt(p?.ApellidosNombresUsuario)).FontSize(9).SemiBold().AlignCenter();
                    });
                    col.Item().Component(KeyValueTable(rowsPredio, 7));

                    // Fila destacada de Consumo
                    col.Item().Container().Background(Colors.Black).Padding(4).AlignCenter()
                        .Text($"Consumo M3: {Num(f.ConsumoM3)}").FontSize(9).FontColor(Colors.White).SemiBold();
                    // Consumo alert note (if provided) - render same layout as UI: header + body (both on black)
                    if (!string.IsNullOrWhiteSpace(consumoAlertNote))
                    {
                        col.Item().Container().Background(Colors.Black).Padding(4).AlignCenter()
                            .Text("ALERTA DESPERDICIO!").FontSize(9).FontColor(Colors.White).SemiBold();

                        col.Item().Container().Background(Colors.Black).Padding(3).AlignCenter()
                            .Text(consumoAlertNote).FontSize(8).FontColor(Colors.White).SemiBold();
                    }


                    // Tarifa detallada si existe
                    if (rowsTarifa.Count > 0)
                    {
                        col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text("Desglose Tarifa (rango / uso * valor)").FontSize(8).SemiBold();
                        col.Item().Component(KeyValueTable(rowsTarifa.ToArray(), 7));
                    }

                    // Valores / porcentajes
                    col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text("VALORES BIMESTRE").FontSize(8).SemiBold();
                    col.Item().Component(KeyValueTable(rowsValores, 7));

                    // Recaudo info
                    var recaudoTexto = f.RecaudoPeriodoTotal.GetValueOrDefault() > 0 ? "RECAUDO EN EL BIMESTRE" : "SIN RECAUDO EN EL BIMESTRE";
                    col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text($"{recaudoTexto} {Txt(periodo?.DescripcionCorta ?? periodo?.DescripcionLarga)}").FontSize(8).SemiBold();

                    // Notas
                    col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text("NOTAS").FontSize(8).SemiBold();
                    col.Item().Text(Txt(f.NotasFactura)).FontSize(7);

                    // Reporte Cuenta
                    col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text("REPORTE CUENTA").FontSize(8).SemiBold();
                    col.Item().Component(KeyValueTable(rowsCuenta, 7));

                    // Caja de Deuda Total
                    col.Item().Container().Background(Colors.Black).Padding(5).AlignCenter()
                        .Text($"Deuda Total: {Cur(deudaTotal)}").FontSize(11).FontColor(Colors.White).SemiBold();

                    // Consumos históricos (gráfico)
                    if (chartBytes != null)
                    {
                        col.Item().Container().Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text("Consumos Bimestrales Totales").FontSize(8).SemiBold();
                        col.Item().Width(150).Image(chartBytes).FitWidth();
                    }

                    col.Item().LineHorizontal(0.5f);
                    col.Item().AlignCenter().Text("CONSIGNAR CUENTA AHORROS BANCOLOMBIA # 89233904466\nNIT:832003939-2. JUNTA DE ACCIÓN COMUNAL TOBIA").FontSize(7);
                    col.Item().AlignCenter().Text("HORARIO CASA COMUNAL: MAR, JUE Y SÁB 3:00PM - 5:00PM").FontSize(7);
                });
            });
        }).GeneratePdf();
    }

    private IComponent KeyValueTable((string,string)[] rows, float fontSize = 6f) => new KeyValueTableComponent(rows, fontSize);

    private sealed class KeyValueTableComponent : IComponent
    {
        private readonly (string Label,string Value)[] _rows;
        private readonly float _fontSize;
        public KeyValueTableComponent((string Label,string Value)[] rows, float fontSize)
        { _rows = rows; _fontSize = fontSize; }
        public void Compose(IContainer container)
        {
            container.Table(t =>
            {
                t.ColumnsDefinition(c => { c.ConstantColumn(72); c.RelativeColumn(); });
                foreach (var r in _rows)
                {
                    t.Cell().Border(0.5f).PaddingHorizontal(2).PaddingVertical(1).Background(Colors.Grey.Lighten4).Text(r.Label).FontSize(_fontSize).SemiBold();
                    t.Cell().Border(0.5f).PaddingHorizontal(2).PaddingVertical(1).Text(r.Value).FontSize(_fontSize);
                }
            });
        }
    }

    private static string StripHtml(string html) => System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
}
