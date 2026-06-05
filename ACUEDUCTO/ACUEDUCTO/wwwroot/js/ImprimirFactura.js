// ImprimirFactura.js - app-level client helpers (charts, printing, toasts)

window.imprimirVentana = function () {
  window.print();
};

window.renderConsumoChart = function (series, canvasId, label) {
  try {
    const targetId = canvasId || 'consumoChart';
    const canvas = document.getElementById(targetId);
    if (!canvas) {
      console.warn('renderConsumoChart: canvas #' + targetId + ' no encontrado');
      return;
    }
    const ctx = canvas.getContext('2d');
    const labels = (series || []).map(c => c.periodo || c.Periodo);
    const values = (series || []).map(c => c.totalRecaudo ?? c.TotalRecaudo ?? c.totalConsumoM3 ?? c.TotalConsumoM3 ?? 0);
    const resolvedLabel = label || ((series && series.length && (series[0].TotalRecaudo !== undefined || series[0].totalRecaudo !== undefined)) ? 'Recaudos ($)' : 'Consumo M3');

    if (!window.Chart) {
      // simple fallback
      const W = canvas.width, H = canvas.height;
      ctx.clearRect(0, 0, W, H);
      const max = Math.max(1, ...values);
      const barW = Math.max(10, (W - 20) / Math.max(values.length, 1));
      values.forEach((v, i) => {
        const h = Math.round((v / max) * (H - 30));
        ctx.fillStyle = '#0d6efd';
        ctx.fillRect(10 + i * barW, H - h - 10, barW - 6, h);
      });
      ctx.fillStyle = '#6c757d';
      ctx.font = '10px sans-serif';
      labels.forEach((l, i) => ctx.fillText(String(l ?? ''), 10 + i * barW, H - 2));
      return;
    }

    const existing = canvas._chartInstance;
    if (existing && typeof existing.destroy === 'function') existing.destroy();

    const chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: resolvedLabel,
          data: values,
          backgroundColor: 'rgba(13,110,253,0.25)',
          borderColor: 'rgba(13,110,253,1)',
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: { y: { beginAtZero: true } }
      }
    });

    canvas._chartInstance = chart;
  } catch (e) {
    console.error('renderConsumoChart error', e);
  }
};

window.getCanvasPngBase64 = function (canvasId) {
  try {
    const id = canvasId || 'consumoChart';
    const canvas = document.getElementById(id);
    if (!canvas) return null;
    const dataUrl = canvas.toDataURL('image/png');
    const parts = dataUrl.split(',');
    return parts.length > 1 ? parts[1] : dataUrl;
  } catch (e) {
    console.error('getCanvasPngBase64 error', e);
    return null;
  }
};

window.showToast = function (message, duration) {
  try {
    duration = duration || 2000;
    const id = 'acu-toast-container';
    let container = document.getElementById(id);
    if (!container) {
      container = document.createElement('div');
      container.id = id;
      container.style.position = 'fixed';
      container.style.zIndex = 2000;
      container.style.right = '12px';
      container.style.bottom = '12px';
      container.style.display = 'flex';
      container.style.flexDirection = 'column';
      container.style.gap = '8px';
      document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = 'alert alert-success';
    toast.style.minWidth = '200px';
    toast.style.boxShadow = '0 2px 6px rgba(0,0,0,0.2)';
    toast.style.opacity = '0';
    toast.style.transition = 'opacity 200ms ease-in-out, transform 200ms ease-in-out';
    toast.style.transform = 'translateY(8px)';
    toast.innerText = message || '';

    container.appendChild(toast);
    void toast.offsetHeight;
    toast.style.opacity = '1';
    toast.style.transform = 'translateY(0)';

    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateY(8px)';
      setTimeout(() => { try { container.removeChild(toast); } catch { } }, 220);
    }, duration);
  } catch (e) {
    console.error('showToast error', e);
  }
};

// Print a single QR label per page. Falls back to in-page print if popups blocked.
window.imprimirQrEtiqueta = function (qrUrl, number, description) {
  try {
    number = number || '';
    // prepare printed-on label (Mes Año) in Spanish
    const _months = ['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];
    const _d = new Date();
    const _printedOn = _months[_d.getMonth()] + ' ' + _d.getFullYear();
    // Responsive label that fills the printable page area
    const html = '<!doctype html><html><head><meta charset="utf-8"><title>Imprimir Etiqueta</title>' +
      '<style>@page{margin:0;}html,body{height:100%;margin:0;padding:0;}body{background:#fff;margin:0;padding:0;} .page{width:100%;height:100vh;display:flex;align-items:center;justify-content:center;box-sizing:border-box;padding:4mm;} .label{width:100%;height:100%;box-sizing:border-box;padding:6mm;display:flex;align-items:center;justify-content:center;} .card{width:100%;height:100%;display:flex;flex-direction:row;align-items:center;gap:4%;} .left{width:30%;display:flex;flex-direction:column;align-items:center;justify-content:center;} .right{width:70%;display:flex;flex-direction:column;align-items:flex-start;justify-content:center;} .title{font-size:1.6vw;color:#666;letter-spacing:0.4px} .big{font-size:6.6vw;font-weight:800;color:#111} .sub{font-size:2.4vw;color:#444;margin-top:6px} .logo{max-width:80%;height:auto;border-radius:6px;} .qrimg{max-width:80%;height:auto;margin-top:6px;} @media print{ .big{font-size:28pt;} .title{font-size:10pt;} .sub{font-size:8pt;} }</style>' +
      '</head><body>' +
      '<div class="page"><div class="label"><div class="card">' +
      '<div class="left">' +
      '<img src="/img/favicon-drop.svg" alt="JAC" class="logo" onerror="this.style.display=\'none\'" />' +
      (qrUrl ? ('<img src="' + qrUrl + '" alt="QR" class="qrimg"/>') : '') +
      '</div>' +
      '<div class="right">' +
      '<div class="title">INVENTARIO</div>' +
      '<div class="big">' + number + ' / 1</div>' +
      (description ? ('<div class="sub">' + description + '</div>') : '<div class="sub">Etiqueta oficial - JAC</div>') +
      // printed-on date placed inside card to avoid clipping by printers
      '<div style="margin-top:12px;color:#777;font-size:2.0vw;align-self:flex-end;">' + _printedOn + '</div>' +
      '</div>' +
      '</div></div></div>' +
      '<script>window.onload=function(){setTimeout(function(){window.print();setTimeout(function(){try{window.close();}catch(e){}},400);},250);};</' + 'script></body></html>';

    let win = null;
    try { win = window.open('', '_blank'); } catch (e) { win = null; }
    if (win) { win.document.open(); win.document.write(html); win.document.close(); return true; }

    // fallback in-page
    const container = document.createElement('div');
    container.innerHTML = html;
    container.style.position = 'fixed'; container.style.left = '0'; container.style.top = '0'; container.style.width = '100%'; container.style.height = '100%'; container.style.background = 'white'; container.style.zIndex = 99999;
    document.body.appendChild(container);
    setTimeout(() => { try { window.print(); } catch (e) { console.warn(e); } setTimeout(() => { try { document.body.removeChild(container); } catch (e) { } }, 400); }, 300);
    return false;
  } catch (e) {
    console.error('imprimirQrEtiqueta error', e);
    return false;
  }
};

window.printQrFromApp = window.imprimirQrEtiqueta;
window.imprimirExiste = function () { try { return typeof window.imprimirQrEtiqueta === 'function'; } catch { return false; } };

// New function: imprimirQrTiras - print multiple labels vertically
window.imprimirQrTiras = function (qrUrl, numberStart, copies) {
  try {
    // accepts optional 4th parameter description
    const description = arguments.length >= 4 ? (arguments[3] || '') : '';
    copies = Math.max(1, parseInt(copies) || 1);
    const pages = [];
    const _months = ['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];
    const _d = new Date();
    const _printedOn = _months[_d.getMonth()] + ' ' + _d.getFullYear();
    for (let i = 0; i < copies; i++) {
      const n = (parseInt(numberStart) || 1) + i;
      // Layout: left column with logo (and optional small QR below), right column with INVENTARIO X/Y and large number
      const label =
        '<div style="width:100%;height:100vh;display:flex;align-items:center;justify-content:center;box-sizing:border-box;padding:6mm;">' +
        '<div style="width:340px;border:1px solid #222;border-radius:10px;padding:12px;display:flex;flex-direction:row;align-items:center;justify-content:space-between;background:white;box-shadow:0 6px 18px rgba(0,0,0,0.06);font-family:Arial,Helvetica,sans-serif;">' +
        '  <div style="display:flex;flex-direction:column;align-items:center;justify-content:center;width:110px;">' +
        '    <img src="/img/favicon-drop.svg" alt="JAC" style="width:72px;height:72px;object-fit:contain;border-radius:8px;" onerror="this.style.display=\'none\'" />' +
        (qrUrl ? ('    <img src="' + qrUrl + '" alt="QR" style="width:72px;height:72px;object-fit:contain;margin-top:8px;"/>') : '') +
        '  </div>' +
        '  <div style="flex:1;display:flex;flex-direction:column;align-items:flex-start;padding-left:12px;">' +
        '    <div style="font-size:14px;color:#666;letter-spacing:0.6px;padding-bottom:6px;">INVENTARIO</div>' +
        '    <div style="font-size:26px;font-weight:800;color:#111;">' + n + ' / ' + copies + '</div>' +
        (description ? ('    <div style="margin-top:10px;font-size:12px;color:#444;">' + description + '</div>') : '    <div style="margin-top:10px;font-size:12px;color:#444;">Etiqueta oficial - JAC</div>') +
        '    <div style="margin-top:10px;color:#777;font-size:10px;align-self:flex-end;">' + _printedOn + '</div>' +
        '  </div>' +
        '</div>' +
        '</div>';
      pages.push(label);
    }

    const win = window.open('', '_blank');
    if (!win) {
      return window.imprimirQrEtiqueta(qrUrl, numberStart);
    }

    win.document.open();
    win.document.write('<!doctype html><html><head><meta charset="utf-8"><title>Imprimir Tiras</title>');
    win.document.write('<style>@page{margin:4mm;} body{margin:0;padding:0;background:white;}</style>');
    win.document.write('</head><body>');
    for (const p of pages) win.document.write(p);
    // add printed-on footer for each page (CSS fixed position)
    win.document.write('<div style="position:fixed;right:6mm;bottom:4mm;color:#777;font-size:10px;">' + _printedOn + '</div>');
    win.document.write('<script>window.onload=function(){setTimeout(function(){window.print();setTimeout(function(){try{window.close();}catch(e){}},300);},200);};</' + 'script>');
    win.document.write('</body></html>');
    win.document.close();
    return true;
  } catch (e) {
    console.error('imprimirQrTiras error', e);
    return false;
  }
};
