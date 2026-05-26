/* Documented for easier understanding. */
let activeId = null;

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
      const sel = qs("#stdSelect");
      sel.innerHTML = "";
      s.students.forEach(st=>{
        const o = document.createElement("option");
        o.value = st.id;
        o.textContent = `${st.fullName} (${st.id})`;
        sel.appendChild(o);
      });
      if(!activeId && s.students[0]) activeId = s.students[0].id;
      if(activeId) sel.value = activeId;
      setActive(activeId);
    }

    function setActive(id){
      activeId = id;
      const s = Store.get();
      const st = Store.findById(s.students, id);
      if(!st) return;
      qs("#activeStdChip").textContent = `Active: ${st.fullName}`;
      qs("#stdStatus").innerHTML = Store.statusBadge(st.status);
      qs("#fullName").value = st.fullName || "";
      qs("#phone").value = st.phone || "";
      qs("#institute").value = st.institute || "";
      qs("#city").value = st.city || "";
      qs("#address").value = st.address || "";
      qs("#needItems").value = st.needItems || "";
      qs("#reason").value = st.reason || "";
      qs("#openDetailBtn").href = `student-detail.html?id=${encodeURIComponent(st.id)}`;
      renderChat();
    }

    function renderTable(){
      const s = Store.get();
      const tb = qs("#stdTable tbody");
      tb.innerHTML = "";
      s.students
        .slice()
        .sort((a,b)=> (b.createdAt||"").localeCompare(a.createdAt||""))
        .forEach(st=>{
          const needed = (st.needItems || "").split("\n")[0].slice(0,28);
          const tr = document.createElement("tr");
          tr.innerHTML = `
            <td>
              <div style="font-weight:700">${st.fullName}</div>
              <div class="small">${st.phone || ""}</div>
            </td>
            <td>${st.institute || "-"}</td>
            <td>${needed || "-"}</td>
            <td>${Store.statusBadge(st.status)}</td>
            <td><a class="iconbtn" title="View Detail" href="student-detail.html?id=${encodeURIComponent(st.id)}">🔎</a></td>
          `;
          tb.appendChild(tr);
        });
    }

    function renderChat(){
      const s = Store.get();
      const st = Store.findById(s.students, activeId);
      const box = qs("#chatMsgs");
      box.innerHTML = "";
      if(!st) return;
      (st.chat || []).forEach(m=>{
        const div = document.createElement("div");
        div.className = "bubble " + (m.by === "student" ? "me" : "them");
        div.innerHTML = `<div>${m.text}</div><div class="small" style="margin-top:6px">${Store.fmtDate(m.at)}</div>`;
        box.appendChild(div);
      });
      box.scrollTop = box.scrollHeight;
    }

    qs("#stdSelect").addEventListener("change", (e)=> setActive(e.target.value));

    qs("#newBtn").addEventListener("click", ()=>{
      activeId = null;
      qs("#activeStdChip").textContent = "Active: New";
      qs("#stdStatus").innerHTML = Store.statusBadge("pending");
      ["fullName","phone","institute","city","address","needItems","reason"].forEach(id=> qs("#"+id).value = "");
      qs("#openDetailBtn").href = "#";
      qs("#chatMsgs").innerHTML = `<div class="bubble them">Create applicant first, then chat will appear here.</div>`;
      toast("New applicant mode");
    });

    qs("#saveBtn").addEventListener("click", ()=>{
      const fullName = qs("#fullName").value.trim();
      if(!fullName){ toast("Enter Full Name"); return; }

      const phone = qs("#phone").value.trim();
      const institute = qs("#institute").value.trim();
      const city = qs("#city").value.trim();
      const address = qs("#address").value.trim();

      Store.update(data=>{
        if(activeId){
          const st = Store.findById(data.students, activeId);
          if(st){
            st.fullName=fullName; st.phone=phone; st.institute=institute; st.city=city; st.address=address;
          }
        }else{
          const id = "STD-" + Math.floor(100000 + Math.random()*899999);
          data.students.unshift({
            id, fullName, phone, institute, city, address,
            needItems:"", reason:"",
            status:"pending",
            createdAt: new Date().toISOString(),
            chat: [{ by:"admin", text:"Thanks for applying. Admin will review your request.", at: new Date().toISOString() }]
          });
          activeId = id;
        }
      });

      renderSelect(); renderTable();
      toast("Applicant saved");
    });

    qs("#applyBtn").addEventListener("click", ()=>{
      if(!activeId){ toast("Create / select applicant first"); return; }
      const needItems = qs("#needItems").value.trim();
      const reason = qs("#reason").value.trim();
      const location = readLocationFromInputs({ addressEl: qs("#locAddress"), latEl: qs("#locLat"), lonEl: qs("#locLon") });
      if(!needItems || !reason){ toast("Fill items needed AND reason"); return; }

      Store.update(data=>{
        const st = Store.findById(data.students, activeId);
        if(!st) return;
        st.needItems = needItems;
        st.reason = reason;
        st.location = location || st.location || null;
        st.createdAt = st.createdAt || new Date().toISOString();
        st.chat = st.chat || [];
        st.chat.push({ by:"student", text:"I submitted my application with full details.", at: new Date().toISOString() });
      });

      renderTable(); renderChat();
      toast("Application submitted (pending review)");
    });

    qs("#sendBtn").addEventListener("click", ()=>{
      if(!activeId){ toast("Select applicant"); return; }
      const text = qs("#chatInput").value.trim();
      if(!text) return;
      Store.update(data=>{
        const st = Store.findById(data.students, activeId);
        if(!st) return;
        st.chat = st.chat || [];
        st.chat.push({ by:"student", text, at: new Date().toISOString() });
      });
      qs("#chatInput").value="";
      renderChat();
    });

    renderSelect();
    renderTable();