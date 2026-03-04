var $ = jQuery;

//#region "Option Select2"
$(function () {
    $('.select2').each(function () {
        $(this).select2({
            dropdownParent: $(this).parent(),
            width: "100%",
        });
    });
});
//#endregion

//#region "Create"
function CreateStudent() {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "Create";

    $("#ViewStudent").html(null);
    $("#CreateStudent").html(null);
    $("#UpdateStudent").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#CreateStudent").html(data);
            $("#CreateStudent").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}

function upgradeStudent() {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "UpgradeStudent";

    $("#ViewStudent").html(null);
    $("#CreateStudent").html(null);
    $("#UpdateStudent").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#UpgradeStudent").html(data);
            $("#UpgradeStudent").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}

function updateStatus() {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "UpdateStatus";

    $("#ViewStudent").html(null);
    $("#CreateStudent").html(null);
    $("#UpdateStudent").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#updateStatus").html(data);
            $("#updateStatus").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}
//#endregion

function ViewStudent(Id) {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "Detail" + "?Id=" + Id;

    $("#ViewStudent").html(null);
    $("#CreateStudent").html(null);
    $("#UpdateStudent").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#ViewStudent").html(data);
            initGradeHistoryTable();
            $("#ViewStudent").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}


function UpdateStudent(Id) {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "Update" + "?Id=" + Id;

    $("#ViewStudent").html(null);
    $("#CreateStudent").html(null);
    $("#UpdateStudent").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#UpdateStudent").html(data);
            $("#UpdateStudent").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}

function DeleteStudent(Id) {
    var url = $("#urlDefualt").data("request-url");
    var urlPost = url + "/" + "Delete";
    Swal.fire({
        title: "ยืนยันการลบข้อมูล",
        text: "คุณต้องการดำเนินการต่อหรือไม่?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#E3A06D",
        cancelButtonColor: "#d33",
        confirmButtonText: "ยืนยันการลบ",
        cancelButtonText: "ยกเลิก",
        allowOutsideClick: false,
        allowEscapeKey: false
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: urlPost,
                type: 'POST',
                data: { Id },
                success: function (response) {
                    console.log('Response:', response);
                    if (response.status) {
                        Swal.fire({
                            title: "ดำเนินการสำเร็จ",
                            text: "ข้อมูลนักเรียนถูกลบเรียบร้อยแล้ว",
                            icon: "success",
                            timer: 2000,
                            timerProgressBar: true,
                            allowOutsideClick: true,
                            allowEscapeKey: true,
                            didClose: () => {
                                location.reload(true);
                                //$('#tblStudent').DataTable().ajax.reload(null, false);
                            }
                        });
                    } else {
                        Swal.fire({
                            title: "ล้มเหลว",
                            text: "ไม่สามารถลบข้อมูลรายการนี้ได้",
                            icon: "error"
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error:', error);
                    Swal.fire({
                        title: "เกิดข้อผิดพลาด!",
                        text: error.toString(),
                        icon: "error"
                    });
                }
            });
        }
    });
}

function initGradeHistoryTable() {
    const table = $('#tblYearList');

    if (!table.length) return;

    // กัน init ซ้ำ
    if ($.fn.DataTable.isDataTable(table)) {
        table.DataTable().destroy();
    }

    table.DataTable({
        paging: false,
        searching: false,
        ordering: true,
        info: false,
        lengthChange: false,
        order: [[0, 'desc'],
            [1, 'desc']],
        columnDefs: [
            { orderable: false, targets: 3 }
        ],
        language: {
            search: "ค้นหา:",
            zeroRecords: "ไม่พบข้อมูล",
            paginate: {
                previous: "ก่อนหน้า",
                next: "ถัดไป"
            }
        }
    });
}
