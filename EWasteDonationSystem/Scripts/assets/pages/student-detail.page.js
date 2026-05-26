/* Documented for easier understanding. */
const id = getParam("id");
    const s = Store.get();
    const st = Store.findById(s.students, id);

    function render(){
      if(!st){
        qs("#profileBox").innerHTML = "<div class='small'>Student not found.</div>";
        return;
      }
      qs("#metaLine").innerHTML = `ID: <b>${st.id}</b> · Created: ${Store.fmtDate(st.createdAt)} · ${Store.statusBadge(st.status)}`;

      qs("#profileBox").innerHTML = `
        <div class="stack">
          <div><span class="small">Full Name</span><div style="font-weight:800; font-size:18px">${st.fullName}</div></div>
          <div class="split">
            <div><span class="small">Phone</span><div style="font-weight:650">${st.phone || "-"}</div></div>
            <div><span class="small">Institute</span><div style="font-weight:650">${st.institute || "-"}</div></div>
          </div>
          <div class="split">
            <div><span class="small">City</span><div style="font-weight:650">${st.city || "-"}</div></div>
            <div><span class="small">Address</span><div style="font-weight:650">${st.address || "-"}</div></div>
          </div>
        </div>
      `;

      qs("#appBox").innerHTML = `
        <div class="stack">
          <div>
            <div class="small">Items Needed</div>
            <div style="font-weight:650; margin-top:6px; white-space:pre-wrap">${st.needItems || "-"}</div>
          </div>

          <div class="hr"></div>
          <div>
            <div class="small">Location</div>
            <div style="font-weight:650; margin-top:6px; white-space:pre-wrap">${formatLocation(st.location)}</div>
          </div>

          <div class="hr"></div>
          <div>
            <div class="small">Reason</div>
            <div style="font-weight:650; margin-top:6px; white-space:pre-wrap">${st.reason || "-"}</div>
          </div>
        </div>
      `;

      renderChat();
    }

    function renderChat(){
      const s2 = Store.get();
      const st2 = Store.findById(s2.students, id);
      const box = qs("#chatMsgs");
      box.innerHTML = "";
      (st2?.chat || []).forEach(m=>{
        const div = document.createElement("div");
        div.className = "bubble " + (m.by === "student" ? "me" : "them");
        div.innerHTML = `<div>${m.text}</div><div class="small" style="margin-top:6px">${Store.fmtDate(m.at)}</div>`;
        box.appendChild(div);
      });
      box.scrollTop = box.scrollHeight;
    }

    qs("#sendBtn").addEventListener("click", ()=>{
      const text = qs("#chatInput").value.trim();
      if(!text) return;
      Store.update(data=>{
        const ss = Store.findById(data.students, id);
        if(!ss) return;
        ss.chat = ss.chat || [];
        ss.chat.push({ by:"student", text, at: new Date().toISOString() });
      });
      qs("#chatInput").value="";
      renderChat();
    });

    render();