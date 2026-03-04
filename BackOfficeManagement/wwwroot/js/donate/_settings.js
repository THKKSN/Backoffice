// _settings.js

// สร้างฟังก์ชันสำหรับ Init Select2 โดยเฉพาะ เพื่อเรียกใช้ซ้ำได้ง่าย
function initSelect2DonateSettings() {
  // หา Offcanvas ที่เพิ่งถูกโหลดเข้ามา
  var $offcanvas = $("#CreateDonateSettingsOffcanvas");

  // ถ้าไม่เจอ (เช่น กรณี Update อาจจะเป็น ID อื่น) ให้ลองหา class ทั่วไป
  if ($offcanvas.length === 0) {
    $offcanvas = $(".offcanvas");
  }

  $offcanvas.find("select.select2").each(function () {
    // destroy ตัวเก่าถ้ามี เพื่อป้องกัน error
    if ($(this).data("select2")) {
      $(this).select2("destroy");
    }

    // init ใหม่ พร้อมกำหนด dropdownParent เพื่อแก้บั๊กพิมพ์ค้นหาไม่ได้ใน Modal/Offcanvas
    $(this).select2({
      dropdownParent: $offcanvas,
      width: "100%", // แนะนำให้ใส่เพื่อให้ขนาดพอดี
    });
  });
}

function CreateDonateSettings() {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "Create";

  // Clear ค่าเก่า
  $("#ViewDonateSettings").html(null);
  $("#CreateDonateSettings").html(null);
  $("#UpdateDonateSettings").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#CreateDonateSettings").html(data);

      // --- จุดสำคัญ: เรียก Init Select2 หลังจากยัด HTML ลงไปแล้ว ---
      initSelect2DonateSettings();
      // --------------------------------------------------------

      $("#CreateDonateSettings").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function UpdateDonateSettings(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "Update?id=" + id;

  $("#ViewDonateSettings").html(null);
  $("#CreateDonateSettings").html(null);
  $("#UpdateDonateSettings").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#UpdateDonateSettings").html(data);

      // --- จุดสำคัญ: เรียก Init Select2 ตรงนี้ด้วย ---
      initSelect2DonateSettings();
      // ------------------------------------------------

      $("#UpdateDonateSettings").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function CreateDonateIndividual() {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "CreateIndividual";

  // Clear ค่าเก่า
  $("#ViewDonateSettings").html(null);
  $("#CreateDonateSettings").html(null);
  $("#UpdateDonateSettings").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#CreateDonateIndividual").html(data);

      // --- จุดสำคัญ: เรียก Init Select2 หลังจากยัด HTML ลงไปแล้ว ---
      initSelect2DonateSettings();
      // --------------------------------------------------------

      $("#CreateDonateIndividual").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function UpdateDonateSettingInv(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "UpdateIndividual?id=" + id;

  $("#ViewDonateSettings").html(null);
  $("#CreateDonateSettings").html(null);
  $("#UpdateDonateSettings").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#UpdateDonateSettingIndividual").html(data);

      // --- จุดสำคัญ: เรียก Init Select2 ตรงนี้ด้วย ---
      initSelect2DonateSettings();
      // ------------------------------------------------

      $("#UpdateDonateSettingIndividual").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}