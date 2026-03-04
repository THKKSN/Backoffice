function initTopDonateTable() {
  if ($.fn.DataTable.isDataTable("#topDonateTable")) {
    $("#topDonateTable").DataTable().clear().draw().destroy();
  }

  $("#topDonateTable").DataTable({
    ajax: {
      url: DT_URLS.TOP_DONATE,
      type: "POST",
      contentType: "application/json",
      data: function (d) {
        const queryData = {
          Draw: 1,
          Start: 0,
          Length: 0,
          Name: "",
          Search: "",
          SearchDate: "",
          StartDate: "",
          EndDate: "",
          SortColumn: "",
          SortOrder: "",
          FilterBy: "",
          schoolYearId: $("#schoolYearSelect").val() || null,
          gradeLevelId: $("#topDonateSelect").val() || null,
        };
        return JSON.stringify(queryData); // ส่งข้อมูลในรูปแบบ JSON
      },
      dataSrc: function (json) {
        return json.data || []; // รับข้อมูลจาก API และใช้ข้อมูลที่ส่งกลับมา
      },
      beforeSend: function (jqXHR) {
        if (AppState.ajax.topDonate) {
          AppState.ajax.topDonate.abort();
        }
        AppState.ajax.topDonate = jqXHR;
      },
      complete: function () {
        AppState.ajax.topDonate = null;
      },
      error: function (xhr, status, error) {
        if (status === "abort") {
          console.log("คำขอถูกยกเลิก");
        } else {
          console.log("เกิดข้อผิดพลาด: " + error);
        }
      },
    },
    columns: [
      {
        data: null,
        className: "text-center",
        render: function (data, type, row, meta) {
          const index = meta.row + 1;

          //  if (type === "display") {
          //     switch (index) {
          //         case 1:
          //             return '<i class="bi bi-1-circle-fill text-warning fs-4"></i>';
          //         case 2:
          //             return '<i class="bi bi-2-circle-fill" style="color:#C0C0C0; font-size:1.4rem;"></i>';
          //         case 3:
          //             return '<i class="bi bi-3-circle-fill" style="color:#CD7F32; font-size:1.3rem;"></i>';
          //         default:
          //             return index;
          //     }
          // }
          if (type === "filter") {
            return index; // ให้ search แบบนี้
          }
          return index; // ใช้รูปแบบ สำหรับ sorting
        },
      },
      { data: "studentName" },
      {
        data: "gradeLevel_Room",
        className: "text-center",
      },
      {
        data: "totalDonate",
        className: "text-end",
        createdCell: function (td, cellData, rowData, row, col) {
          td.classList.add("text-success"); // ใส่เฉพาะ td
        },
        render: function (data, type, row, meta) {
          if (type === "display") {
            return data.toLocaleString("en-US", {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2,
            });
          }
          if (type === "filter") {
            return data; // ให้ search แบบนี้
          }
          return data; // ใช้รูปแบบ สำหรับ sorting
        },
      },
    ],
    rowCallback: function (row, data) {
      $(row)
        .css("cursor", "pointer")
        .off("click")
        .on("click", function () {
          Outstandings(data.studentId);
        });
    },
    initComplete: function () {
      const table = this.api();

      // ดึง container ของปุ่ม DataTables
      const buttonsContainer = table.buttons().container();

      // ย้ายปุ่มไปไว้ตรง div ที่ต้องการ
      $("#dtExcelPaidBtn").empty().append(buttonsContainer);

      $("#dtExcelPaidBtn .dt-button")
        .addClass("btn btn-outline-success px-3 py-2")
        .removeClass("dt-button")
        .css("margin", "0");
    },
    buttons: [
      {
        extend: "excelHtml5",
        text: '<i class="bi bi-box-arrow-right"></i> Export Excel',
        className: "btn btn-success text-white px-3 py-2",
        filename: function () {
          return `รายชื่อผู้บริจาค-ปีการศึกษา${$("#schoolYearSelect :selected").text()}_${moment().format("YYYY-MM-DD_HH:mm:ss")}`;
        },
        exportOptions: {
          columns: ":visible",
        },
        customize: function (xlsx) {
          let sheetName = $("#schoolYearSelect :selected").text();

          // ป้องกัน error ของ Excel
          sheetName = sheetName.replace(/[\\\/\?\*\[\]]/g, "").substring(0, 31);

          // ✅ วิธีเปลี่ยนชื่อ sheet ที่ถูกต้อง
          const workbook = xlsx.xl["workbook.xml"];
          const sheets = workbook.getElementsByTagName("sheet");

          sheets[0].setAttribute("name", sheetName);
        },
        action: function (e, dt, button, config) {
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
              // 👉 เรียก action เดิมของ excelHtml5
              $.fn.dataTable.ext.buttons.excelHtml5.action.call(
                this,
                e,
                dt,
                button,
                config,
              );
            }
          });
        },
      },
    ],
    processing: true,
    paging: true,
    pageLength: 10,
    lengthChange: false,
    searching: true,
    ordering: true,
    info: true,
    responsive: false,
    autoWidth: false,
    language: {
      search: "_INPUT_",
      searchPlaceholder: "ค้นหา...",
      paginate: {
        previous: "ก่อนหน้า",
        next: "ถัดไป",
      },
      zeroRecords: "ไม่พบข้อมูล",
      info: "แสดง _START_ ถึง _END_ จาก _TOTAL_ รายการ",
      infoEmpty: "ไม่มีข้อมูล",
      processing: "กำลังโหลดข้อมูล...",
    },
    dom: "lrtip",
    drawCallback: function () {
      // ฟังก์ชันที่ถูกเรียกทุกครั้งที่ตารางมีการวาดใหม่ เช่น เมื่อเปลี่ยนหน้า เรียงลำดับ หรือกรองข้อมูล

      var tableEl = document.getElementById("topDonateTable");

      // ตรวจว่ามี div ครอบอยู่แล้วหรือยัง
      if (!tableEl.parentElement.classList.contains("table-responsive")) {
        var wrapper = document.createElement("div");
        wrapper.className = "table-responsive";

        // ย้าย table เข้า div
        tableEl.parentNode.insertBefore(wrapper, tableEl);
        wrapper.appendChild(tableEl);
      }
    },
  });
}
