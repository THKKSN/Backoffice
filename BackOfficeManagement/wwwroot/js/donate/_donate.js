var $ = jQuery;

//#region "Option Select2"
$(function () {
  $(".select2").each(function () {
    $(this).select2({
      dropdownParent: $(this).parent(),
      width: "100%",
    });
  });
});
//#endregion

function CreateDonateFrom() {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "DonateFrom";

  $("#ViewDonateFrom").html(null);
  $("#CreateDonateFrom").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#CreateDonateFrom").html(data);
      $("#CreateDonateFrom").find(".offcanvas").offcanvas("show");
      initSelect2();
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}


// ===================== Reset Helpers =====================
function resetStudent() {
  $("#Student_Id").empty().append(`<option value="">เลือก</option>`);
}

function resetYear() {
  $("#SchoolYear_Id").empty().append(`<option value="">เลือก</option>`);
}

function resetTerm() {
  $("#TermContainer").empty();
}

function hideDonate() {
  $("#DonateContainer").hide();
  $("#DonateContainer input").val("");
  $("#image-preview").empty();
}

// ===================== Grade Change =====================
$(document).on("change", "#GradeLevel_Id", function () {
  let gradeId = $(this).val();
  let url = $("#Student_Id").data("request-url-student");

  resetStudent();
  resetYear();
  resetTerm();
  hideDonate();

  if (!gradeId) return;

  $.get(url, { gradeId: gradeId }, function (data) {
    let $ddl = $("#Student_Id");
    $ddl.append(
      data.map((i) => `<option value="${i.value}">${i.text}</option>`)
    );
    $ddl.trigger("change.select2");
  });
});

// ===================== Student Change =====================
$(document).on("change", "#Student_Id", function () {
  let studentId = $(this).val();
  let gradeId = $("#GradeLevel_Id").val();
  let url = $("#SchoolYear_Id").data("request-url-schoolyear");

  resetYear();
  resetTerm();
  hideDonate();

  if (!studentId || !gradeId) return;

  $.get(url, { gradeId, studentId }, function (data) {
    let $ddl = $("#SchoolYear_Id");
    $ddl.append(
      data.map((i) => `<option value="${i.value}">${i.text}</option>`)
    );
    $ddl.trigger("change.select2");
  });
});

// ===================== SchoolYear Change =====================
$(document).on("change", "#SchoolYear_Id", function () {
  let yearId = $(this).val();
  let studentId = $("#Student_Id").val();
  let gradeId = $("#GradeLevel_Id").val();
  let urlBase = $("#urlDefualt").data("request-url");
  let urlGet = `${urlBase}/GetTermCards`;

  resetTerm();
  hideDonate();

  if (!yearId || !studentId || !gradeId) return;

  $.get(urlGet, { gradeId, schoolYearId: yearId, studentId }, function (html) {
    $("#TermContainer").html(html);
  });
});

// ===================== Term Card Click =====================
$(document).on("change", "input[name='Term_Id']", function () {
  // เมื่อเลือก term แล้วแสดง DonateContainer
  $("#DonateContainer").slideDown(150);
});

// ===================== Init Select2 =====================
function initSelect2() {
  $("#CreatDonateFrom ")
    .find("select.select2")
    .each(function () {
      // ถ้า select2 ถูก init อยู่แล้ว ให้ destroy ก่อน
      if ($(this).data("select2")) {
        $(this).select2("destroy");
      }
      // init ใหม่
      $(this).select2({
        dropdownParent: $(this).parent(),
      });
    });
}

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


let isSubmitting = false;

document.addEventListener("submit", async function (e) {
  const form = e.target;
  if (!form || form.id !== "DonateFrom") return;

  e.preventDefault();

  if (isSubmitting) return; // กันกดซ้ำ
  isSubmitting = true;

  const btn = document.getElementById("btnDonateSubmit");
  btn.disabled = true;
  btn.innerHTML = `
    <span class="spinner-border spinner-border-sm me-1"></span>
    กำลังบันทึก...
  `;

  const url = document.getElementById("urlDonate")?.dataset.requestUrl;
  if (!url) {
    console.error("URL Donate ไม่พบ");
    resetButton(btn);
    isSubmitting = false;
    return;
  }

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
        if (data.redirect) window.location.href = data.redirect;
      });

      form.reset();
      document.getElementById("image-preview").innerHTML = "";
    } else {
      Swal.fire({
        icon: "error",
        title: "ผิดพลาด",
        text: data.message || "กรุณากรอกข้อมูลให้ครบ",
      });

      resetButton(btn);
      isSubmitting = false;
    }
  } catch (err) {
    console.error(err);
    Swal.fire({
      icon: "error",
      title: "เกิดข้อผิดพลาด",
      text: "ลองใหม่อีกครั้ง",
    });

    resetButton(btn);
    isSubmitting = false;
  }
});

function resetButton(btn) {
  btn.disabled = false;
  btn.innerHTML = "บันทึก";
}
