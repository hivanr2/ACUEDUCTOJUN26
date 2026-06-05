window.renderConsumoChart = (data, elementId) => {
  try {
    const id = elementId || 'consumoChart';
    const canvas = document.getElementById(id);
    if (!canvas) {
      console.warn('renderConsumoChart: canvas not found for id', id);
      return;
    }
    // Asegurar altura visible si el contenedor no define alto
    const parent = canvas.parentElement;
    const parentH = parent ? parent.clientHeight : 0;
    if (!parentH || parentH < 50) {
      // Forzar una altura mínima
      if (!canvas.style.height) canvas.style.height = '280px';
      if (!canvas.height) canvas.height = 280;
    }

    const ctx = canvas.getContext('2d');
    const labels = (data || []).map(d => d.periodo || d.Periodo);
    const values = (data || []).map(d => d.totalConsumoM3 ?? d.TotalConsumoM3 ?? 0);

    // Fallback simple sin Chart.js
    if (!window.Chart) {
      const W = canvas.clientWidth || canvas.width || 400;
      const H = canvas.clientHeight || canvas.height || 280;
      canvas.width = W; canvas.height = H;
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
      labels.forEach((l, i) => {
        ctx.fillText(String(l ?? ''), 10 + i * barW, H - 2);
      });
      return;
    }

    // Si existe Chart.js, usarlo y limpiar si ya existe uno
    if (canvas._chartInstance) {
      try { canvas._chartInstance.destroy(); } catch { }
    }

    const chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Consumo M3',
          data: values,
          backgroundColor: 'rgba(13,110,253,.5)',
          borderColor: 'rgba(13,110,253,1)',
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: { beginAtZero: true }
        }
      }
    });
    canvas._chartInstance = chart;
  } catch (e) {
    console.error('renderConsumoChart error', e);
  }
};

// Nueva función para gráfico de Recaudos en pesos
window.renderRecaudosChart = (data, elementId) => {
  try {
    const id = elementId || 'recaudosChart';
    const canvas = document.getElementById(id);
    if (!canvas) { console.warn('renderRecaudosChart: canvas not found for id', id); return; }
    const parent = canvas.parentElement;
    const parentH = parent ? parent.clientHeight : 0;
    if (!parentH || parentH < 50) {
      if (!canvas.style.height) canvas.style.height = '280px';
      if (!canvas.height) canvas.height = 280;
    }
    const ctx = canvas.getContext('2d');
    const labels = (data || []).map(d => d.periodo || d.Periodo);
    const values = (data || []).map(d => d.totalRecaudo ?? d.TotalRecaudo ?? 0);
    if (!window.Chart) {
      const W = canvas.clientWidth || canvas.width || 400;
      const H = canvas.clientHeight || canvas.height || 280;
      canvas.width = W; canvas.height = H;
      ctx.clearRect(0, 0, W, H);
      const max = Math.max(1, ...values);
      const barW = Math.max(10, (W - 20) / Math.max(values.length, 1));
      values.forEach((v, i) => {
        const h = Math.round((v / max) * (H - 30));
        ctx.fillStyle = '#198754';
        ctx.fillRect(10 + i * barW, H - h - 10, barW - 6, h);
      });
      ctx.fillStyle = '#6c757d';
      ctx.font = '10px sans-serif';
      labels.forEach((l, i) => ctx.fillText(String(l ?? ''), 10 + i * barW, H - 2));
      return;
    }
    if (canvas._chartInstance) { try { canvas._chartInstance.destroy(); } catch { } }
    const chart = new Chart(ctx, {
      type: 'bar',
      data: { labels, datasets: [{ label: 'Recaudo pesos', data: values, backgroundColor: 'rgba(25,135,84,.5)', borderColor: 'rgba(25,135,84,1)', borderWidth: 1 }] },
      options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { callback: v => '$' + v.toLocaleString('es-CO') } } }, plugins: { tooltip: { callbacks: { label: ctx => '$' + ctx.parsed.y.toLocaleString('es-CO') } } } }
    });
    canvas._chartInstance = chart;
  } catch (e) { console.error('renderRecaudosChart error', e); }
};
