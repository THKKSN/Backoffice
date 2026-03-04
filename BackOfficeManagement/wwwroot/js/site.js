// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(function () {
  const body = document.body;
  const toggleBtn = document.getElementById("toggleDarkMode");

  if (!toggleBtn) return;

  const offcanvasIds = ["DonateDetail"]; // ใส่ id offcanvas ได้หลายตัว

  function syncOffcanvasTheme() {
    const isDark = body.classList.contains("dark-mode");

    offcanvasIds.forEach((id) => {
      const el = document.getElementById(id);
      if (!el) return;

      el.classList.toggle("offcanvas-dark", isDark);
      el.classList.toggle("offcanvas-light", !isDark);
    });
  }

  // โหลดค่าที่เคยเลือก
  const darkMode = localStorage.getItem("darkMode");
  if (darkMode === "on") {
    body.classList.add("dark-mode");
  }

  // sync ตอนโหลด
  syncOffcanvasTheme();

  toggleBtn.addEventListener("click", function (e) {
    e.preventDefault();

    body.classList.toggle("dark-mode");

    localStorage.setItem(
      "darkMode",
      body.classList.contains("dark-mode") ? "on" : "off",
    );

    syncOffcanvasTheme();

    // 🔥 ยิง event บอกทั้งระบบว่า theme เปลี่ยนแล้ว
    document.dispatchEvent(new Event("themeChanged"));
  });
})();
