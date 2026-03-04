function showLoading() {
  $("#annualSummaryWrapper").addClass("loading-dim");
  $("#annualSummaryLoading").removeClass("d-none");
}

function hideLoading() {
  $("#annualSummaryWrapper").removeClass("loading-dim");
  $("#annualSummaryLoading").addClass("d-none");
}

function loadAnnualSummary() {
  $.ajax({
    url: DT_URLS.ANNUALSUMMARY,
    type: "GET",
    data: { schoolYear: AppState.selectedYear },

    beforeSend: function () {
      showLoading();
    },

    success: function (res) {
      /* ===== เงิน (บาท) ===== */
      animateNumber({
        el: document.getElementById("totalExpectedAmount"),
        end: res.totalExpectedAmount,
        decimals: 2,
        suffix: " บาท",
      });

      animateNumber({
        el: document.getElementById("term1Donation"),
        end: res.term1Donation,
        decimals: 2,
        suffix: " บาท",
      });

      animateNumber({
        el: document.getElementById("term2Donation"),
        end: res.term2Donation,
        decimals: 2,
        suffix: " บาท",
      });

      animateNumber({
        el: document.getElementById("totalPaidAmount"),
        end: res.totalDonationYear,
        decimals: 2,
        suffix: " บาท",
      });

      animateNumber({
        el: document.getElementById("totalUnpaidAmount"),
        end: res.totalOutstandingAmount,
        decimals: 2,
        suffix: " บาท",
      });

      /* ===== จำนวนคน ===== */
      animateNumber({
        el: document.getElementById("totalStudent"),
        end: res.totalStudent,
        suffix: " คน",
      });

      animateNumber({
        el: document.getElementById("totalPaidStudents"),
        end: res.totalStudentDonate,
        suffix: " คน",
      });

      animateNumber({
        el: document.getElementById("totalUnpaidStudents"),
        end: res.totalStudentNotDonate,
        suffix: " คน",
      });
    },

    error: function (err) {
      console.error(err);
    },

    complete: function () {
      hideLoading();
    },
  });
}

function animateNumber({
  el,
  start = 0,
  end = 0,
  duration = 900,
  suffix = "",
  decimals = 0,
  locale = "th-TH",
}) {
  let startTime = null;

  const formatter = new Intl.NumberFormat(locale, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });

  function step(timestamp) {
    if (!startTime) startTime = timestamp;
    const progress = Math.min((timestamp - startTime) / duration, 1);
    const value = start + (end - start) * progress;
    el.textContent = formatter.format(value) + suffix;

    if (progress < 1) {
      requestAnimationFrame(step);
    }
  }

  requestAnimationFrame(step);
}
