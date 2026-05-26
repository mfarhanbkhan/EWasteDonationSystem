/* Documented for easier understanding. */

const Store = (() => {
  const KEY = "donation_ui_store_v1";

  const seed = () => ({
    meta: { seededAt: new Date().toISOString() },
    donors: [
      {
        id: "DNR-" + Math.floor(100000 + Math.random()*899999),
        fullName: "Ayesha Khan",
        phone: "+92 300 1234567",
        city: "Karachi",
        address: "Block 7, Gulshan-e-Iqbal",
        email: "ayesha@example.com",
        items: [{ itemName:"Old Laptop", qty:1, category:"E-Waste", condition:"Working", notes:"Battery weak" }],
        status: "pending",
        createdAt: new Date(Date.now() - 86400000*2).toISOString(),
        chat: [
          { by:"admin", text:"Assalam o Alaikum Ayesha. Please share pickup time window.", at: new Date(Date.now()-86400000).toISOString() },
          { by:"donor", text:"Walaikum Salam. Any day after 6pm is fine.", at: new Date(Date.now()-86000000).toISOString() }
        ]
      },
      {
        id: "DNR-" + Math.floor(100000 + Math.random()*899999),
        fullName: "Muhammad Ali",
        phone: "+92 312 5550199",
        city: "Lahore",
        address: "Johar Town, Street 4",
        email: "m.ali@example.com",
        items: [{ itemName:"Mobile Phones", qty:3, category:"E-Waste", condition:"Not Working", notes:"For recycling" }],
        status: "approved",
        createdAt: new Date(Date.now() - 86400000*5).toISOString(),
        chat: [
          { by:"admin", text:"Thanks! Your donation is approved. We'll assign pickup agent.", at: new Date(Date.now()-86400000*4).toISOString() }
        ]
      }
    ],
    students: [
      {
        id: "STD-" + Math.floor(100000 + Math.random()*899999),
        fullName: "Hira Noor",
        phone: "+92 331 7788123",
        institute: "Govt College",
        city: "Islamabad",
        address: "G-10 Markaz",
        needItems: "Laptop for studies (any working condition).",
        reason: "Final year project + online classes.",
        status: "pending",
        createdAt: new Date(Date.now() - 86400000*1).toISOString(),
        chat: [
          { by:"admin", text:"Hi Hira, please share your student ID photo in real system. (UI demo)", at: new Date(Date.now()-70000000).toISOString() }
        ]
      }
    ],
    agents: [
      { id:"AGT-101", name:"Pickup Team A - Hamza", phone:"+92 300 8887711", area:"Karachi" },
      { id:"AGT-102", name:"Pickup Team B - Sana", phone:"+92 301 4442200", area:"Lahore" }
    ],
    assignments: [] // { donorId, agentId, assignedAt }
  });

  const load = () => {
    try{
      const raw = localStorage.getItem(KEY);
      if(!raw){
        const s = seed();
        localStorage.setItem(KEY, JSON.stringify(s));
        return s;
      }
      return JSON.parse(raw);
    }catch(e){
      const s = seed();
      localStorage.setItem(KEY, JSON.stringify(s));
      return s;
    }
  };

  const save = (data) => localStorage.setItem(KEY, JSON.stringify(data));

  const get = () => load();

  const update = (mutator) => {
    const data = load();
    mutator(data);
    save(data);
    return data;
  };

  const findById = (arr, id) => arr.find(x => x.id === id);

  const fmtDate = (iso) => {
    try{
      const d = new Date(iso);
      return d.toLocaleString();
    }catch(_){ return iso || ""; }
  };

  const statusBadge = (status) => {
    if(status === "approved") return `<span class="badge ok">Approved</span>`;
    if(status === "rejected") return `<span class="badge bad">Rejected</span>`;
    return `<span class="badge warn">Pending</span>`;
  };

  return { KEY, get, update, findById, fmtDate, statusBadge };
})();

function qs(sel){ return document.querySelector(sel); }
function qsa(sel){ return [...document.querySelectorAll(sel)]; }

function toast(msg){
  const t = document.createElement("div");
  t.style.position="fixed";
  t.style.bottom="18px";
  t.style.left="50%";
  t.style.transform="translateX(-50%)";
  t.style.padding="12px 14px";
  t.style.border="1px solid rgba(255,255,255,.18)";
  t.style.borderRadius="14px";
  t.style.background="rgba(15,23,48,.92)";
  t.style.backdropFilter="blur(10px)";
  t.style.boxShadow="0 20px 60px rgba(0,0,0,.45)";
  t.style.color="rgba(233,238,255,.95)";
  t.style.fontSize="13px";
  t.style.zIndex="9999";
  t.textContent = msg;
  document.body.appendChild(t);
  setTimeout(()=> t.remove(), 2200);
}

/**
 * Geolocation helpers (used by Donor/Student forms)
 * - Fetches browser GPS coordinates via navigator.geolocation
 * - Attempts reverse geocode via OpenStreetMap Nominatim (best-effort)
 */
async function reverseGeocodeOSM(lat, lon){
  try{
    const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${encodeURIComponent(lat)}&lon=${encodeURIComponent(lon)}`;
    const res = await fetch(url, { headers: { "Accept":"application/json" } });
    if(!res.ok) return "";
    const j = await res.json();
    return (j && (j.display_name || j.name)) ? (j.display_name || j.name) : "";
  }catch(e){
    return "";
  }
}

async function fetchAndFillLocation({ addressEl, latEl, lonEl, buttonEl }){
  if(!("geolocation" in navigator)){
    toast("Geolocation not supported in this browser");
    return;
  }
  if(buttonEl){ buttonEl.disabled = true; buttonEl.textContent = "Fetching..."; }

  const done = () => {
    if(buttonEl){ buttonEl.disabled = false; buttonEl.textContent = "Fetch Location"; }
  };

  navigator.geolocation.getCurrentPosition(async (pos)=>{
    try{
      const lat = pos.coords.latitude.toFixed(6);
      const lon = pos.coords.longitude.toFixed(6);
      if(latEl) latEl.value = lat;
      if(lonEl) lonEl.value = lon;
      let addr = "";
      if(navigator.onLine){
        addr = await reverseGeocodeOSM(lat, lon);
      }
      if(addressEl){
        addressEl.value = addr || `Lat: ${lat}, Lon: ${lon}`;
      }
      toast("Location captured");
    }finally{
      done();
    }
  }, (err)=>{
    done();
    toast(err && err.message ? err.message : "Location permission denied");
  }, { enableHighAccuracy:true, timeout:12000, maximumAge:0 });
}

function readLocationFromInputs({ addressEl, latEl, lonEl }){
  const lat = (latEl && latEl.value) ? latEl.value.trim() : "";
  const lon = (lonEl && lonEl.value) ? lonEl.value.trim() : "";
  const address = (addressEl && addressEl.value) ? addressEl.value.trim() : "";
  if(!lat || !lon) return null;
  return { lat, lon, address };
}

function formatLocation(loc){
  if(!loc) return "—";
  if(loc.address && !/^Lat:\s*/i.test(loc.address)) return loc.address;
  return `Lat: ${loc.lat}, Lon: ${loc.lon}`;
}


function getParam(name){
  const u = new URL(location.href);
  return u.searchParams.get(name);
}


// --- Admin access gate (simple UI demo) ---
const AdminAuth = (() => {
  const KEY = "ewaste_admin_auth_v1";
  const CODE = "admin123"; // demo code (change anytime)

  const isAuthed = () => {
    try{
      const raw = localStorage.getItem(KEY);
      if(!raw) return false;
      const obj = JSON.parse(raw);
      return obj && obj.ok === true && (Date.now() - (obj.at||0)) < 6*60*60*1000;
    }catch(e){ return false; }
  };

  const setAuthed = () => localStorage.setItem(KEY, JSON.stringify({ ok:true, at: Date.now() }));
  const logout = () => localStorage.removeItem(KEY);

  const require = () => {
    if(isAuthed()) return true;
    const input = window.prompt("Admin access only.\nEnter Admin Code:", "");
    if(input === null) return false;
    if(String(input).trim() === CODE){
      setAuthed();
      toast("Admin access granted");
      return true;
    }
    toast("Wrong admin code");
    return false;
  };

  return { isAuthed, require, logout };
})();
