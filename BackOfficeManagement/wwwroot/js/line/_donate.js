$(function () {
  $(".select2").each(function () {
    $(this).select2({
      dropdownParent: $(this).parent(),
      width: "100%",
    });
  });
});

/*--------------------------------------------------------------- Fetch Donate Balance --------------------------------------*/

document.addEventListener("DOMContentLoaded", () => {
  const config = document.getElementById("donateConfig");
  const url = config.dataset.url;
  const studentId = config.dataset.student;

  document
    .getElementById("School_Year_Id")
    .addEventListener("change", function () {
      const yearId = this.value;
      const balanceDiv = document.getElementById("donateBalance");

      if (!yearId) {
        balanceDiv.style.display = "none";
        return;
      }

      if (!studentId) {
        console.log("ยังไม่มีข้อมูลนักเรียน — ข้ามการ fetch ยอดคงเหลือ");
        balanceDiv.style.display = "none";
        return;
      }

      fetch(
        `${url}?schoolYearId=${yearId}&studentId=${studentId}&term=${
          document.getElementById("Term_Id").value
        }`
      )
        .then((res) => {
          if (!res.ok) throw new Error("Network Error");
          return res.json();
        })
        .then((data) => {
          document.getElementById("requiredAmount").innerText = (
            data.requiredAmount || 0
          ).toLocaleString();
          document.getElementById("donatedAmount").innerText = (
            data.donatedAmount || 0
          ).toLocaleString();
          document.getElementById("remainBalance").innerText = (
            data.remain || 0
          ).toLocaleString();

          balanceDiv.style.display = "block";
        })
        .catch((err) => console.error("Error fetching donate balance:", err));
    });
});

/*--------------------------------------------------------------- Manage Attachment --------------------------------------*/

function previewImage(event) {
  const file = event.target.files[0];
  const previewContainer = document.getElementById("image-preview");
  const base64Input = document.getElementById("ImageBase64");
  const extInput = document.getElementById("ImageExtension");

  // เคลียร์ preview ก่อน
  previewContainer.innerHTML = "";
  base64Input.value = "";
  extInput.value = "";

  if (!file) return;

  // เก็บนามสกุลไฟล์
  const extension = file.name.split(".").pop().toLowerCase();
  extInput.value = extension;

  const reader = new FileReader();

  reader.onload = function (e) {
    // preview image
    const img = document.createElement("img");
    img.src = e.target.result;
    img.style.maxWidth = "200px";
    img.style.borderRadius = "8px";
    img.style.boxShadow = "0 2px 6px rgba(0,0,0,0.15)";
    img.classList.add("img-thumbnail");

    previewContainer.appendChild(img);

    // เก็บ base64 (ตัด prefix ออก)
    const base64Data = e.target.result.split(",")[1];
    base64Input.value = base64Data;
  };

  reader.readAsDataURL(file);
}
document.getElementById("donateForm").addEventListener("submit", function () {
  const date = document.getElementById("DonateDate").value;
  const time = document.getElementById("DonateTime").value;

  document.getElementById("DonateDateTime").value = `${date}T${time}`;
});

let isSubmitting = false;

document.getElementById("donateForm")
  .addEventListener("submit", async function (e) {
    e.preventDefault();

    if (isSubmitting) return; // กันกดซ้ำ
    isSubmitting = true;

    const submitBtn = e.submitter;
    if (submitBtn) {
      submitBtn.disabled = true;
      submitBtn.innerHTML = `
        <span class="spinner-border spinner-border-sm me-1"></span>
        กำลังบันทึก...
      `;
    }

    document.getElementById("actionInput").value = submitBtn?.value || "donate";

    const form = document.getElementById("donateForm");
    const url = $("#urlDonate").data("request-url");
    const formData = new FormData(form);

    try {
      const res = await fetch(url, {
        method: "POST",
        body: formData,
        headers: { "X-Requested-With": "XMLHttpRequest" },
      });

      const data = await res.json();

      if (data.success) {
        Swal.fire({
          icon: "success",
          title: "สำเร็จ",
          text: data.message,
        }).then(() => {
          if (data.redirect) {
            window.location.href = data.redirect;
          }
        });

        form.reset();
        document.getElementById("image-preview").innerHTML = "";
      } else {
        Swal.fire({
          icon: "error",
          title: "ผิดพลาด",
          text: data.message || "กรุณากรอกข้อมูลให้ครบ",
        });

        resetSubmit(submitBtn);
      }
    } catch (err) {
      console.error(err);
      Swal.fire({
        icon: "error",
        title: "เกิดข้อผิดพลาด",
        text: "ลองใหม่อีกครั้ง",
      });

      resetSubmit(submitBtn);
    }
  });

function resetSubmit(btn) {
  if (!btn) return;
  btn.disabled = false;
  btn.innerHTML = "บันทึก";
  isSubmitting = false;
}
