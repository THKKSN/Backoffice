var $ = jQuery;

function initSelect2(context) {
  context = context || document;

  $(context)
    .find("select.select2")
    .each(function () {
      if ($(this).data("select2")) {
        $(this).select2("destroy");
      }

      $(this).select2({
        dropdownParent: $(this).parent(),
        width: "100%",
      });
    });
}


function CreateOverdueFrom() {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/Create";

  $("#ViewOverdueFrom").html(null);
  $("#CreateOverdueFrom").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#CreateOverdueFrom").html(data);

      var $offcanvas = $("#CreateOverdueFrom").find(".offcanvas");
      $offcanvas.offcanvas("show");

      initSelect2($offcanvas);

      $offcanvas
        .off("shown.bs.offcanvas.overdue")
        .on("shown.bs.offcanvas.overdue", function () {
          initOverdueTable();
        });
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

$(document).on("change", "#gradelevelSelect", function () {
  let gradeId = $(this).val();
  let url = $("#studentId").data("request-url-student");
  let $ddl = $("#studentId");

  resetStudent();
  resetSchoolYear();

  if (!gradeId) return;

  $.get(url, { gradeId }, function (data) {
    $ddl.append(
      data.map((i) => `<option value="${i.value}">${i.text}</option>`)
    );

    $ddl.trigger("change.select2");
  });
});

$(document).on("change", "#studentId", function () {
  let uuid = $(this).val();
  let url = $("#schoolYearSelect").data("request-url-schoolyear");
  let $ddl = $("#schoolYearSelect");

  resetSchoolYear();

  if (!uuid) return;

  $.get(url, { uuid }, function (data) {
    $ddl.append(
      data.map((i) => `<option value="${i.value}">${i.text}</option>`)
    );

    $ddl.trigger("change.select2");
  });
});

function resetStudent() {
  $("#studentId").empty().append(`<option value="">เลือก</option>`);
}

function resetSchoolYear() {
  $("#schoolYearSelect").empty().append(`<option value="">เลือก</option>`);
}

function resetStudentSelected() {
  $("#studentSelected").empty().append(`<option value="">เลือก</option>`);
}

function resetSchoolYearSelected() {
  $("#schoolYearSelected").empty().append(`<option value="">เลือก</option>`);
}

function loadOverdueTable() {
  let schoolYearId = $("#schoolYearSelect").val();
  let studentId = $("#studentId").val();

  if (!schoolYearId || !studentId) return;

  let url = $("#tableOverdue").data("request-url");

  $.get(url, {schoolYearId, studentId }, function (data) {
    let $tbody = $("#tableOverdue tbody");
    $tbody.empty();

    data.forEach((item) => {
      let outstanding = item.total - item.paid;

      $tbody.append(`
                <tr>
                    <td>${item.term}</td>
                    <td class="text-end text-success">
                        ${item.paid.toLocaleString(undefined, {
                          minimumFractionDigits: 2,
                        })}
                    </td>
                    <td class="text-end ${
                      outstanding > 0 ? "text-danger" : "text-muted"
                    }">
                        ${outstanding.toLocaleString(undefined, {
                          minimumFractionDigits: 2,
                        })}
                    </td>
                </tr>
            `);
    });
  });
}

let overdueTable;

function initOverdueTable() {
  const $table = $("#tableOverdue");
  const url = $table.data("request-url");

  if ($.fn.DataTable.isDataTable($table)) {
    overdueTable.ajax.reload();
    return;
  }

  overdueTable = $table.DataTable({
    processing: true,
    paging: false,
    searching: false,
    info: false,
    ordering: false,
    ajax: {
      url: url,
      type: "GET",
      cache: true,
      data: function (d) {
        const studentId = $("#studentId").val();
        const schoolYearId = $("#schoolYearSelect").val();

        d.studentId = studentId;
        d.schoolYearId = schoolYearId;
      },
      dataSrc: function (json) {
        if (
          !$("#studentId").val() ||
          !$("#schoolYearSelect").val()
        ) {
          return [];
        }

        return json;
      },
    },
    columns: [
      { data: "term" },
      {
        data: "paid",
        className: "text-end text-success",
        render: function (data) {
          return data.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
      {
        data: "balance",
        className: "text-end text-danger",
        render: function (data) {
          return data.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
    ],
  });

  $("#gradelevelSelect, #studentId, #schoolYearSelect")
    .off("change.overdue")
    .on("change.overdue", function () {
      if (!overdueTable) return;

      overdueTable.ajax.reload();
    });
}
$(document).on("click", ".btnDelete", function () {
  const id = $(this).data("id");
  let url = $("#urlDefualt").data("request-url");

  Swal.fire({
    title: "ยืนยันการลบ?",
    text: "ต้องการลบข้อมูลการติดตามการค้างบริจาคนี้ใช่หรือไม่",
    icon: "warning",
    showCancelButton: true,
    confirmButtonText: "ลบ",
    cancelButtonText: "ยกเลิก",
    reverseButtons: true,
  }).then((result) => {
    if (result.isConfirmed) {
      $.ajax({
        url: url + "/Delete",
        type: "POST",
        data: { id },
        success: function () {
          Swal.fire({
            icon: "success",
            title: "ดำเนินการลบข้อมูลสำเร็จ",
            timer: 1200,
            showConfirmButton: true,
            confirmButtonText: "ยืนยัน",
          }).then(() => {
            window.location.href = url;
          });
        },
        error: function () {
          Swal.fire("เกิดข้อผิดพลาด", "ไม่สามารถลบข้อมูลได้", "error");
        },
      });
    }
  });
});

function updateOverdueTable() {
  const $table = $("#tblOverdue");
  const url = $table.data("request-url");

  if ($.fn.DataTable.isDataTable($table)) {
    overdueTable.ajax.reload();
    return;
  }

  overdueTable = $table.DataTable({
    processing: true,
    paging: false,
    searching: false,
    info: false,
    ordering: false,
    ajax: {
      url: url,
      type: "GET",
      cache: true,
      data: function (d) {
        const gradeId = $("#gradelevelSelected").val();
        const studentId = $("#studentSelected").val();
        const schoolYearId = $("#schoolYearSelected").val();

        d.gradeId = gradeId;
        d.studentId = studentId;
        d.schoolYearId = schoolYearId;
      },
      dataSrc: function (json) {
        if (
          !$("#gradelevelSelected").val() ||
          !$("#studentSelected").val() ||
          !$("#schoolYearSelected").val()
        ) {
          return [];
        }

        return json;
      },
    },
    columns: [
      { data: "term" },
      {
        data: "paid",
        className: "text-end text-success",
        render: function (data) {
          return data.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
      {
        data: null,
        className: "text-end text-danger",
        render: function (data) {
          const outstanding = data.total - data.paid;
          return outstanding.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          });
        },
      },
    ],
  });

  // reload เมื่อเปลี่ยนค่า
  $("#gradelevelSelected, #studentSelected, #schoolYearSelected")
    .off("change.overdue")
    .on("change.overdue", function () {
      if (!overdueTable) return;
      overdueTable.ajax.reload();
    });
}

function UpdateOverdueFrom(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/Update?id=" + id;

  $("#ViewOverdueFrom").html(null);
  $("#CreateOverdueFrom").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#UpdateOverdueFrom").html(data);

      var $offcanvas = $("#UpdateOverdueFrom").find(".offcanvas");
      $offcanvas.offcanvas("show");

      initSelect2($offcanvas);

     $offcanvas
  .off("shown.bs.offcanvas.overdue")
  .on("shown.bs.offcanvas.overdue", function () {
    $("#gradelevelSelected").trigger("change");
  });

    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function loadUpdateOverdueTable() {
  let schoolYearId = $("#schoolYearSelected").val();
  let studentId = $("#studentSelected").val();

  if (!schoolYearId || !studentId) return;

  let url = $("#urlGetOverdueDatatable").val();

  $.get(url, {schoolYearId, studentId }, function (data) {
    let $tbody = $("#tblOverdue tbody");
    $tbody.empty();

    data.forEach((item) => {

      $tbody.append(`
                <tr>
                    <td>${item.term}</td>
                    <td class="text-end text-success">
                        ${item.paid.toLocaleString(undefined, {
                          minimumFractionDigits: 2,
                        })}
                    </td>
                    <td class="text-end text-danger">
                        ${item.balance.toLocaleString(undefined, {
                          minimumFractionDigits: 2,
                        })}
                    </td>
                </tr>
            `);
    });
  });
}

$(document).on("change", "#gradelevelSelected", function () {
  let gradeId = $(this).val();
  let url = $("#urlGetStudentsByGrade").val();
  let $ddl = $("#studentSelected");
  let initStudentId = $("#initStudentId").val();

  resetStudentSelected();
  resetSchoolYearSelected();

  if (!gradeId) return;

  $.get(url, { gradeId }, function (data) {
    data.forEach((i) => {
      $ddl.append(`<option value="${i.value}">${i.text}</option>`);
    });

    if (initStudentId) {
      $ddl.val(initStudentId).trigger("change");
    }
  });
});

$(document).on("change", "#studentSelected", function () {
  let uuid = $(this).val();
  let url = $("#urlGetSchoolYearByUUID").val();
  let $ddl = $("#schoolYearSelected");
  let initSchoolYearId = $("#initSchoolYearId").val();

  resetSchoolYearSelected();

  if (!uuid) return;

  $.get(url, { uuid }, function (data) {
    data.forEach((i) => {
      $ddl.append(`<option value="${i.value}">${i.text}</option>`);
    });

    if (initSchoolYearId) {
      $ddl.val(initSchoolYearId).trigger("change.select2");
    }

    loadUpdateOverdueTable();
  });
});

$(document).on("submit", "#OverdueUpdateFrom", function (e) {
  e.preventDefault(); 

  const form = this;

  Swal.fire({
    title: "ยืนยันการแก้ไข?",
    text: "ต้องการแก้ไขข้อมูลการติดตามค้างบริจาคนี้ใช่หรือไม่",
    icon: "warning",
    showCancelButton: true,
    confirmButtonText: "ยืนยัน",
    cancelButtonText: "ยกเลิก",
    reverseButtons: true,
  }).then((result) => {
    if (result.isConfirmed) {
      form.submit();
    }
  });
});
$(document).on(
  "change",
  " #studentSelected, #schoolYearSelected",
  function () {
    loadUpdateOverdueTable();
  }
);


function ViewOverdueFrom(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/Detail?id=" + id;

  $("#ViewOverdueFrom").html(null);
  $("#CreateOverdueFrom").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#ViewOverdueFrom").html(data);

      var $offcanvas = $("#ViewOverdueFrom").find(".offcanvas");
      $offcanvas.offcanvas("show");

    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}