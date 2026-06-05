window._acuCharts = window._acuCharts || {};


/*
  Versión simplificada y robusta:
  - SIEMPRE dibuja 2 líneas verticales (bimestre anterior):
      * Suspensión (rojo)
      * Último Día Pago (púrpura)
  - Las inserta en la secuencia de labels en su posición cronológica (si no existen).
  - No genera todas las fechas, solo añade esas dos si faltan.
  - No depende del plugin de anotaciones (las dibuja manualmente).
  - Mantiene línea horizontal del totalAnterior.
  - Firma: (canvasId, labels[], diario[], acumulado[], totalAnterior, titulo, fechaLimitePagoAnterior, fechaSuspensionAnterior)
*/
window.renderRecaudoDiarioChart = function (
  canvasId,
  labels,
  diario,
  acumulado,
  totalAnterior,
  titulo,
  fechaLimitePagoAnterior,
  fechaSuspensionAnterior
) {
  // Compatibilidad con firma antigua (antes sin totalAnterior)
  if (typeof totalAnterior === 'string' && titulo === undefined) {
    titulo = totalAnterior;
    totalAnterior = 0;
    fechaLimitePagoAnterior = undefined;
    fechaSuspensionAnterior = undefined;
  }

  if (window._acuCharts[canvasId]) {
    try { window._acuCharts[canvasId].destroy(); } catch { }
  }

  const canvas = document.getElementById(canvasId);
  if (!canvas) return;

  // Clonar datos originales
  let fullLabels = Array.isArray(labels) ? [...labels] : [];
  let diarioData = Array.isArray(diario) ? [...diario] : [];
  let acumData = Array.isArray(acumulado) ? [...acumulado] : [];

  // Recalcular acumulado si está desalineado
  if (acumData.length !== diarioData.length) {
    acumData = [];
    let r = 0;
    for (let v of diarioData) { r += Number(v) || 0; acumData.push(r); }
  }

  // Función: inserta marcador en la posición cronológica correcta
  function insertChronologically(markerDate) {
    if (!markerDate || markerDate.length !== 10) return;
    if (fullLabels.includes(markerDate)) return;
    // Buscar índice para mantener orden ascendente (yyyy-MM-dd)
    const idx = fullLabels.findIndex(l => l > markerDate);
    if (idx === -1) {
      fullLabels.push(markerDate);
      diarioData.push(0);
      // acumulado final repite el último valor
      const last = acumData.length ? acumData[acumData.length - 1] : 0;
      acumData.push(last);
    } else {
      fullLabels.splice(idx, 0, markerDate);
      diarioData.splice(idx, 0, 0);
      // Recalcular acumulado completo (simple y seguro)
      let run = 0;
      acumData = [];
      for (let v of diarioData) { run += v; acumData.push(run); }
    }
  }

  // Insertar primero la de suspensión (rojo), luego último día (púrpura)
  insertChronologically(fechaSuspensionAnterior);
  insertChronologically(fechaLimitePagoAnterior);

  const totalAnt = Number(totalAnterior) || 0;
  const lineaHorizontal = fullLabels.map(() => totalAnt);

  // Plugin manual para dibujar las 2 líneas siempre
  const verticalLinesPlugin = {
    id: 'verticalLinesAlways',
    afterDatasetsDraw(chart) {
      const { ctx, chartArea, scales } = chart;
      const xScale = scales.x;
      if (!xScale) return;

      function drawVertical(fecha, color, texto) {
        if (!fecha) return;
        const index = fullLabels.indexOf(fecha);
        if (index === -1) return;
        const x = xScale.getPixelForValue(fecha);
        ctx.save();
        // Línea
        ctx.strokeStyle = color;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(x, chartArea.top);
        ctx.lineTo(x, chartArea.bottom);
        ctx.stroke();

        // Etiqueta
        ctx.font = '600 11px sans-serif';
        const padding = 4;
        const text = texto;
        const textW = ctx.measureText(text).width;
        const boxW = textW + padding * 2;
        const boxH = 16;
        const boxX = x - boxW / 2;
        const boxY = chartArea.top - boxH - 4;

        ctx.fillStyle = color + '22';
        ctx.strokeStyle = color;
        ctx.lineWidth = 1;
        ctx.beginPath();
        // Esquinas redondeadas
        const r = 4;
        ctx.moveTo(boxX + r, boxY);
        ctx.lineTo(boxX + boxW - r, boxY);
        ctx.quadraticCurveTo(boxX + boxW, boxY, boxX + boxW, boxY + r);
        ctx.lineTo(boxX + boxW, boxY + boxH - r);
        ctx.quadraticCurveTo(boxX + boxW, boxY + boxH, boxX + boxW - r, boxY + boxH);
        ctx.lineTo(boxX + r, boxY + boxH);
        ctx.quadraticCurveTo(boxX, boxY + boxH, boxX, boxY + boxH - r);
        ctx.lineTo(boxX, boxY + r);
        ctx.quadraticCurveTo(boxX, boxY, boxX + r, boxY);
        ctx.closePath();
        ctx.fill();
        ctx.stroke();

        ctx.fillStyle = '#000';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(text, x, boxY + boxH / 2);
        ctx.restore();
      }

      drawVertical(fechaSuspensionAnterior, 'red', 'Suspensión Ant.');
      drawVertical(fechaLimitePagoAnterior, 'purple', 'Último Día Pago Ant.');
    }
  };

  window._acuCharts[canvasId] = new Chart(canvas, {
    type: 'line',
    data: {
      labels: fullLabels,
      datasets: [
        {
          label: 'Diario',
          data: diarioData,
            borderColor: 'rgba(54,162,235,1)',
          backgroundColor: 'rgba(54,162,235,0.25)',
          tension: 0.2,
          pointRadius: 3
        },
        {
          label: 'Acumulado',
          data: acumData,
          borderColor: 'rgba(75,192,192,1)',
          backgroundColor: 'rgba(75,192,192,0.15)',
          tension: 0.2,
          fill: true,
          pointRadius: 0
        },
        {
          label: 'Total Bimestre Anterior',
          data: lineaHorizontal,
          borderColor: 'rgba(255,159,64,0.9)',
          backgroundColor: 'rgba(255,159,64,0.05)',
          borderDash: [6, 6],
          pointRadius: 0,
          fill: false,
          tension: 0
        }
      ]
    },
    options: {
      responsive: true,
      interaction: { mode: 'index', intersect: false },
      stacked: false,
      plugins: {
        title: { display: true, text: titulo },
        tooltip: {
          callbacks: {
            afterBody: (items) => {
              if (!items || !items.length) return '';
              const acumItem = items.find(i => i.dataset && i.dataset.label === 'Acumulado');
              if (acumItem) {
                const actual = Number(acumItem.raw || 0);
                const diff = totalAnt - actual;
                return `Dif. vs Total Ant.: ${diff.toLocaleString('es-CO')}`;
              }
              return '';
            }
          }
        }
      },
      scales: {
        x: { type: 'category' },
        y: { beginAtZero: true }
      }
    },
    plugins: [verticalLinesPlugin]
  });
};

// Render PUEAA charts: a bar for per-category score (0-100) and a doughnut for yes/no flags
window.renderPueaaCharts = function (barCanvasId, donutCanvasId, labels, scoresPercent, yesCount, noCount, titleBar, titleDonut) {
  try {
    if (window._acuCharts[barCanvasId]) {
      try { window._acuCharts[barCanvasId].destroy(); } catch { }
    }
    if (window._acuCharts[donutCanvasId]) {
      try { window._acuCharts[donutCanvasId].destroy(); } catch { }
    }

    const barCanvas = document.getElementById(barCanvasId);
    const donutCanvas = document.getElementById(donutCanvasId);
    if (!barCanvas || !donutCanvas) return;

    window._acuCharts[barCanvasId] = new Chart(barCanvas, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Puntaje (%)',
          data: scoresPercent,
          backgroundColor: 'rgba(13,110,253,0.35)',
          borderColor: 'rgba(13,110,253,1)',
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: !!titleBar, text: titleBar || 'PUEAA por categoría' },
          legend: { display: false }
        },
        scales: {
          y: { beginAtZero: true, max: 100 }
        }
      }
    });

    window._acuCharts[donutCanvasId] = new Chart(donutCanvas, {
      type: 'doughnut',
      data: {
        labels: ['Cumple (Sí)', 'No cumple (No)'],
        datasets: [{
          data: [yesCount, noCount],
          backgroundColor: ['rgba(25,135,84,0.7)', 'rgba(220,53,69,0.7)'],
          borderColor: ['rgba(25,135,84,1)', 'rgba(220,53,69,1)'],
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: !!titleDonut, text: titleDonut || 'Cumplimiento PUEAA' }
        }
      }
    });
  } catch (e) {
    console.error('renderPueaaCharts error', e);
  }
};