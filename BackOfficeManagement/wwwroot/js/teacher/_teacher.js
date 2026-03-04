$(function () {
    $('.select2').each(function () {
        $(this).select2({
            dropdownParent: $(this).parent(),
            width: "100%",
        });
    });
});

//#region "Create"
function CreateTeacher() {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "Create";

    $("#ViewTeacher").html(null);
    $("#CreateTeacher").html(null);
    $("#UpdateTeacher").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#CreateTeacher").html(data);
            $("#CreateTeacher").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}
//#endregion

//#region "Update"
function UpdateTeacher(Id) {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "Update" + "?Id=" + Id;

    $("#ViewTeacher").html(null);
    $("#CreateTeacher").html(null);
    $("#UpdateTeacher").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#UpdateTeacher").html(data);
            $("#UpdateTeacher").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}
//#endregion

//#region "Detail"
function ViewTeacher(Id) {
    var url = $("#urlDefualt").data("request-url");
    var urlGet = url + "/" + "Detail" + "?Id=" + Id;

    $("#ViewTeacher").html(null);
    $("#CreateTeacher").html(null);
    $("#UpdateTeacher").html(null);

    $.ajax({
        type: "Get",
        url: urlGet,
        success: function (data) {
            $("#ViewTeacher").html(data);
            $("#ViewTeacher").find(".offcanvas").offcanvas("show");
        }, error: function (response) {
            console.log(response.responseText);
        }
    });
}
//#endregion

//#region "Detail"
function DeleteTeacher(Id) {
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
                            text: "ข้อมูลคุณครูถูกลบเรียบร้อยแล้ว",
                            icon: "success",
                            timer: 2000,
                            timerProgressBar: true,
                            allowOutsideClick: true,
                            allowEscapeKey: true,
                            didClose: () => {
                                $('#tblTeacher').DataTable().ajax.reload(null, false);
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
//#endregion