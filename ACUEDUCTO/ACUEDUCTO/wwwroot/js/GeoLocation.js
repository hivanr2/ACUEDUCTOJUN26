window.getGeolocation = function () {
    return new Promise((resolve, reject) => {
        try {
            const host = location.hostname || '';
            const isLocalHost = host === 'localhost' || host === '127.0.0.1' || host === '::1';
            const isSecure = (typeof isSecureContext !== 'undefined' ? isSecureContext : location.protocol === 'https:') || isLocalHost;

            if (!isSecure) {
                const msg = 'Geolocation requires HTTPS or localhost';
                console.warn('Geolocation blocked: insecure context', { host, protocol: location.protocol });
                alert(`Geolocalización: ${msg}`);
                reject(msg);
                return;
            }
            if (!('geolocation' in navigator)) {
                const msg = 'Geolocation is not supported by your browser';
                alert(`Geolocalización: ${msg}`);
                reject(msg);
                return;
            }

            const opts = { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 };
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const lat = Number(position.coords.latitude.toFixed(10));
                    const lon = Number(position.coords.longitude.toFixed(10));
                    const payload = {
                        latitud: lat,
                        longitud: lon,
                        Latitud: lat,
                        Longitud: lon,
                        accuracy: position.coords.accuracy ?? null,
                        timestamp: position.timestamp ?? Date.now()
                    };
                    alert(`Geolocalización exitosa:\nLatitud: ${lat}\nLongitud: ${lon}`);
                    resolve(payload);
                },
                (error) => {
                    let msg = 'Unable to retrieve your location';
                    switch (error.code) {
                        case error.PERMISSION_DENIED: msg = 'Permission denied for geolocation'; break;
                        case error.POSITION_UNAVAILABLE: msg = 'Position unavailable'; break;
                        case error.TIMEOUT: msg = 'Geolocation request timed out'; break;
                    }
                    console.warn('Geolocation error:', msg, error);
                    alert(`Geolocalización: ${msg}`);
                    reject(msg);
                },
                opts
            );
        } catch (e) {
            const msg = e?.message || 'Geolocation error';
            alert(`Geolocalización: ${msg}`);
            reject(msg);
        }
    });
};
