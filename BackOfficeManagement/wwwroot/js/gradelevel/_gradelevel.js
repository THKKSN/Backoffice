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

function CreateGradeLevel() {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "Create";

  $("#ViewGradeLevel").html(null);
  $("#CreateGradeLevel").html(null);
  $("#UpdateGradeLevel").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#CreateGradeLevel").html(data);
      $("#CreateGradeLevel").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function ViewGradeLevel(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/Detail?id=" + id;

  $("#ViewGradeLevel").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#ViewGradeLevel").html(data);
      $("#ViewGradeLevel").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function UpdateGradeLevel(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/Update?id=" + id;

  $("#UpdateGradeLevel").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#UpdateGradeLevel").html(data);
      $("#UpdateGradeLevel").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

$("#GradeLevelForm").submit(function (e) {
  e.preventDefault();

  var formData = $(this).serialize();

  $.ajax({
    type: "POST",
    url: $(this).attr("action"),
    data: formData,
    success: function (res) {
      if (res.status === 200) {
        $("#CreateGradeLevel").find(".offcanvas").offcanvas("hide");
        gradeTable.ajax.reload();
      } else {
        alert(res.message);
      }
    },
    error: function (err) {
      console.log(err);
    },
  });
});



