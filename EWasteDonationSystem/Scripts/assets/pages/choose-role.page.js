/* Documented for easier understanding. */
const s = Store.get();
    const pending = s.donors.filter(d=>d.status==="pending").length + s.students.filter(x=>x.status==="pending").length;
    document.getElementById("kpiDonors").textContent = s.donors.length;
    document.getElementById("kpiStudents").textContent = s.students.length;
    document.getElementById("kpiPending").textContent = pending;

(function(){
      const ROLE_KEY = 'ewaste_selected_role_v1';
      const roleBtns = Array.from(document.querySelectorAll('.roleBtn'));
      const loginBtn = document.getElementById('loginBtn');
      const openBtn = document.getElementById('openSignupBtn');
      const closeBtn = document.getElementById('closeSignupBtn');
      const card = document.getElementById('signupCard');

      const setSelected = (role) => {
        try{ localStorage.setItem(ROLE_KEY, role || ''); }catch(e){}
        roleBtns.forEach(b => {
          const isSel = (b.getAttribute('data-role') === role);
          b.classList.toggle('selected', isSel);
          b.setAttribute('aria-pressed', isSel ? 'true' : 'false');
        });
      };

      const getSelected = () => {
        try{ return localStorage.getItem(ROLE_KEY) || ''; }catch(e){ return ''; }
      };

      // Restore selected role on refresh
      setSelected(getSelected());

      roleBtns.forEach(btn => {
        btn.addEventListener('click', () => {
          setSelected(btn.getAttribute('data-role'));
        });
      });

      if(loginBtn){
        loginBtn.addEventListener('click', () => {
          const role = getSelected();
          if(!role){ toast('Please select role first'); return; }
          toast('Demo Login Successful');
          if(role === 'donor') location.href = 'pages/donor.html';
          else if(role === 'admin') location.href = 'pages/admin.html';
          else if(role === 'student') location.href = 'pages/student.html';
          else toast('Invalid role');
        });
      }

      if(openBtn && closeBtn && card){
        openBtn.addEventListener('click', ()=>{
          const role = getSelected();
          if(!role){ toast('Please select role first (Donor / Student)'); return; }
          if(role === 'admin'){ toast('Admin signup is disabled in demo UI'); return; }
          card.style.display='block';
          card.scrollIntoView({behavior:'smooth', block:'start'});
        });
        closeBtn.addEventListener('click', ()=>{ card.style.display='none'; });
      }

      const createBtn = document.getElementById('createAccountBtn');
      if(createBtn){
        createBtn.addEventListener('click', ()=>{
          const firstName = (document.getElementById('suFirstName')?.value || '').trim();
          const fullName = (document.getElementById('suName')?.value || '').trim();
          const email = (document.getElementById('suEmail')?.value || '').trim();
          const pass = (document.getElementById('suPassword')?.value || '');
          const cpass = (document.getElementById('suConfirmPassword')?.value || '');

          if(!firstName || !fullName || !email || !pass || !cpass){ toast('Please fill all fields'); return; }
          if(pass !== cpass){ toast('Password and Confirm Password must match'); return; }

          // UI-only demo: we do not create a real account here.
          toast('Account Created! Please login now.');
          if(card) card.style.display='none';
          window.scrollTo({ top: 0, behavior: 'smooth' });
        });
      }
    })();