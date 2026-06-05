(function(){
  if(!('serviceWorker' in navigator)) return;
  window.swRegister = {
    install: function(){
      try{
        navigator.serviceWorker.register('/service-worker.js')
          .then(reg => console.log('[SW] registrado', reg.scope))
          .catch(err => console.warn('[SW] fallo', err));
      }catch(e){ console.warn('[SW] error', e); }
    }
  };
})();
