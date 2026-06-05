// html2pdf client-side export helper
// Requires /js/html2pdf.bundle.min.js

window.pdfExport = {
  delay: ms => new Promise(r => setTimeout(r, ms)),

  exportElement: async function(elementId, fileName){
    if (typeof html2pdf === 'undefined') throw new Error('html2pdf.js no está cargado.');
    const el = document.getElementById(elementId);
    if(!el) throw new Error('Elemento no encontrado: ' + elementId);

    // Forzar scrollWidth correcto (algunos estilos diferidos)
    await this.delay(350);

    const opt = {
      margin:       [5, 5, 8, 5],
      filename:     fileName || 'documento.pdf',
      pagebreak:    { mode: ['css', 'legacy'], avoid: ['tr', '.no-split', '.keep-together'] },
      image:        { type: 'jpeg', quality: 0.98 },
      html2canvas:  { scale: 2, useCORS: true, logging: false, windowWidth: document.documentElement.scrollWidth },
      jsPDF:        { unit: 'mm', format: 'a4', orientation: 'portrait', compress: true }
    };

    console.log('[pdfExport] Iniciando render HTML -> PDF', opt);
    const worker = html2pdf().set(opt).from(el).toPdf();
    const blob = await worker.get('pdf').then(pdf => pdf.output('blob'));
    console.log('[pdfExport] PDF generado (blob size)', blob.size);
    return blob;
  },

  blobToBase64: async function(blob){
    return await new Promise((resolve, reject)=>{
      const reader = new FileReader();
      reader.onload = () => {
        const res = reader.result;
        if(!res || typeof res !== 'string') return reject(new Error('FileReader sin resultado'));
        const b64 = res.split(',')[1];
        resolve(b64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  },

  exportAndSave: async function(elementId, fileName, codigoUsuario){
    try {
      const safeFile = (fileName || 'documento').replace(/[^A-Za-z0-9._-]/g,'_');
      const safeUser = (codigoUsuario || 'usuario').replace(/[^A-Za-z0-9._-]/g,'_');
      const blob = await this.exportElement(elementId, safeFile);
      const base64 = await this.blobToBase64(blob);
      console.log('[pdfExport] Base64 listo, longitud', base64.length);
      const res = await this.saveToServer(base64, safeFile, safeUser);
      console.log('[pdfExport] Respuesta servidor', res);
      if(!res || !res.path){
        throw new Error('Servidor no devolvió ruta válida');
      }
      return res.path;
    } catch(err){
      console.error('[pdfExport] Error exportAndSave', err);
      throw err;
    }
  },

  saveToServer: async function(base64, fileName, codigoUsuario){
    const payload = { Base64: base64, FileName: fileName, CodigoUsuario: codigoUsuario };
    console.log('[pdfExport] POST /api/pdf/save', payload);
    const res = await fetch('/api/pdf/save', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      cache: 'no-store'
    });
    if(!res.ok){
      const txt = await res.text().catch(()=> '');
      console.error('[pdfExport] Error HTTP', res.status, txt);
      throw new Error('Error HTTP ' + res.status + (txt ? (' - ' + txt) : ''));
    }
    const json = await res.json();
    return json;
  }
};