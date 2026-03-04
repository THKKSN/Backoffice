using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BackOfficeManagement.Data;
using BackOfficeManagement.Interface;
using Microsoft.EntityFrameworkCore;
using BackOfficeLibrary;

public class DailyLineSummaryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyLineSummaryWorker> _logger;

    public DailyLineSummaryWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyLineSummaryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                if (now.Hour == 9 && now.Minute == 0 && now.Second < 30)
                {
                    using var scope = _scopeFactory.CreateScope();

                    var context = scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();

                    var summaryService = scope.ServiceProvider
                        .GetRequiredService<ILineDailySummaryRepository>();

                    var lineConfig = await context.LineConfig.Where(x => x.channel_id == "channel_id").Select(x => x.line_group_id).FirstOrDefaultAsync(stoppingToken);

                    var targetDate = DateTime.Today.AddDays(-1);
                    var schoolYearName = GetSchoolYearName(targetDate);

                    await summaryService.SendDailySummary(
                        schoolYearName: schoolYearName,
                        date: targetDate,
                        groupId: lineConfig
                    );

                    _logger.LogInformation("Daily LINE summary sent.");

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // กันยิงซ้ำ
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DailyLineSummaryWorker error");
            }
        }
    }
    private static string GetSchoolYearName(DateTime date)
    {
        // ถ้าเดือน >= พฤษภาคม → ใช้ปีนั้น
        // ถ้า ม.ค.–เม.ย. → ถอยไปปีที่แล้ว
        var buddhistYear = date.Year + 543;
        return date.Month >= 5
            ? buddhistYear.ToString()
            : (buddhistYear - 1).ToString();
    }

}
