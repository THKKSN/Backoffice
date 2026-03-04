function isDarkMode() {
  return document.body.classList.contains("dark-mode");
}

function getColor(percent) {
  const dark = isDarkMode();

  if (percent >= 100) {
    return dark ? "#34d399" : "#198754"; // green
  }

  return dark ? "#60a5fa" : "#0d6efd"; // blue
}

function renderDonut(id, paid, unpaid, percent) {
  const el = document.getElementById(id);
  const unpaidColor = isDarkMode() ? "rgba(255,255,255,0.15)" : "#0d6efd50";

  if (!el) return;

  if (el._chartInstance) {
    el._chartInstance.destroy();
  }

  el._chartInstance = new Chart(el, {
    type: "doughnut",
    data: {
      datasets: [
        {
          data: [paid, unpaid],
          backgroundColor: [getColor(percent), unpaidColor],
          borderWidth: 0,
        },
      ],
      centerText: percent,
      centerTextColor: isDarkMode() ? "#f8fafc" : "#020617",
    },
    options: {
      cutout: "78%",
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: function (ctx) {
              return ctx.raw + " คน";
            },
          },
        },
      },
    },
    plugins: [window.centerTextPlugin],
  });
}

function percent(paid, total) {
  if (!total) return "0.00";
  return ((paid * 100) / total).toFixed(2); // ⭐ ทศนิยม 2 ตำแหน่ง
}
function loadDonutDashboard() {
  const schoolYear = $("#schoolYearSelect").val();
  const gradeLevelId = $("#gradeLevelSelect").val() || null;

  if (!schoolYear) return;

  $.ajax({
    url: DT_URLS.DONUT,
    type: "GET",
    dataType: "json",
    data: {
      schoolYear: schoolYear,
      gradeLevelId: gradeLevelId,
    },
    success: function (res) {
      // TERM 1
      renderDonut(
        "term1Chart",
        res.term1.paidStudents,
        res.term1.unpaidStudents,
        percent(res.term1.paidStudents, res.term1.totalStudents),
      );

      $("#term1Text").text(
        `ยังไม่บริจาค ${res.term1.unpaidStudents} / ${res.term1.totalStudents} คน`,
      );

      $("#term1Amount").text(
        `บริจาคแล้ว ${res.term1.totalPaid.toLocaleString()} / ${res.term1.expectedAmount.toLocaleString()} บาท`,
      );

      // TERM 2
      renderDonut(
        "term2Chart",
        res.term2.paidStudents,
        res.term2.unpaidStudents,
        percent(res.term2.paidStudents, res.term2.totalStudents),
      );

      $("#term2Text").text(
        `ยังไม่บริจาค ${res.term2.unpaidStudents} / ${res.term2.totalStudents} คน`,
      );

      $("#term2Amount").text(
        `บริจาคแล้ว ${res.term2.totalPaid.toLocaleString()} / ${res.term2.expectedAmount.toLocaleString()} บาท`,
      );
    },
  });
}

if (!window.centerTextPlugin) {
  window.centerTextPlugin = {
    id: "centerText",
    beforeDraw(chart) {
      const { ctx, width, height } = chart;
      const text = chart.data.centerText;
      if (!text) return;

      const isDark = document.body.classList.contains("dark-mode");

      ctx.save();
      ctx.font = "700 18px 'Inter', sans-serif";
      ctx.fillStyle =
        chart.data.centerTextColor || (isDark ? "#f8fafc" : "#020617"); // ✅ เปลี่ยนสีตาม theme
      ctx.textAlign = "center";
      ctx.textBaseline = "middle";
      ctx.fillText(text + "%", width / 2, height / 2);
      ctx.restore();
    },
  };
}

function updateDonutTheme() {
  const donutIds = ["term1Chart", "term2Chart"];

  donutIds.forEach(id => {
    const el = document.getElementById(id);
    if (!el || !el._chartInstance) return;

    const chart = el._chartInstance;

    const paid = chart.data.datasets[0].data[0];
    const unpaid = chart.data.datasets[0].data[1];
    const total = paid + unpaid;

    const percentValue = total
      ? ((paid * 100) / total).toFixed(2)
      : "0.00";

    const unpaidColor = isDarkMode()
      ? "rgba(255,255,255,0.15)"
      : "#0d6efd50";

    chart.data.datasets[0].backgroundColor = [
      getColor(percentValue),
      unpaidColor,
    ];

    chart.data.centerTextColor = isDarkMode()
      ? "#f8fafc"
      : "#020617";

    chart.update();
  });
}