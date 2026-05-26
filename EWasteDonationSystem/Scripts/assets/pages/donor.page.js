/* Documented for easier understanding. */
let activeId = null;
    let pendingImgData = null;

    // Location capture
    function bindLocation(){
      const btn = qs("#fetchLocBtn");
      if(!btn) return;
      btn.addEventListener("click", ()=>{
        fetchAndFillLocation({
          addressEl: qs("#locAddress"),
          latEl: qs("#locLat"),
          lonEl: qs("#locLon"),
          buttonEl: btn
        });
      });
    }

    function renderSelect(){
      const s = Store.get();
      const sel = qs("#donorSelect");
      sel.innerHTML = "";
      s.donors.forEach(d=>{
        const o = document.createElement("option");
        o.value = d.id;
        o.textContent = `${d.fullName} (${d.id})`;
        sel.appendChild(o);
      });
      if(!activeId && s.donors[0]) activeId = s.donors[0].id;
      if(activeId) sel.value = activeId;
      setActive(activeId);
    }

    function setActive(id){
      activeId = id;
      const s = Store.get();
      const d = Store.findById(s.donors, id);
      if(!d) return;
      qs("#activeDonorChip").textContent = `Active Donor: ${d.fullName}`;
      qs("#donorStatus").innerHTML = Store.statusBadge(d.status);
      qs("#fullName").value = d.fullName || "";
      qs("#phone").value = d.phone || "";
      qs("#email").value = d.email || "";
      qs("#city").value = d.city || "";
      qs("#address").value = d.address || "";
      qs("#openDetailBtn").href = `donor-detail.html?id=${encodeURIComponent(d.id)}`;
      renderChat();
    }

    function renderTable(){
      const s = Store.get();
      const tb = qs("#donorTable tbody");
      tb.innerHTML = "";
      s.donors
        .slice()
        .sort((a,b)=> (b.createdAt||"").localeCompare(a.createdAt||""))
        .forEach(d=>{
          const last = d.items?.[d.items.length-1];
          const tr = document.createElement("tr");
          tr.innerHTML = `
            <td>
              <div style="font-weight:700">${d.fullName}</div>
              <div class="small">${d.phone || ""}</div>
            </td>
            <td>${d.city || ""}</td>
            <td>${last ? `${last.itemName} (x${last.qty})` : "-"}</td>
            <td>${(() => { const src = last?.img || "../assets/item-placeholder.svg"; return `<img class="thumb" src="${src}" alt="item">`; })()}</td>
            <td>${Store.statusBadge(d.status)}</td>
            <td><a class="iconbtn" title="View Detail" href="donor-detail.html?id=${encodeURIComponent(d.id)}">🔎</a></td>
          `;
          tb.appendChild(tr);
        });
    }

    function renderChat(){
      const s = Store.get();
      const d = Store.findById(s.donors, activeId);
      const box = qs("#chatMsgs");
      box.innerHTML = "";
      if(!d) return;
      (d.chat || []).forEach(m=>{
        const div = document.createElement("div");
        div.className = "bubble " + (m.by === "donor" ? "me" : "them");
        div.innerHTML = `<div>${m.text}</div><div class="small" style="margin-top:6px">${Store.fmtDate(m.at)}</div>`;
        box.appendChild(div);
      });
      box.scrollTop = box.scrollHeight;
    }

    qs("#donorSelect").addEventListener("change", (e)=> setActive(e.target.value));

    qs("#newDonorBtn").addEventListener("click", ()=>{
      activeId = null;
      qs("#activeDonorChip").textContent = "Active Donor: New";
      qs("#donorStatus").innerHTML = Store.statusBadge("pending");
      ["fullName","phone","email","city","address"].forEach(id=> qs("#"+id).value = "");
      qs("#openDetailBtn").href = "#";
      qs("#chatMsgs").innerHTML = `<div class="bubble them">Create donor profile first, then chat will appear here.</div>`;
      toast("New donor mode");
    });

    qs("#saveProfileBtn").addEventListener("click", ()=>{
      const fullName = qs("#fullName").value.trim();
      if(!fullName){ toast("Enter Full Name"); return; }
      const phone = qs("#phone").value.trim();
      const email = qs("#email").value.trim();
      const city = qs("#city").value.trim();
      const address = qs("#address").value.trim();

      const updated = Store.update(data=>{
        if(activeId){
          const d = Store.findById(data.donors, activeId);
          if(d){
            d.fullName=fullName; d.phone=phone; d.email=email; d.city=city; d.address=address;
          }
        }else{
          const id = "DNR-" + Math.floor(100000 + Math.random()*899999);
          data.donors.unshift({
            id, fullName, phone, email, city, address,
            items: [], status: "pending", createdAt: new Date().toISOString(),
            chat: [{ by:"admin", text:"Thanks for registering. Admin will review your donation.", at: new Date().toISOString() }]
          });
          activeId = id;
        }
      });
      renderSelect();
      renderTable();
      toast("Profile saved");
    });

    qs("#itemImage").addEventListener("change", (e)=>{
      const f = e.target.files && e.target.files[0];
      if(!f){ pendingImgData = null; qs("#itemPreview").src="../assets/item-placeholder.svg"; return; }
      const reader = new FileReader();
      reader.onload = ()=>{ pendingImgData = reader.result; qs("#itemPreview").src = pendingImgData; };
      reader.readAsDataURL(f);
    });

    qs("#postItemBtn").addEventListener("click", ()=>{
      if(!activeId){ toast("Create / select donor first"); return; }
      const itemName = qs("#itemName").value.trim();
      if(!itemName){ toast("Enter Item Name"); return; }
      const qty = parseInt(qs("#qty").value || "1", 10);
      const category = qs("#category").value;
      const condition = qs("#condition").value;
      const notes = qs("#notes").value.trim();
      const location = readLocationFromInputs({ addressEl: qs("#locAddress"), latEl: qs("#locLat"), lonEl: qs("#locLon") });

      Store.update(data=>{
        const d = Store.findById(data.donors, activeId);
        if(!d) return;
        d.items = d.items || [];
        d.items.push({ itemName, qty, category, condition, notes, img: pendingImgData, location });
        d.status = d.status || "pending"; // keep current status
        d.createdAt = d.createdAt || new Date().toISOString();
        d.chat = d.chat || [];
        d.chat.push({ by:"donor", text:`I submitted donation item: ${itemName} (x${qty}).`, at: new Date().toISOString() });
      });
      qs("#itemName").value=""; qs("#qty").value="1"; qs("#notes").value="";
      pendingImgData = null; qs("#itemImage").value=""; qs("#itemPreview").src="../assets/item-placeholder.svg";
      renderTable(); renderChat();
      toast("Donation submitted (pending review)");
    });

    qs("#sendBtn").addEventListener("click", ()=>{
      if(!activeId){ toast("Select donor"); return; }
      const text = qs("#chatInput").value.trim();
      if(!text) return;
      Store.update(data=>{
        const d = Store.findById(data.donors, activeId);
        if(!d) return;
        d.chat = d.chat || [];
        d.chat.push({ by:"donor", text, at: new Date().toISOString() });
      });
      qs("#chatInput").value="";
      renderChat();
    });

    renderSelect();
    renderTable();