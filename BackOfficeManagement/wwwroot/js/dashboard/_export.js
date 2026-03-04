function confirmExport(onConfirm) {
  Swal.fire({
    icon: "question",
    title: "ยืนยันการนำข้อมูลออก",
    text: "คุณต้องการนำข้อมูลออกเป็น Excel ใช่หรือไม่",
    showCancelButton: true,
    confirmButtonText: "ยืนยัน",
    cancelButtonText: "ยกเลิก",
    reverseButtons: true,
  }).then((result) => {
    if (result.isConfirmed) {
      onConfirm();
    }
  });
}

function exportExcel({ url, payload, defaultFileName }, $btn) {
  Swal.fire({
    title: "กำลังดำเนินการ...",
    allowOutsideClick: false,
    didOpen: () => Swal.showLoading(),
  });

  $.ajax({
    url: url,
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),
    xhrFields: { responseType: "blob" },
    success: function (blob, status, xhr) {
      let fileName = defaultFileName || "report.xlsx";
      const disposition = xhr.getResponseHeader("Content-Disposition");
      if (disposition && disposition.includes("filename=")) {
        fileName = disposition.split("filename=")[1].replace(/"/g, "");
      }

      const link = document.createElement("a");
      link.href = URL.createObjectURL(blob);
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      link.remove();

      Swal.fire({
        icon: "success",
        title: "นำออกข้อมูลสำเร็จ",
        text: "ดาวน์โหลดไฟล์เรียบร้อยแล้ว",
      });
    },
    error: function () {
      Swal.fire({
        icon: "error",
        title: "เกิดข้อผิดพลาด",
        text: "ไม่สามารถนำออกข้อมูลได้",
      });
    },
    complete: function () {
      $btn
        .prop("disabled", false)
        .html('<i class="bi bi-box-arrow-right"></i> Export Excel');
    },
  });
}

$(document).on("click", "#exportToGoogleSheetBtn", function () {
  const schoolYearId = $("#schoolYearSelect").val();
  if (!schoolYearId) {
    Swal.fire({
      icon: "warning",
      title: "ยังไม่ได้เลือกปีการศึกษา",
      text: "กรุณาเลือกปีการศึกษาก่อน Export",
      confirmButtonText: "รับทราบ",
    });
    return;
  }

  const $btn = $(this).prop("disabled", true);

  const payload = {
    schoolYearIds: [parseInt(schoolYearId)],
    gradeLevelId: $("#topNotDonateSelect").val() || null,
  };

  // 🔄 Loading popup
  Swal.fire({
    title: "กำลังอัปโหลดข้อมูล",
    text: "ระบบกำลังส่งข้อมูลไป Google Sheet",
    allowOutsideClick: false,
    didOpen: () => {
      Swal.showLoading();
    },
  });

  $.ajax({
    url: DT_URLS.EXPORT.TOP_NOT_DONATE,
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),

    success: function (res) {
      Swal.close();

      if (res.success) {
        Swal.fire({
          icon: "success",
          title: "Export สำเร็จ",
          text: "ข้อมูลถูกส่งไปยัง Google Sheet เรียบร้อยแล้ว",
          confirmButtonText: "Close",
        });
      } else {
        Swal.fire({
          icon: "info",
          title: "ไม่พบข้อมูล",
          text: res.message || "ไม่มีข้อมูลสำหรับเงื่อนไขที่เลือก",
          confirmButtonText: "Close",
        });
      }
    },

    error: function () {
      Swal.close();
      Swal.fire({
        icon: "error",
        title: "เกิดข้อผิดพลาด",
        text: "ไม่สามารถอัปโหลดข้อมูลได้ กรุณาลองใหม่อีกครั้ง",
        confirmButtonText: "Close",
      });
    },

    complete: function () {
      $btn
        .prop("disabled", false)
        .html('<i class="bi bi-cloud-upload"></i> Export Google Sheet');
    },
  });
});

$(document).on("click", "#exportPiadToGoogleSheetBtn", function () {
  const schoolYearId = $("#schoolYearSelect").val();
  if (!schoolYearId) {
    Swal.fire({
      icon: "warning",
      title: "ยังไม่ได้เลือกปีการศึกษา",
      text: "กรุณาเลือกปีการศึกษาก่อน Export",
      confirmButtonText: "รับทราบ",
    });
    return;
  }

  const $btn = $(this).prop("disabled", true);

  const payload = {
    schoolYearIds: [parseInt(schoolYearId)],
    gradeLevelId: $("#topDonateSelect").val() || null,
  };

  // 🔄 Loading popup
  Swal.fire({
    title: "กำลังอัปโหลดข้อมูล",
    text: "ระบบกำลังส่งข้อมูลไป Google Sheet",
    allowOutsideClick: false,
    didOpen: () => {
      Swal.showLoading();
    },
  });

  $.ajax({
    url: DT_URLS.EXPORT.TOP_DONATE,
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),

    success: function (res) {
      Swal.close();

      if (res.success) {
        Swal.fire({
          icon: "success",
          title: "Export สำเร็จ",
          text: "ข้อมูลถูกส่งไปยัง Google Sheet เรียบร้อยแล้ว",
          confirmButtonText: "Close",
        });
      } else {
        Swal.fire({
          icon: "info",
          title: "ไม่พบข้อมูล",
          text: res.message || "ไม่มีข้อมูลสำหรับเงื่อนไขที่เลือก",
          confirmButtonText: "Close",
        });
      }
    },

    error: function () {
      Swal.close();
      Swal.fire({
        icon: "error",
        title: "เกิดข้อผิดพลาด",
        text: "ไม่สามารถอัปโหลดข้อมูลได้ กรุณาลองใหม่อีกครั้ง",
        confirmButtonText: "Close",
      });
    },

    complete: function () {
      $btn
        .prop("disabled", false)
        .html('<i class="bi bi-cloud-upload"></i> Export Google Sheet');
    },
  });
});

$(document).on("click", "#exportSheetBtn", function () {
  const schoolYearId = $("#schoolYearSelect").val();
  if (!schoolYearId) {
    Swal.fire({
      icon: "warning",
      title: "ยังไม่ได้เลือกปีการศึกษา",
      text: "กรุณาเลือกปีการศึกษาก่อน Export",
      confirmButtonText: "รับทราบ",
    });
    return;
  }

  const $btn = $(this).prop("disabled", true);

  const payload = {
    schoolYearIds: [parseInt(schoolYearId)],
    gradeLevelId: null,
  };

  // 🔄 Loading popup
  Swal.fire({
    title: "กำลังอัปโหลดข้อมูล",
    text: "ระบบกำลังส่งข้อมูลไป Google Sheet",
    allowOutsideClick: false,
    didOpen: () => {
      Swal.showLoading();
    },
  });

  $.ajax({
    url: DT_URLS.EXPORT.SHEET,
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),

    success: function (res) {
      Swal.close();

      if (res.success) {
        Swal.fire({
          icon: "success",
          title: "Export สำเร็จ",
          text: "ข้อมูลถูกส่งไปยัง Google Sheet เรียบร้อยแล้ว",
          confirmButtonText: "Close",
        });
      } else {
        Swal.fire({
          icon: "info",
          title: "ไม่พบข้อมูล",
          text: res.message || "ไม่มีข้อมูลสำหรับเงื่อนไขที่เลือก",
          confirmButtonText: "Close",
        });
      }
    },

    error: function () {
      Swal.close();
      Swal.fire({
        icon: "error",
        title: "เกิดข้อผิดพลาด",
        text: "ไม่สามารถอัปโหลดข้อมูลได้ กรุณาลองใหม่อีกครั้ง",
        confirmButtonText: "Close",
      });
    },

    complete: function () {
      $btn
        .prop("disabled", false)
        .html('<i class="bi bi-cloud-upload"></i> Export Google Sheet');
    },
  });
});

$(document).on("click", "#exportExcelBtn", function () {
  const schoolYearId = $("#schoolYearSelect").val();
  if (!schoolYearId) {
    Swal.fire({
      icon: "warning",
      title: "ยังไม่ได้เลือกปีการศึกษา",
      text: "กรุณาเลือกปีการศึกษาก่อน Export",
    });
    return;
  }

  const $btn = $(this).prop("disabled", true);

  confirmExport(() => {
    exportExcel(
      {
        url: DT_URLS.EXPORT.EXCEL,
        payload: {
          schoolYearIds: [parseInt(schoolYearId)],
          gradeLevelId: null,
        },
        defaultFileName: "Donation_Report.xlsx",
      },
      $btn,
    );
  });
});
