function isDarkMode() {
  return document.body.classList.contains("dark-mode");
}

function getRoomChartTheme() {
  const dark = isDarkMode();

  return {
    text: dark ? "#f8fafc" : "#020617",
    grid: dark ? "#334155" : "#e5e7eb",
    donated: dark ? "#60a5fa" : "#0d6efd",
    notDonated: dark ? "#60a5fa50" : "#0d6efd50",
  };
}

function loadRoomLabels() {
  const schoolYearId = $("#schoolYearSelect").val();
  if (!schoolYearId) return;

  $.ajax({
    url: DT_URLS.CHART,
    type: "GET",
    dataType: "json",
    data: { schoolYearId: schoolYearId },
    success: function (labels) {
      AppState.chart.roomLabels = labels || [];

      initRoomChart();
    },
  });
}

function initRoomChart() {
  const ctx = $("#roomChart");
  if (!ctx) return;
  const theme = getRoomChartTheme();

  roomChart = new Chart(ctx, {
    type: "bar",
    data: {
      labels: [],
      datasets: [
        {
          label: "บริจาคแล้ว",
          data: [],
          backgroundColor: theme.donated,
        },
        {
          label: "ยังไม่บริจาค",
          data: [],
          backgroundColor: theme.notDonated,
        },
      ],
    },
    options: {
      responsive: true,
      plugins: {
        legend: {
          position: "top",
          labels: {
            color: theme.text,
          },
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              const index = context.dataIndex;
              const row = context.dataset.rawData[index];

              if (!row) return "";

              const isPaid = context.datasetIndex === 0;

              const amount = context.raw.toLocaleString();
              const percent = isPaid ? row.paidPercent : row.unpaidPercent;

              const students = isPaid ? row.paidStudents : row.unpaidStudents;

              return [
                `${context.dataset.label}: ${amount} บาท (${percent}%)`,
                `นักเรียน: ${students} / ${row.totalStudents} คน`,
              ];
            },
          },
        },
      },
      scales: {
        x: {
          stacked: true,
          ticks: {
            color: theme.text, // ✅ x label
          },
          grid: {
            color: theme.grid,
          },
        },
        y: {
          stacked: true,
          beginAtZero: true,
          ticks: {
            color: theme.text,
          },
          grid: {
            color: theme.grid,
          },
        },
      },
    },
  });
}

function loadTermBySchoolYear(schoolYearId) {
  const $term = $("#graphRoomSelect");

  $term.empty().append('<option value="">ทุกเทอม</option>').trigger("change");

  clearRoomChart();

  if (!schoolYearId) return;

  $.ajax({
    url: DT_URLS.TERMBYYEAR,
    type: "GET",
    dataType: "json",
    data: { schoolYearId: schoolYearId },
    success: function (terms) {
      if (!terms || terms.length === 0) return;

      terms.forEach((t) => {
        $term.append(
          $("<option>", {
            value: t.id,
            text: t.text,
          }),
        );
      });

      // สำหรับ select2
      $term.trigger("change.select2");
    },
  });
}

$(document).on("change", "#graphRoomSelect", function () {
  const termId = $(this).val();
  const schoolYearId = $("#schoolYearSelect").val();

  if (!schoolYearId) {
    clearRoomChart();
    return;
  }

  $.ajax({
    url: DT_URLS.GRAPHROOM,
    type: "GET",
    dataType: "json",
    data: {
      schoolYear: AppState.selectedYear,
      termId: termId,
    },
    success: function (data) {
      updateRoomChart(data);
    },
    error: function (xhr, status, error) {
      console.error("GraphRoomDashboard error:", error);
      clearRoomChart();
    },
  });
});

function updateRoomChart(data) {
  if (!data || data.length === 0) {
    $("#graph-room-empty").removeClass("d-none");
    $("#roomChart").hide();
    return;
  }

  $("#graph-room-empty").addClass("d-none");
  $("#roomChart").show();

  roomChart.data.labels = data.map((x) => x.roomName);
  roomChart.data.datasets[0].data = data.map((x) => x.paid);
  roomChart.data.datasets[1].data = data.map((x) => x.unpaid);
  roomChart.data.datasets.forEach((ds, i) => {
    ds.rawData = data;
  });
  roomChart.update();
}

function clearRoomChart() {
  if (!roomChart) return;

  roomChart.data.labels = [];
  roomChart.data.datasets.forEach((ds) => (ds.data = []));
  roomChart.update();
}

function updateRoomChartTheme() {
  if (!roomChart) return;

  const theme = getRoomChartTheme();

  // update legend
  roomChart.options.plugins.legend.labels.color = theme.text;

  // update scales
  roomChart.options.scales.x.ticks.color = theme.text;
  roomChart.options.scales.x.grid.color = theme.grid;

  roomChart.options.scales.y.ticks.color = theme.text;
  roomChart.options.scales.y.grid.color = theme.grid;

  // update dataset colors
  roomChart.data.datasets[0].backgroundColor = theme.donated;
  roomChart.data.datasets[1].backgroundColor = theme.notDonated;

  roomChart.update();
}

document.addEventListener("themeChanged", function () {
  updateRoomChartTheme();
  updateDonutTheme();
});
