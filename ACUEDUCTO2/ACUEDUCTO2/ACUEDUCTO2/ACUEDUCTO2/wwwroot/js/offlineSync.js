(function(){
  const DB_NAME = 'acueducto_offline';
  const DB_VERSION = 1;
  const FACT_TABLE = 'facturacionCache';
  const CHANGE_TABLE = 'pendingChanges';

  function openDb(){
    return new Promise((resolve,reject)=>{
      const req = indexedDB.open(DB_NAME, DB_VERSION);
      req.onupgradeneeded = e => {
        const db = req.result;
        if(!db.objectStoreNames.contains(FACT_TABLE)) db.createObjectStore(FACT_TABLE,{keyPath:'id'});
        if(!db.objectStoreNames.contains(CHANGE_TABLE)) db.createObjectStore(CHANGE_TABLE,{keyPath:'id'});
      };
      req.onsuccess = ()=> resolve(req.result);
      req.onerror = ()=> reject(req.error);
    });
  }

  async function put(storeName, value){
    const db = await openDb();
    return new Promise((resolve,reject)=>{
      const tx = db.transaction(storeName,'readwrite');
      tx.objectStore(storeName).put(value);
      tx.oncomplete = ()=> resolve();
      tx.onerror = ()=> reject(tx.error);
    });
  }
  async function getAll(storeName){
    const db = await openDb();
    return new Promise((resolve,reject)=>{
      const tx = db.transaction(storeName,'readonly');
      const req = tx.objectStore(storeName).getAll();
      req.onsuccess = ()=> resolve(req.result);
      req.onerror = ()=> reject(req.error);
    });
  }
  async function clear(storeName){
    const db = await openDb();
    return new Promise((resolve,reject)=>{
      const tx = db.transaction(storeName,'readwrite');
      tx.objectStore(storeName).clear();
      tx.oncomplete = ()=> resolve();
      tx.onerror = ()=> reject(tx.error);
    });
  }

  function guid(){return crypto.randomUUID ? crypto.randomUUID() : (Date.now()+Math.random()).toString(36);}  

  window.offlineSync = {
    isOnline: ()=> navigator.onLine,
    saveCache: async (json)=> {
      try{ await clear(FACT_TABLE); const arr = JSON.parse(json); if(Array.isArray(arr)){
        for(const f of arr){ await put(FACT_TABLE,{id:f.idfactura||guid(), raw:f}); }
      }}catch(e){ console.error(e); }
    },
    getCache: async ()=> {
      try{ const rows = await getAll(FACT_TABLE); return JSON.stringify(rows.map(r=>r.raw)); }catch{ return '[]'; }
    },
    queueChange: async (json)=> { try { const obj = JSON.parse(json); await put(CHANGE_TABLE,{id:guid(), raw:obj}); } catch(e){ console.error(e);} },
    getPendingChanges: async ()=> { try { const rows = await getAll(CHANGE_TABLE); return JSON.stringify(rows.map(r=>r.raw)); } catch{ return '[]'; } },
    clearPending: async ()=> { try { await clear(CHANGE_TABLE); } catch(e){ console.error(e);} }
  };

  // Auto-sync attempt when back online: raise an event for .NET if desired later
  window.addEventListener('online', ()=>{ console.log('[offlineSync] Back online'); });
})();