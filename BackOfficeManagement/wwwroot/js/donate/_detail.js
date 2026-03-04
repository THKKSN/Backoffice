function Outstandings(Id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/OutstandingsByUuid";

  const $container = $("#Outstandings");
  $container.html("");

  $.ajax({
    type: "GET",
    url: urlGet,
    data: { uuid: Id },
    success: function (data) {
      $container.html(data);

      const offcanvasEl = $container.find(".offcanvas");

      // กัน bind ซ้ำ
      offcanvasEl.off("shown.bs.offcanvas");

      offcanvasEl.on("shown.bs.offcanvas", function () {
        initOutstandingDataTable(this);
      });

      offcanvasEl.offcanvas("show");
    },
    error: function (response) {
      console.error(response.responseText);
    },
  });
}

function DonateDetail(Id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "DonateDetail";

  $("#DonateDetail").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    data: {
      id: Id,
    },
    success: function (data) {
      $("#DonateDetail").html(data);
      $("#DonateDetail").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function initOutstandingDataTable(context) {
  const $table = context
    ? $(context).find("#tblOutstanding")
    : $("#tblOutstanding");

  if (!$table.length) return;
  if ($.fn.DataTable.isDataTable($table)) return;

  $table.DataTable({
    scrollX: true,
    scrollCollapse: true,
    autoWidth: false,
    responsive: false,
    lengthChange: true,
    searching: false,
    ordering: true,
    info: false,
    paging: false,

    language: {
      lengthMenu: "แสดง _MENU_ รายการ",
      zeroRecords: "ไม่พบข้อมูล",
      paginate: {
        first: "แรก",
        last: "สุดท้าย",
        next: "ถัดไป",
        previous: "ก่อนหน้า",
      },
    },
    order: [[0, "desc"]],

    columnDefs: [
      { targets: [1, 2, 3], className: "text-end", width: "120px" },
      { targets: [0], className: "text-center", width: "160px" },
    ],
  });
}