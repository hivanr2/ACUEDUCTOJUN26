window.getGeolocation = function () {
    return new Promise((resolve, reject) => {
        try {
            const host = location.hostname || '';
            const isLocalHost = host === 'localhost' || host === '127.0.0.1' || host === '::1';
            const isSecure = (typeof isSecureContext !== 'undefined' ? isSecureContext : location.protocol === 'https:') || isLocalHost;

            if (!isSecure) {
                const msg = 'La geolocalización requiere HTTPS o localhost';
                console.warn('Geolocalización bloqueada: contexto inseguro', { host, protocol: location.protocol });
                alert(`Geolocalización: ${msg}`);
                reject(msg);
                return;
            }

            if (!('geolocation' in navigator)) {
                const msg = 'Su navegador no soporta geolocalización';
                alert(`Geolocalización: ${msg}`);
                reject(msg);
                return;
            }

            const opts = { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 };
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    // Preserve the full precision provided by the browser (do not truncate)
                    const lat = position.coords.latitude;
                    const lon = position.coords.longitude;
                    const payload = {
                        latitud: lat,
                        longitud: lon,
                        Latitud: lat,
                        Longitud: lon,
                        accuracy: position.coords.accuracy ?? null,
                        timestamp: position.timestamp ?? Date.now()
                    };
                    // Optional small notification (can be removed if undesired)
                    // alert(`Geolocalización exitosa:\nLatitud: ${lat}\nLongitud: ${lon}`);
                    resolve(payload);
                },
                (error) => {
                    let msg = 'No se pudo obtener su ubicación';
                    switch (error.code) {
                        case error.PERMISSION_DENIED: msg = 'Permiso denegado para geolocalización'; break;
                        case error.POSITION_UNAVAILABLE: msg = 'Posición no disponible'; break;
                        case error.TIMEOUT: msg = 'Tiempo de espera agotado para geolocalización'; break;
                    }
                    console.warn('Error de geolocalización:', msg, error);
                    alert(`Geolocalización: ${msg}`);
                    reject(msg);
                },
                opts
            );
        } catch (e) {
            const msg = e?.message || 'Error de geolocalización';
            alert(`Geolocalización: ${msg}`);
            reject(msg);
        }
    });
};
