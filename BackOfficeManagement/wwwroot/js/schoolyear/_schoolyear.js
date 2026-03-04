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

function CreateShcoolYear() {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/" + "Create";

  $("#ViewShcoolYear").html(null);
  $("#CreateShcoolYear").html(null);
  $("#UpdateShcoolYear").html(null);

  $.ajax({
    type: "Get",
    url: urlGet,
    success: function (data) {
      $("#CreateShcoolYear").html(data);
      $("#CreateShcoolYear").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function ViewShcoolYear(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/ViewTerm?id=" + id;

  $("#ViewShcoolYear").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#ViewShcoolYear").html(data);
      $("#ViewShcoolYear").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

function UpdateShcoolYear(id) {
  var url = $("#urlDefualt").data("request-url");
  var urlGet = url + "/Update?id=" + id;

  $("#UpdateShcoolYear").html(null);

  $.ajax({
    type: "GET",
    url: urlGet,
    success: function (data) {
      $("#UpdateShcoolYear").html(data);
      $("#UpdateShcoolYear").find(".offcanvas").offcanvas("show");
    },
    error: function (response) {
      console.log(response.responseText);
    },
  });
}

$(document).on("change", ".switch-term-status", function () {
  var termId = $(this).data("id");
  var isActive = $(this).is(":checked");
var url = $("#urlDefualt").data("request-url");
  $.ajax({
    type: "POST",
      url: url + '/ToggleTermStatus',
    data: { id: termId, active: isActive },
    success: function () {
      console.log("Updated term status!");
    },
  });
});
