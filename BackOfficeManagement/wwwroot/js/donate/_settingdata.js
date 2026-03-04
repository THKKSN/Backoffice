$(document).ready(function () {
  // init select2 ครั้งเดียวพอ
  $("#schoolYearDefault, #SchoolYearIndividual").select2({
    width: "100%",
  });

  // โหลด Default ตอนเปิดหน้า
  loadDonateSettingsTable();

  // โหลด Individual ตอนเปิด tab
  $('button[data-bs-toggle="tab"]').on("shown.bs.tab", function (e) {
    const target = $(e.target).attr("data-bs-target");

    if (target === "#product-tab-pane") {
      if (!$.fn.DataTable.isDataTable("#tblIndividual")) {
        loadDonateSettingIndividualTable();
      }
    }
  });
});

function loadDonateSettingsTable() {
  if (!$("#tblDonateSettings").length) return;

  if ($.fn.DataTable.isDataTable("#tblDonateSettings")) {
    $("#tblDonateSettings").DataTable().clear().destroy();
  }

  const table = $("#tblDonateSettings").DataTable({
    ajax: {
      url: DF_URLS.DEFAULT_DATA,
      type: "POST",
      contentType: "application/json",
      data: function () {
        return JSON.stringify({
          SchoolYear: $("#schoolYearDefault").val() || "",
        });
      },
      dataSrc: function (json) {
        return json?.data ?? [];
      },
    },
    columns: [
      { data: "schoolYear", className: "text-center" },
      { data: "term", className: "text-center" },
      { data: "gradeLevel", className: "text-center" },
      {
        data: "donate",
        className: "text-end",
        render: function (d) {
          if (!d) return "0.00";
          return Number(d).toLocaleString("en-US", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
      {
        data: "id",
        className: "text-center",
        render: function (id) {
          return `
                            <button class="btn btn-default"
                                    onclick="UpdateDonateSettings('${id}')">
                                <i class="bi bi-pencil-square"></i>
                            </button>
                        `;
        },
      },
    ],
    paging: true,
    searching: true,
    ordering: false,
    responsive: true,
    pageLength: 10,
    deferRender: true,
  });

  $("#schoolYearDefault").on("change", function () {
    table.ajax.reload();
  });
}

function loadDonateSettingIndividualTable() {
  if (!$("#tblIndividual").length) return;

  if ($.fn.DataTable.isDataTable("#tblIndividual")) {
    $("#tblIndividual").DataTable().clear().destroy();
  }

  const table = $("#tblIndividual").DataTable({
    ajax: {
      url: DF_URLS.INDIVIDUAL_DATA,
      type: "POST",
      contentType: "application/json",
      data: function () {
        return JSON.stringify({
          SchoolYear: $("#SchoolYearIndividual").val() || "",
        });
      },
      dataSrc: function (json) {
        return json?.data ?? [];
      },
    },
    columns: [
      { data: "studentName" },
      { data: "schoolYear", className: "text-center" },
      { data: "term", className: "text-center" },
      { data: "gradeLevel", className: "text-center" },
      {
        data: "overrideAmount",
        className: "text-end",
        render: function (d) {
          if (!d) return "0.00";
          return Number(d).toLocaleString("en-US", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
      { data: "remark", className: "text-center" },
      {
        data: "id",
        className: "text-center",
        render: function (id) {
          return `
                                <button onclick="UpdateDonateSettingInv('${id}')" class="btn btn-default"><i class="bi bi-pencil-square"></i></button>
                                <button onclick="DeleteDonateSettingInv(${id})" class="btn btn-danger"><i class="bi bi-trash"></i></button>`;
        },
      },
    ],
    paging: true,
    searching: true,
    ordering: false,
    responsive: true,
    pageLength: 10,
    deferRender: true,
  });

  $("#SchoolYearIndividual").on("change", function () {
    table.ajax.reload();
  });
}

function DeleteDonateSettingInv(id) {
  var baseUrl = $("#urlDefualt").data("request-url");
  var url = baseUrl + "/DeleteIndividual";

  Swal.fire({
    title: "ยืนยันการลบ?",
    text: "ข้อมูลนี้จะถูกปิดการใช้งาน",
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    cancelButtonColor: "#6c757d",
    confirmButtonText: "ใช่, ลบเลย",
    cancelButtonText: "ยกเลิก",
    reverseButtons: true,
    showLoaderOnConfirm: true,
    preConfirm: () => {
      return $.ajax({
        url: url,
        type: "POST",
        data: { id: id },
      }).catch(() => {
        Swal.fire({
          title: "ล้มเหลว",
          text: "ไม่สามารถลบข้อมูลรายการนี้ได้",
          icon: "error",
        });
      });
    },
    allowOutsideClick: () => !Swal.isLoading(),
  }).then((result) => {
    if (result.isConfirmed) {
      Swal.fire({
        title: "ลบสำเร็จ",
        icon: "success",
        timer: 1200,
        showConfirmButton: false,
      });

      setTimeout(() => {
        location.reload();
      }, 1200);
    }
  });
}
