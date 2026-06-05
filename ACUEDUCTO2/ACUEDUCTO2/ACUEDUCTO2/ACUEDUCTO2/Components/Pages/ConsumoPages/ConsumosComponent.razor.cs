using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ACUEDUCTO2.Share.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using Facturacion = ACUEDUCTO2.Share.Models.Factura;

namespace ACUEDUCTO2.Components.Pages.ConsumoPages;

public partial class ConsumosComponent : ComponentBase
{
    [SupplyParameterFromQuery]
    private string id { get; set; }
    [SupplyParameterFromQuery]
    private string? codigo { get; set; }

    [SupplyParameterFromQuery(Name = "focus")]
    private string? focus { get; set; }

    private string? _focusedFacturaId;
    private string logoDataUri; // logo embebido sin URL pública
    private IList<Factura> facturacion;
    private string str_planilla;
    private string id_planilla_siguiente;
    private string id_planilla_anterior;
    private string str_periodo_corto;
    private string str_periodo_largo;
    private string errorMessage;
    private bool sortAscending = true;
    private double totalConsumoM3;
    private bool showModal = false;
    private Factura selectedFactura;
    private string nombreRegistrador;
    private string currentSortHeader = "Predio"; // Valor predeterminado
    private string facturaHtml = string.Empty;
    private bool editarRuta = false;
    private bool tomaDatos = false;
    private bool actualizaAnterior = false;
    private bool editarObservaciones = false;
    private Tarifa[] tarifas;
    private bool mostrar = true;
    private DateTime? FechaTomaConsumoPlanilla;
    private DateOnly? FechaUltimoPagoPlanilla;
    private DateOnly? FechaSuspensionPlanilla;

    private bool bol_termino = true;
    private bool editaPlanilla = false; // controla si se puede editar según Planilla.Abierta

    private string filtroNombrePredio = string.Empty;
    private string filtroCodigoUsuario = string.Empty;
    private System.Threading.CancellationTokenSource? _filtroCts;

    private HashSet<string> sentEmails = new();

    private bool _registroAutomaticoEnProceso;
    private string _registroAutomaticoEstado = "";

    protected override async Task OnInitializedAsync()
    {
        logoDataUri = GetLogoDataUri();
        _focusedFacturaId = string.IsNullOrWhiteSpace(focus) ? null : focus;
        await LoadData();
        CalcularTotalConsumoM3();
    }

    private void OnFocusedFacturaChanged(string idFactura)
    {
        _focusedFacturaId = idFactura;
        var uri = nav.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["focus"] = idFactura
        });
        nav.NavigateTo(uri, replace: true);
    }

    private async Task OnTomaDatosToggled()
    {
        if (!tomaDatos) return;
        await ScrollToFocusedAsync();
    }

    private async Task ScrollToFocusedAsync()
    {
        if (string.IsNullOrWhiteSpace(_focusedFacturaId)) return;
        try
        {
            await JSRuntime.InvokeVoidAsync("scrollToElement", $"factura-{_focusedFacturaId}");
        }
        catch
        {
            // ignore
        }
    }

    private void CalcularValorPeriodo(Facturacion factura)
    {
        if (factura.PorcentajeSubsidio != null && factura.PorcentajeSubsidio > 0)
        {
            factura.ValorSubsidio = -((double)(((factura.CargoFijo ?? 0)
                            + (factura.ValorConsumo ?? 0)) * factura.PorcentajeSubsidio) / 100);
        }
        else
            factura.PorcentajeSubsidio = 0;

        double valorPeriodo = (double)((factura.CargoFijo ?? 0)
                            + (factura.ValorConsumo ?? 0)
                            + (factura.ValorSobreTasa ?? 0)
                            + (factura.ValorMora ?? 0)
                            + (factura.ValorSubsidio ?? 0)
                            + (factura.OtrosValores ?? 0));

        factura.ValorPeriodo = RedondearValor(valorPeriodo);
    }

    private void CalcularValorSobreTasa(Facturacion factura)
    {
        if (factura.CargoFijo.HasValue && factura.PorcentajeSobreTasa.HasValue)
        {
            double valorSobreTasa = (double)(factura.CargoFijo.Value + (factura.ValorConsumo ?? 0)) * (factura.PorcentajeSobreTasa.Value / 100);
            factura.ValorSobreTasa = RedondearValor(valorSobreTasa);
        }
    }

    private async Task CalcularConsumo(Factura factura)
    {
        if ((factura.MedicionActual ?? 0) >= (factura.MedicionAnterior ?? 0))
        {
            factura.ConsumoM3 = (factura.MedicionActual ?? 0) - (factura.MedicionAnterior ?? 0);
        }
        else
        {
            factura.ConsumoM3 = 0;
        }

        await CalcularValorConsumo(factura);
        CalcularValorSobreTasa(factura);
        CalcularValorPeriodo(factura);
    }

    private async Task CalcularValorConsumo(Factura factura)
    {
        using var context = DbFactory.CreateDbContext();
        var tarifasDb = await context.Tarifas.ToListAsync();

        double valorConsumo = 0;
        double consumoRestante = factura.ConsumoM3 ?? 0;

        foreach (var tarifa in tarifasDb.OrderBy(t => t.Desde))
        {
            if (tarifa.ValorUnidad.HasValue)
            {
                double rango = Math.Max(0, tarifa.Hasta - (tarifa.Desde - 1));
                double rangoConsumo = Math.Min(consumoRestante, rango);
                valorConsumo += rangoConsumo * tarifa.ValorUnidad.Value;
                consumoRestante -= rangoConsumo;

                if (consumoRestante <= 0) break;
            }
        }

        factura.ValorConsumo = valorConsumo;
        await ActualizarMedicionAnteriorEnSiguienteFactura(factura);
    }

    private void CalcularTotalConsumoM3()
    {
        totalConsumoM3 = facturacion?.Sum(f => f.ConsumoM3 ?? 0) ?? 0;
    }

    private void CloseModal()
    {
        showModal = false;
    }

    private bool YaEnvioCorreoHoy(Factura factura)
    {
        try
        {
            var notas = factura?.NotaDeFactura;
            if (string.IsNullOrWhiteSpace(notas)) return false;

            var todayTag = "[MAIL:" + DateTime.Today.ToString("yyyy-MM-dd") + "]";
            if (notas.Contains(todayTag, StringComparison.OrdinalIgnoreCase)) return true;

            // Buscar fecha en las notas cerca de palabras clave de correo
            var pattern = @"(ENV.?O\s+CORREO|CORREO|MAIL)[^0-9]*(?<date>(\d{1,2}/\d{1,2}/\d{2,4}|\d{4}-\d{2}-\d{2}))";
            var matches = Regex.Matches(notas, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                var s = m.Groups["date"].Value;
                string[] fmts = new[] { "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd", "dd/MM/yy", "MM/dd/yy" };
                if (DateTime.TryParseExact(s, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    if (dt.Date == DateTime.Today) return true;
                }
                else if (DateTime.TryParse(s, out var dt2))
                {
                    if (dt2.Date == DateTime.Today) return true;
                }
            }
        }
        catch { }
        return false;
    }

    private bool PuedeEnviarCorreo(Factura factura)
    {
        // Permitido solo si no se está enviando actualmente y no se envió ya hoy
        return bol_termino && !YaEnvioCorreoHoy(factura);
    }

    private string GetCorreoDisabledTitle(Factura factura)
    {
        if (!bol_termino) return "Enviando correo...";
        if (YaEnvioCorreoHoy(factura)) return "Ya se envió un correo para esta factura hoy";
        return "Correo deshabilitado";
    }

    private async Task EnviarPorCorreo(Factura factura)
    {
        if (sentEmails.Contains(factura.Guid)) return;

        try
        {
            var email = factura.IdPredioNavigation.IdentificacionFacturacionNavigation?.CorreoElectronico;
            if (string.IsNullOrWhiteSpace(email))
            {
                await JSRuntime.InvokeVoidAsync("alert", "No se encontró el correo electrónico del usuario.");
                return;
            }

            var subject = $"Factura {factura.CodigoFactura} - {factura.IdPredioNavigation.ApellidosNombresUsuario}";
            var body = $"Adjunto encuentra la factura correspondiente para {factura.IdPredioNavigation.ApellidosNombresUsuario}. Valor a pagar: {FormatCurrency(factura.ValorPeriodo)}.";

            var chartBase64 = await BuildConsumoChartDataUri(factura.IdPredio);
            var pdfBytes = PdfService.FromFactura(factura, tarifas, logoDataUri, chartBase64);

            await EmailService.SendEmailAsync(email, subject, body, attachment: pdfBytes, attachmentName: $"{factura.CodigoFactura}.pdf");

            sentEmails.Add(factura.Guid);
            StateHasChanged();

            await JSRuntime.InvokeVoidAsync("alert", "Factura enviada por correo electrónico.");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error enviando correo: {ex.Message}");
        }
    }

    private async Task<string> BuildFacturaEmailHtml(Factura factura)
    {
        var sb = new System.Text.StringBuilder();
        var culture = new CultureInfo("es-CO");
        string L(DateTime? d) => d.HasValue ? d.Value.ToString("D", culture) : string.Empty;
        var chart = await BuildConsumoChartDataUri(factura.IdPredio);

        sb.Append("<html><head><meta charset='UTF-8'>");
        sb.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        sb.Append("<link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>");
        sb.Append("<style>.print-container{width:100%;margin:0 auto;padding:20px;box-sizing:border-box;font-family:Arial,sans-serif}table{width:100%;border-collapse:collapse;font-size:12px}th,td{border:1px solid #000;padding:8px;text-align:left;vertical-align:top}th{background-color:#f2f2f2}.text-info{color:#0d6efd}</style>");
        sb.Append("</head><body><div class='modal-content text-center' id='printableArea'>");
        sb.Append("<div class='modal-header'><h3 class='modal-title'>Factura: ");
        sb.Append(System.Net.WebUtility.HtmlEncode(factura.CodigoFactura ?? string.Empty));
        sb.Append("</h3></div><div class='modal-body'><div class='print-container'>");
        sb.Append("<div class='text-center'>");
        sb.Append($"<img src='{logoDataUri}' class='text-center' />");
        sb.Append("<h2 class='text-center'><b>JUNTA DE ACCIÓN COMUNAL TOBIA CHICA</b></h2>");
        sb.Append("<h2 class='text-center'><b>MUNICIPIO DE NOCAIMA - CUNDINAMARCA</b></h2>");
        sb.Append("<h2 class='text-center'><b>NIT: 832.003.939-2</b></h2></div>");
        sb.Append("<h2 class='text-center'><b>SERVICIO DE ACUEDUCTO</b></h2>");
        sb.Append("<h2 class='text-center'><b>JAC TOBIA CHICA</b></h2>");
        sb.Append($"<h4><b>BIMESTRE: {System.Net.WebUtility.HtmlEncode(factura.IdPeriodoNavigation?.DescripcionLarga ?? string.Empty)}</b></h4>");
        sb.Append($"<h6>Fecha Hora Elabora Factura: {factura.FechaHoraRegistro}</h6>");
        sb.Append($"<h6>Fecha Hora Registro Consumo: {factura.IdPeriodoNavigation?.FechaHoraIniciaPeriodo}</h6>");
        sb.Append("<h5>Último día Pago: </h5>");
        sb.Append($"<h4><b>{L(factura.IdPeriodoNavigation?.FechaLimitePago)}</b></h4>");
        sb.Append("<h4>Fecha Suspensión: </h4>");
        sb.Append($"<h4><b>{L(factura.IdPeriodoNavigation?.FechaSuspension)}</b></h4><hr />");
        sb.Append("<table class='table table-bordered' style='font-size:10px'><tbody>");
        void Row(string k, string v, bool big=false)
        {
            sb.Append("<tr><td><strong>"); sb.Append(k); sb.Append(":</strong></td><td"); if(big) sb.Append(" class='text-info h2'"); sb.Append("><strong>"); sb.Append(System.Net.WebUtility.HtmlEncode(v)); sb.Append("</strong></td></tr>");
        }
        Row("Predio", factura.IdPredioNavigation.ApellidosNombresUsuario ?? string.Empty, true);
        Row("Dirección", factura.IdPredioNavigation.Direccion ?? string.Empty);
        Row("Código Usuario", factura.CodigoUsuario ?? string.Empty);
        Row("Id o Cédula", factura.IdPredioNavigation.IdentificacionFacturacion ?? string.Empty);
        Row("Ruta", (factura.IdPredioNavigation.Ruta ?? 0).ToString(culture));
        Row("Medidor Número", factura.IdPredioNavigation.NumeroMedidor ?? string.Empty);
        Row("Medidor Marca", factura.IdPredioNavigation.MarcaMedidor ?? string.Empty);
        Row("Medidor Estado", factura.IdPredioNavigation.EstadoMedidor ?? string.Empty);
        Row("Medición Anterior M3", (factura.MedicionAnterior ?? 0).ToString(culture));
        Row("Medición Actual M3", (factura.MedicionActual ?? 0).ToString(culture));
        Row("Consumo M3", (factura.ConsumoM3 ?? 0).ToString(culture));
        Row("Valor Consumo", FormatCurrency(factura.ValorConsumo));
        Row("Cargo Fijo", FormatCurrency(factura.CargoFijo));
        Row("Porcentaje SobreTasa", (factura.PorcentajeSobreTasa ?? 0).ToString(culture));
        Row("Valor SobreTasa", FormatCurrency(factura.ValorSobreTasa));
        Row("Porcentaje Mora", (factura.PorcentajeMora ?? 0).ToString(culture));
        Row("Valor Mora", FormatCurrency(factura.ValorMora));
        Row("Porcentaje Subsidio", (factura.PorcentajeSubsidio ?? 0).ToString(culture));
        Row("Valor Subsidio", FormatCurrency(factura.ValorSubsidio));
        Row("Otros Valores", FormatCurrency(factura.OtrosValores));
        Row("VALOR BIMESTRE A PAGAR", FormatCurrency(factura.ValorPeriodo));
        Row("Notas", factura.NotaDeFactura ?? string.Empty);
        sb.Append("<tr><td colspan='2' class='text-center'><h3>REPORTE CUENTA</h3></td></tr>");
        Row("Deuda Bimestre Anterior", FormatCurrency(factura.DeudaAnterior));
        Row("Pagos Realizados Bimestre", FormatCurrency(factura.RecaudoPeriodoTotal));
        Row("Deuda Total", FormatCurrency(factura.DeudaPasa));
        sb.Append("<tr><td colspan='2'><h2>Consumos Bimestrales Totales</h2>");
        if(!string.IsNullOrEmpty(chart)) sb.Append($"<img src='{chart}' style='display:block;margin:6px auto 0;max-height:180px;max-width:80%' />");
        sb.Append("<hr /></td></tr>");
        sb.Append("</tbody><tfoot>");
        sb.Append("<tr><td colspan='2' class='h6'>CONSIGNAR CUENTA AHORROS BANCOLOMBIA # 89233904466. NIT:832003939-2. JUNTA DE ACCIÓN COMUNAL TOBIA</td></tr>");
        sb.Append("<tr><td colspan='1' class='h6'>HORARIO DE ATENCIÓN CASA COMUNAL:</td><td colspan='1' class='h6'>MARTES, JUEVES Y SABADO DE 2:30PM A 5:00PM:</td></tr>");
        sb.Append("</tfoot></table></div></div></div></body></html>");
        return sb.ToString();
    }

    private string BuildFacturaHtml(Facturacion f)
    {
        var culture = new CultureInfo("es-CO");
        string Format(double? v) => v.HasValue ? v.Value.ToString("C0", culture) : "$0";
        var logoSrc = GetLogoDataUri();
        var sb = new System.Text.StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'>");
        sb.Append(@"<style>
@page{size:58mm auto;margin:0}
html,body{margin:0;padding:0;-webkit-print-color-adjust:exact;print-color-adjust:exact}
.ticket{width:58mm;margin:0 auto;padding:2mm 2mm 1mm 2mm;font-family:'Courier New',Courier,monospace;font-size:11px;line-height:1.25;color:#000}
.center{text-align:center}.right{text-align:right}.small{font-size:10px}.big{font-size:13px;font-weight:700}
hr{border:none;border-top:1px dashed #000;margin:2mm 0}
.table{width:100%;border-collapse:collapse}
.table td{padding:1px 0;vertical-align:top}
.k{width:48%;text-align:right;padding-right:3px}
.v{width:52%}
</style>");
        sb.Append("</head><body>");
        sb.Append("<div id='printableArea' class='ticket'>");
        sb.Append($"<div class='center big'>Factura # {System.Net.WebUtility.HtmlEncode(f.CodigoFactura ?? "")}</div>");
        sb.Append($"<img src='{logoSrc}' style='display:block;margin:2mm auto;max-width:48mm;width:48mm;height:auto' />");
        sb.Append("<div class='center'><div><b>JUNTA DE ACCIÓN COMUNAL TOBIA CHICA</b></div><div><b>MUNICIPIO DE NOCAIMA - CUNDINAMARCA</b></div><div><b>NIT: 832.003.939-2</b></div></div>");
        sb.Append("<hr/>");
        sb.Append("<table class='table'><tbody>");
        void Row(string k, string v) => sb.Append($"<tr><td class='k'><strong>{k}:</strong></td><td class='v'>{System.Net.WebUtility.HtmlEncode(v)}</td></tr>");
        Row("Bimestre", f.IdplanillaNavigation?.NombreLargoPeriodo ?? "");
        Row("Reg.Consumo", f.IdplanillaNavigation is null ? "" : f.IdplanillaNavigation.FechaHoraIniciaPeriodo.ToString("d", culture));
        Row("Último Pago", f.IdplanillaNavigation?.FechaLimitePago?.ToString("d", culture) ?? "");
        Row("Suspensión", f.IdplanillaNavigation?.FechaSuspension?.ToString("d", culture) ?? "");
        Row("Predio", f.IdpredioNavigation?.ApellidosNombresUsuario ?? "");
        Row("Dirección", f.IdpredioNavigation?.Direccion ?? "");
        Row("Ruta", (f.IdpredioNavigation?.Ruta ?? 0).ToString(culture));
        Row("Código Usuario", f.CodigoUsuario ?? "");
        Row("Medidor #", f.IdpredioNavigation?.NumeroMedidor ?? "");
        Row("Marca", f.IdpredioNavigation?.MarcaMedidor ?? "");
        Row("Estado", f.IdpredioNavigation?.EstadoMedidor ?? "");
        Row("M3 Anterior", (f.MedicionAnterior ?? 0).ToString(culture));
        Row("M3 Actual", (f.MedicionActual ?? 0).ToString(culture));
        Row("Consumo M3", (f.ConsumoM3 ?? 0).ToString(culture));
        Row("Valor Consumo", Format(f.ValorConsumo));
        Row("Cargo Fijo", Format(f.CargoFijo));
        Row("SobreTasa", Format(f.ValorSobreTasa));
        Row("Mora", Format(f.ValorMora));
        Row("Subsidio", Format(f.ValorSubsidio));
        Row("Otros", Format(f.OtrosValores));
        sb.Append($"<tr><td class='k'><strong>VALOR A PAGAR:</strong></td><td class='v'><strong>{Format(f.ValorPeriodo)}</strong></td></tr>");
        Row("Notas", f.NotasFactura ?? "");
        sb.Append("</tbody></table>");
        sb.Append("<hr/>");
        sb.Append("<div class='small center'>CONSIGNAR CUENTA AHORROS BANCOLOMBIA # 89233904466</div>");
        sb.Append("<div class='small center'>HORARIO: MAR, JUE, SÁB 2:30pm - 5:00pm</div>");
        sb.Append("</div></body></html>");
        return sb.ToString();
    }

    private async Task PonerEnlacesAntesDespues(List<Periodo> periodosActual)
    {
        if (periodosActual == null || !periodosActual.Any())
        {
            id_planilla_anterior = "No disponible";
            id_planilla_siguiente = "No disponible";
            return;
        }
        var fechaActual = periodosActual.FirstOrDefault()?.FechaHoraIniciaPeriodo ?? DateTime.Now.Date;

        using var context = DbFactory.CreateDbContext();

        var periodoAnterior = await context.Periodos
            .Where(p => p.FechaHoraIniciaPeriodo < fechaActual)
            .OrderByDescending(p => p.FechaHoraIniciaPeriodo)
            .FirstOrDefaultAsync();

        var periodoSiguiente = await context.Periodos
            .Where(p => p.FechaHoraIniciaPeriodo > fechaActual)
            .OrderBy(p => p.FechaHoraIniciaPeriodo)
            .FirstOrDefaultAsync();

        id_planilla_anterior = periodoAnterior?.Guid ?? "No disponible";
        id_planilla_siguiente = periodoSiguiente?.Guid ?? "No disponible";
    }

    private async Task PrintTextOnlyVersion()
    {
        // Removed the '/planillas' prefix so the route matches the new component paths
        var url = $"/facturasimprimircomponent?id={id}";
        await JSRuntime.InvokeVoidAsync("open", url, "_blank");
    }

    private double RedondearValor(double valor)
    {
        int centenas = (int)(valor / 100);
        int decenas = (int)((valor % 100) / 10);

        if (decenas <= 5)
        {
            return centenas * 100;
        }
        else
        {
            return (centenas + 1) * 100;
        }
    }

    private async Task RecalcularValores(Facturacion factura)
    {
        CalcularValorPeriodo(factura);
        factura.DeudaPasa = (factura.ValorPeriodo ?? 0) + (factura.DeudaAnterior ?? 0) - (factura.RecaudoPeriodoTotal ?? 0);
    }

    private async Task UpdateFactura(Facturacion factura)
    {
        using var context = DbFactory.CreateDbContext();
        context.Facturacions.Update(factura);
        await context.SaveChangesAsync();
    }

    private async Task UpdatePlanilla()
    {
        using var context = DbFactory.CreateDbContext();
        var planilla = await context.Planillas.FirstOrDefaultAsync(p => p.Guid == id);
        if (planilla != null)
        {
            planilla.CantidadConsumoM3Total = totalConsumoM3;
            context.Planillas.Update(planilla);
            await context.SaveChangesAsync();
        }
    }

    private async Task ActualizarMedicionAnteriorEnSiguienteFactura(Factura facturaActual)
    {
        using var context = DbFactory.CreateDbContext();
        var periodoActual = await context.Periodos.FirstOrDefaultAsync(p => p.Guid == facturaActual.IdPeriodo);
        if (periodoActual == null) return;

        var periodoSiguiente = await context.Periodos
            .Where(p => p.FechaHoraIniciaPeriodo > periodoActual.FechaHoraIniciaPeriodo)
            .OrderBy(p => p.FechaHoraIniciaPeriodo)
            .FirstOrDefaultAsync();

        if (periodoSiguiente == null) return;

        var facturaSiguiente = await context.Facturas
            .FirstOrDefaultAsync(f => f.IdPeriodo == periodoSiguiente.Guid && f.IdPredio == facturaActual.IdPredio);

        if (facturaSiguiente == null) return;

        facturaSiguiente.MedicionAnterior = facturaActual.MedicionActual;

        context.Facturas.Update(facturaSiguiente);
        await context.SaveChangesAsync();
    }

    private async Task PropagarDeudaPasa(Factura facturaActual)
    {
        if (facturaActual == null) return;

        try
        {
            using var context = DbFactory.CreateDbContext();

            var periodoActual = await context.Periodos.FirstOrDefaultAsync(p => p.Guid == facturaActual.IdPeriodo);
            if (periodoActual == null) return;

            var subsequentPeriodos = await context.Periodos
                .Where(p => p.FechaHoraIniciaPeriodo > periodoActual.FechaHoraIniciaPeriodo)
                .OrderBy(p => p.FechaHoraIniciaPeriodo)
                .ToListAsync();

            double deudaPasaAnterior = facturaActual.DeudaPasa ?? 0;

            foreach (var periodo in subsequentPeriodos)
            {
                var facturaSiguiente = await context.Facturas.FirstOrDefaultAsync(f => f.IdPeriodo == periodo.Guid && f.IdPredio == facturaActual.IdPredio);
                if (facturaSiguiente == null) break;

                var ingresos = await context.Ingresos.Where(i => i.IdFactura == facturaSiguiente.Guid).ToListAsync();
                facturaSiguiente.RecaudoPeriodoTotal = ingresos.Sum(i => i.Valor ?? 0);

                facturaSiguiente.DeudaAnterior = deudaPasaAnterior;
                var valorPeriodo = facturaSiguiente.ValorPeriodo ?? 0;
                facturaSiguiente.DeudaPasa = RedondearValor((facturaSiguiente.DeudaAnterior ?? 0) + valorPeriodo - (facturaSiguiente.RecaudoPeriodoTotal ?? 0));

                context.Facturas.Update(facturaSiguiente);
                await context.SaveChangesAsync();

                deudaPasaAnterior = facturaSiguiente.DeudaPasa ?? 0;
            }
        }
        catch
        {
            // Registrar o ignorar errores para no bloquear captura de consumos
        }
    }

    private async Task ActualizaTodosFactuaAnterioraActual(string idPeriodo)
    {
        using var context = DbFactory.CreateDbContext();
        var periodoActual = await context.Periodos.Where(f => f.Guid == idPeriodo).FirstOrDefaultAsync();
        if (periodoActual == null) return;

        var facturasActuales = await context.Facturas.Where(f => f.IdPeriodo == periodoActual.Guid).ToListAsync();

        var periodoAnterior = await context.Periodos
            .Where(p => p.FechaHoraIniciaPeriodo < periodoActual.FechaHoraIniciaPeriodo)
            .OrderByDescending(p => p.FechaHoraIniciaPeriodo)
            .FirstOrDefaultAsync();

        if (periodoAnterior == null) return;

        foreach (var factura in facturasActuales)
        {
            var facturaAnterior = await context.Facturas
                .FirstOrDefaultAsync(f => f.IdPeriodo == periodoAnterior.Guid && f.IdPredio == factura.IdPredio);
            if (facturaAnterior != null)
            {
                factura.MedicionAnterior = facturaAnterior.MedicionActual;
                context.Facturas.Update(factura);
                await context.SaveChangesAsync();
            }
        }
    }

    private string GetLogoDataUri()
    {
        try
        {
            var imgPath = Path.Combine(Env.WebRootPath, "fondologo-acueducto.jpg");
            if (File.Exists(imgPath))
            {
                var bytes = File.ReadAllBytes(imgPath);
                var b64 = Convert.ToBase64String(bytes);
                return $"data:image/jpeg;base64,{b64}";
            }
        }
        catch { }
        return "/fondologo-acueducto.jpg";
    }

    private async Task<string> BuildConsumoChartDataUri(string idPredio)
    {
        try
        {
            using var context = DbFactory.CreateDbContext();
            var rows = await context.Facturas
                .Where(f => f.IdPredio == idPredio)
                .Join(context.Periodos, f => f.IdPeriodo, p => p.Guid, (f, p) => new { f.ConsumoM3, p.FechaHoraIniciaPeriodo })
                .OrderByDescending(x => x.FechaHoraIniciaPeriodo)
                .Take(12)
                .ToListAsync();

            rows.Reverse();

            const int width = 600;
            const int height = 200;
            const int padding = 30;
            const float chartLeft = padding;
            const float chartBottom = height - padding;
            const float chartRight = width - padding;
            const float chartTop = padding;

            var valores = rows.Select(r => (float)(r.ConsumoM3 ?? 0)).ToArray();
            float max = valores.Length > 0 ? Math.Max(1f, valores.Max()) : 1f;
            int n = Math.Max(1, valores.Length);
            float barWidth = (chartRight - chartLeft) / n * 0.6f;
            float step = (chartRight - chartLeft) / n;

            var sb = new System.Text.StringBuilder();
            sb.Append("<svg xmlns='http://www.w3.org/2000/svg' width='600' height='200' viewBox='0 0 600 200'>");
            sb.Append("<rect width='600' height='200' fill='white'/>");
            sb.Append("<line x1='30' y1='170' x2='570' y2='170' stroke='#808080' stroke-width='1'/>");
            sb.Append("<line x1='30' y1='170' x2='30' y2='30' stroke='#808080' stroke-width='1'/>");

            for (int i = 0; i < n; i++)
            {
                float x = chartLeft + i * step + (step - barWidth) / 2f;
                float h = (valores[i] / max) * (chartBottom - chartTop);
                sb.Append(CultureInfo.InvariantCulture, $"<rect x='{x:0.##}' y='{chartBottom - h:0.##}' width='{barWidth:0.##}' height='{h:0.##}' fill='#0d6efd'/>");
            }

            sb.Append("</svg>");
            return $"data:image/svg+xml;utf8,{Uri.EscapeDataString(sb.ToString())}";
        }
        catch
        {
            return string.Empty;
        }
    }




    private async Task SaveFacturaPdf(string idFactura)
    {
        try
        {
            using var context = DbFactory.CreateDbContext();
            var f = await context.Facturas.FirstOrDefaultAsync(x => x.Guid == idFactura);
            if (f == null) return;

            f.IdPeriodoNavigation = await context.Periodos.FirstOrDefaultAsync(p => p.Guid == f.IdPeriodo);
            f.IdPredioNavigation = await context.Predios.FirstOrDefaultAsync(p => p.IdPredio == f.IdPredio);

            string html = BuildFacturaHtml(f);
            var bytes = PdfService.FromHtml(html);

            var dir = Path.Combine(Env.WebRootPath, "facturas");
            Directory.CreateDirectory(dir);
            var fileName = Sanitize(f.CodigoFactura ?? f.Idfactura) + ".pdf";
            var fullPath = Path.Combine(dir, fileName);
            await File.WriteAllBytesAsync(fullPath, bytes);
        }
        catch (Exception ex)
        {
            errorMessage = $"Error al generar PDF: {ex.Message}";
        }
    }

    private static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var safe = string.Join("_", input.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return safe.Trim();
    }

    private string FormatCurrency(double? value)
    {
        if (value.HasValue)
        {
            return $"${value.Value:N0}";
        }
        return "$0";
    }

    private async Task ImprimirYCerrar()
    {
        await JSRuntime.InvokeVoidAsync("imprimirYCerrarModal", "printableArea");
    }

    private async Task LoadData()
    {
        using var context = DbFactory.CreateDbContext();

        // Query using current ACUEDUCTO2 model names: Facturas and Periodos
        var query = context.Facturas.Where(p => p.IdPeriodo == id);

        // If opened with ?codigo=... filter the DB query to that usuario
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var codigoTrim = codigo.Trim();
            query = query.Where(f => f.CodigoUsuario != null && f.CodigoUsuario == codigoTrim);
        }

        if (!string.IsNullOrWhiteSpace(filtroNombrePredio))
        {
            var pattern = $"%{filtroNombrePredio}%";
            query = query.Where(f => context.Predios.Any(p => p.Idpredio == f.Idpredio && p.ApellidosNombresUsuario != null && EF.Functions.Like(p.ApellidosNombresUsuario, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(filtroCodigoUsuario))
        {
            var patternCode = $"%{filtroCodigoUsuario}%";
            query = query.Where(f => f.CodigoUsuario != null && EF.Functions.Like(f.CodigoUsuario, patternCode));
        }

        facturacion = await query.Include(f => f.IdPredioNavigation)
            
            .OrderBy(c => c.IdPredioNavigation.Ruta)
            .ToListAsync();

        var planilla1 = await context.Planillas.Where(p => p.Guid == id).ToListAsync();
        await PonerEnlacesAntesDespues(planilla1);

        // Use a single planilla reference to avoid multiple FirstOrDefault() calls and null dereferences
        var planilla = planilla1.FirstOrDefault();
        var periodo = planilla;

        if (!editaPlanilla && actualizaAnterior)
        {
            await ActualizaTodosFactuaAnterioraActual(id);
            StateHasChanged();
        }

        editaPlanilla = periodo?.Abierta ?? false;

        // If opened via ?codigo=... and there is a matching factura, prepare tomaDatos behavior
        if (!string.IsNullOrWhiteSpace(codigo) && facturacion?.Count > 0)
        {
            filtroCodigoUsuario = codigo;
            // prefer the focused factura when provided, otherwise fallback to first
            Factura f = null;
            if (!string.IsNullOrWhiteSpace(focus))
            {
                f = facturacion.FirstOrDefault(x => string.Equals(x.Idfactura, focus, StringComparison.OrdinalIgnoreCase));
            }
            if (f == null) f = facturacion.First();

            var medActual = f.MedicionActual ?? 0;
            var medAnterior = f.MedicionAnterior ?? 0;

            if (medActual == 0)
            {
                // Pre-fill MedicionActual with MedicionAnterior for capture session (in-memory only)
                f.MedicionActual = medAnterior;
                // Enable data capture only if planilla allows editing
                if (editaPlanilla)
                {
                    tomaDatos = true;
                }
            }
            else
            {
                // If actual already greater than anterior, ensure capture mode is off
                tomaDatos = false;
            }
        }

        if (periodo == null)
        {
            str_planilla = "No hay planilla";
        }
        else
        {
            str_planilla = periodo.Descripcion ?? "No hay planilla";
            str_periodo_corto = periodo.NombreCortoPeriodo ?? "N/A";
            str_periodo_largo = periodo.NombreLargoPeriodo ?? "N/A";
            nombreRegistrador = periodo.Encargado ?? "N/A";

            FechaTomaConsumoPlanilla = periodo.FechaHoraIniciaPeriodo;
            FechaUltimoPagoPlanilla = periodo.FechaLimitePago.HasValue ? DateOnly.FromDateTime(periodo.FechaLimitePago.Value) : null;
            FechaSuspensionPlanilla = periodo.FechaSuspension.HasValue ? DateOnly.FromDateTime(periodo.FechaSuspension.Value) : null;
        }
        tarifas = await context.Tarifas.OrderBy(c => c.Desde).ToArrayAsync();
    }

    private async Task OpenPrintWindow(string idFactura)
    {
        // En Consumos no generamos PDF previo; solo abrimos la vista de impresión
        var url = $"/factura/imprimir?IdFactura={idFactura}";
        await JSRuntime.InvokeVoidAsync("open", url, "_blank");
    }

    private async Task OnContadorEnSeguimientoChanged(Facturacion factura, ChangeEventArgs e)
    {
        if (!editaPlanilla) return;
        bool nuevoValor = (bool)((ChangeEventArgs)e).Value;
        using var context = DbFactory.CreateDbContext();
        var predioActual = await context.Predios.FirstOrDefaultAsync(p => p.Idpredio == factura.Idpredio);
        if (predioActual != null)
        {
            predioActual.ContadorEnSeguimiento = nuevoValor;
            context.Predios.Update(predioActual);
            await context.SaveChangesAsync();
        }
        factura.IdpredioNavigation.ContadorEnSeguimiento = nuevoValor;
        StateHasChanged();
    }

    private async Task OnNotasFacturaChanged(Facturacion factura)
    {
        if (!editaPlanilla) return;
        using var context = DbFactory.CreateDbContext();
        context.Facturacions.Update(factura);
        await context.SaveChangesAsync();
    }

    private async Task OnHeaderClick(string header)
    {
        currentSortHeader = header;

        if (header == "Ruta")
        {
            if (sortAscending)
                facturacion = facturacion.OrderBy(f => f.IdpredioNavigation.Ruta).ToList();
            else
                facturacion = facturacion.OrderByDescending(f => f.IdpredioNavigation.Ruta).ToList();
            sortAscending = !sortAscending;
        }
        if (header == "Predio")
        {
            if (sortAscending)
                facturacion = facturacion.OrderBy(f => f.IdpredioNavigation.ApellidosNombresUsuario).ToList();
            else
                facturacion = facturacion.OrderByDescending(f => f.IdpredioNavigation.ApellidosNombresUsuario).ToList();
            sortAscending = !sortAscending;
        }
    }

    private async Task OnObservacionesPredioChanged(Facturacion factura, ChangeEventArgs e)
    {
        if (!editaPlanilla) return;
        factura.IdpredioNavigation.Observaciones = e.Value?.ToString();
        await UpdateFactura(factura);
    }

    private async Task OnRutaChanged(Facturacion factura, ChangeEventArgs e)
    {
        if (!editaPlanilla) return;
        if (currentSortHeader != "Ruta") return;

        if (double.TryParse(e.Value?.ToString(), out double nuevaRuta))
        {
            bool currentSortAscending = sortAscending;
            using var context = DbFactory.CreateDbContext();
            var predioActual = await context.Predios.FirstOrDefaultAsync(p => p.Idpredio == factura.Idpredio);
            if (predioActual != null)
            {
                predioActual.Ruta = nuevaRuta;
                context.Predios.Update(predioActual);
            }

            var prediosConMismaRuta = await context.Predios.Where(p => p.Ruta >= nuevaRuta).ToListAsync();
            var i = 1;
            foreach (var predio in prediosConMismaRuta)
            {
                predio.Ruta = nuevaRuta + i;
                context.Predios.Update(predio);
                i++;
            }

            await context.SaveChangesAsync();
            await LoadData();

            if (currentSortAscending)
                facturacion = facturacion.OrderByDescending(f => f.IdpredioNavigation.Ruta).ToList();
            else
                facturacion = facturacion.OrderBy(f => f.IdpredioNavigation.Ruta).ToList();

            sortAscending = currentSortAscending;
            StateHasChanged();
        }
    }

    private async Task OnMedicionActualChanged(Facturacion factura, ChangeEventArgs e)
    {
        if (!editaPlanilla) return;
        if (double.TryParse(e.Value?.ToString(), out double medicionActual))
        {
            factura.MedicionActual = medicionActual;
            await CalcularConsumo(factura);
            // Persistir cambios de medición y notas de factura si aplica
            using var context = DbFactory.CreateDbContext();

            // Si no hay notas y no hay recaudo, colocar placeholder de "SIN RECAUDO..."
            if (string.IsNullOrWhiteSpace(factura.NotasFactura) && (factura.RecaudoPeriodoTotal ?? 0) == 0)
            {
                factura.NotasFactura += "SIN RECAUDO EN EL BIMESTRE " + (str_periodo_corto ?? string.Empty);
                factura.NotasRecaudo += "SIN RECAUDO EN EL BIMESTRE " + (str_periodo_corto ?? string.Empty);
            }

            // Si no hay notas y ya existe recaudo, agradecer abono
            if (string.IsNullOrWhiteSpace(factura.NotasFactura) && (factura.RecaudoPeriodoTotal ?? 0) > 0)
            {
                factura.NotasFactura += "GRACIAS POR SU ABONO A DEUDA ANTERIOR.";
                factura.NotasRecaudo += "GRACIAS POR SU ABONO A DEUDA ANTERIOR.";
            }

            context.Facturacions.Update(factura);
            await context.SaveChangesAsync();
            CalcularTotalConsumoM3();
            await UpdatePlanilla();
            await RecalcularValores(factura);
            await UpdateFactura(factura);
            await ActualizarMedicionAnteriorEnSiguienteFactura(factura);
            // Propagar cambios de saldos a facturas de planillas posteriores
            try
            {
                await PropagarDeudaPasa(factura);
            }
            catch
            {
                // no bloquear la captura de consumos si la propagación falla
            }
            StateHasChanged();
        }
    }

    private async Task OnValorConsumoChanged(Facturacion factura, ChangeEventArgs e)
    {
        if (!editaPlanilla) return;
        if (double.TryParse(e.Value?.ToString(), out double valorConsumo))
        {
            factura.ValorConsumo = valorConsumo;
            using var context = DbFactory.CreateDbContext();
            context.Facturacions.Update(factura);
            await context.SaveChangesAsync();
            CalcularTotalConsumoM3();
            await UpdatePlanilla();
        }
    }

    private async Task OnNombreRegistradorChanged(ChangeEventArgs e)
    {
        if (!editaPlanilla) return;
        nombreRegistrador = e.Value?.ToString();
        using var context = DbFactory.CreateDbContext();
        var planilla = await context.Planillas.FirstOrDefaultAsync(p => p.Guid == id);
        if (planilla != null)
        {
            planilla.EncargadoRegistroConsumos = nombreRegistrador;
            context.Planillas.Update(planilla);
            await context.SaveChangesAsync();
        }
    }

    private void OpenModal(Facturacion factura)
    {
        selectedFactura = factura;
        showModal = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                mostrar = false;
                InvokeAsync(StateHasChanged);
            });

            if (tomaDatos)
            {
                await ScrollToFocusedAsync();
            }
        }
        await JSRuntime.InvokeVoidAsync("setPageTitle", "CON:" + str_periodo_corto);
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadData();
        CalcularTotalConsumoM3();
    }

    private async Task EnviarPorWhatsApp(Facturacion factura)
    {
        try
        {
            var telefono = factura?.IdpredioNavigation?.IdentificacionFacturacionNavigation?.Celular;
            if (!string.IsNullOrWhiteSpace(telefono))
            {
                var mensaje = $"Factura: {factura.CodigoFactura}\nValor a pagar: {FormatCurrency(factura.ValorPeriodo)}";
                await WhatsAppService.SendMessageAsync(telefono, mensaje);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar por WhatsApp: {ex.Message}");
        }
    }

    private async Task OnFiltroNombrePredioChanged()
    {
        _filtroCts?.Cancel();
        var cts = new System.Threading.CancellationTokenSource();
        _filtroCts = cts;
        try { await Task.Delay(250, cts.Token); await LoadData(); StateHasChanged(); } catch (TaskCanceledException) { }
    }

    private async Task OnFiltroCodigoUsuarioChanged()
    {
        _filtroCts?.Cancel();
        var cts = new System.Threading.CancellationTokenSource();
        _filtroCts = cts;
        try { await Task.Delay(250, cts.Token); await LoadData(); StateHasChanged(); } catch (TaskCanceledException) { }
    }

    private async Task ClearFiltroNombrePredio()
    {
        filtroNombrePredio = string.Empty;
        await LoadData();
    }

    private async Task ClearFiltroCodigoUsuario()
    {
        filtroCodigoUsuario = string.Empty;
        await LoadData();
    }

    private async Task SetFiltroCodigoUsuario(string? codigoUsuario)
    {
        if (string.IsNullOrWhiteSpace(codigoUsuario)) return;
        filtroCodigoUsuario = codigoUsuario;
        await LoadData();
        StateHasChanged();
    }


}
