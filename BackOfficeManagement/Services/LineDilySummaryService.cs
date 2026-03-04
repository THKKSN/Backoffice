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

namespace BackOfficeManagement.Services
{
    public class LineDilySummaryService : ILineDailySummaryRepository
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;
        private readonly ILineMessageRepository _lineMessage;
        private readonly ILogger<LineDilySummaryService> _logger;

        public LineDilySummaryService(
                IHttpClientFactory clientFactory,
                ApplicationDbContext context,
                ILineMessageRepository lineMessage,
                ILogger<LineDilySummaryService> logger
        )
        {
            _clientFactory = clientFactory;
            _context = context;
            _lineMessage = lineMessage;
            _logger = logger;
        }

        public async Task<bool> SendDailySummary(string schoolYearName, DateTime date, string groupId)
        {
            var year = await GetSchoolYearByName(schoolYearName);

            var daily = await GetDailySummary(year.Id, date);
            var annual = await GetAnnualSummary(year.Id);

            var message = BuildDailyMessage(annual, daily);

            return await _lineMessage.SendMessageToGroup(groupId, message);
        }

        private async Task<SchoolYearDataModel> GetSchoolYearByName(string schoolYearName)
        {
            var year = await _context.SchoolYearData
                .FirstOrDefaultAsync(x => x.Name == schoolYearName);

            if (year == null)
                throw new Exception($"ไม่พบปีการศึกษา {schoolYearName}");

            return year;
        }

        public async Task<DailySummaryDto> GetDailySummary(int schoolYear, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            var todayDonation = await _context.DonateDetailsData
                .Where(d =>
                    d.SchoolYear_Id == schoolYear &&
                    d.Status == 1 &&
                    d.CreateDate >= start &&
                    d.CreateDate < end
                )
                .SumAsync(d => (decimal?)d.Donate) ?? 0;

            var todayDonatorCount = await _context.DonateData
                .Join(_context.DonateDetailsData,
                    d => d.Id,
                    dd => dd.Donate_Id,
                    (d, dd) => new { d.Student_Id, dd })
                .Where(x =>
                    x.dd.SchoolYear_Id == schoolYear &&
                    x.dd.Status == 1 &&
                    x.dd.CreateDate >= start &&
                    x.dd.CreateDate < end
                )
                .Select(x => x.Student_Id)
                .Distinct()
                .CountAsync();

            return new DailySummaryDto
            {
                Date = start,
                TodayDonation = todayDonation,
                TodayDonatorCount = todayDonatorCount
            };
        }

        private async Task<AnnualSummaryDto> GetAnnualSummary(int schoolYear)
        {
            var year = await _context.SchoolYearData
                    .FirstOrDefaultAsync(x => x.Id == schoolYear);

            if (year == null)
                throw new Exception("School year not found");

            var totalYearDonation = await _context.DonateDetailsData
                .Where(d => d.SchoolYear_Id == schoolYear && d.Status == 1)
                .SumAsync(d => (decimal?)d.Donate) ?? 0;

            var terms = await _context.TermData
                .Where(t => t.SchoolYear_Id == schoolYear)
                .OrderBy(t => t.Id)
                .Select(t => t.Id)
                .ToListAsync();

            var term1Id = terms.ElementAtOrDefault(0);
            var term2Id = terms.ElementAtOrDefault(1);

            var term1Donation = term1Id == 0 ? 0 :
                await _context.DonateDetailsData
                    .Where(d => d.Term_Id == term1Id && d.Status == 1)
                    .SumAsync(d => (decimal?)d.Donate) ?? 0;

            var term2Donation = term2Id == 0 ? 0 :
                await _context.DonateDetailsData
                    .Where(d => d.Term_Id == term2Id && d.Status == 1)
                    .SumAsync(d => (decimal?)d.Donate) ?? 0;

            var students = await _context.StudentHistoryClassData
                .Where(s => s.School_Year_Id == schoolYear)
                .Select(s => new
                {
                    s.Student_Id,
                    s.Grade_Level_Id,
                    s.Term_Id
                })
                .ToListAsync();

            var donateSettings = await _context.DonateSettingData
                .Where(d => d.SchoolYear_Id == schoolYear && d.IsActive == true)
                .Select(d => new
                {
                    d.Grade_Level_Id,
                    d.Term_Id,
                    d.Donate
                })
                .ToListAsync();

            decimal? totalExpectedAmount = (
                from sh in students
                join ds in donateSettings
                    on new { sh.Grade_Level_Id, sh.Term_Id }
                    equals new { ds.Grade_Level_Id, ds.Term_Id }
                select ds.Donate
            ).Sum();

            var totalOutstandingAmount = totalExpectedAmount - totalYearDonation;
            if (totalOutstandingAmount < 0)
                totalOutstandingAmount = 0;

            var donateByStudent = await _context.DonateData
                .Join(
                    _context.DonateDetailsData
                        .Where(dd => dd.SchoolYear_Id == schoolYear && dd.Status == 1),
                    d => d.Id,
                    dd => dd.Donate_Id,
                    (d, dd) => new { d.Student_Id, dd.Donate }
                )
                .GroupBy(x => x.Student_Id)
                .Select(g => new
                {
                    Student_Id = g.Key,
                    TotalDonate = g.Sum(x => x.Donate)
                })
                .ToListAsync();

            int totalStudentDonate = 0;
            int totalStudentNotDonate = 0;

            foreach (var s in students.GroupBy(x => x.Student_Id))
            {
                var donated = donateByStudent
                    .FirstOrDefault(x => x.Student_Id == s.Key)?.TotalDonate ?? 0;

                var expected = (
                    from sh in s
                    join ds in donateSettings
                        on new { sh.Grade_Level_Id, sh.Term_Id }
                        equals new { ds.Grade_Level_Id, ds.Term_Id }
                    select ds.Donate
                ).Sum();

                if (donated >= expected)
                    totalStudentDonate++;
                else
                    totalStudentNotDonate++;
            }

            return new AnnualSummaryDto
            {
                SchoolYearName = year.Name,

                TotalDonationYear = totalYearDonation,
                TotalExpectedAmount = totalExpectedAmount,
                TotalOutstandingAmount = Math.Max(0m, (totalExpectedAmount ?? 0) - totalYearDonation),

                TotalStudentDonate = totalStudentDonate,
                TotalStudentNotDonate = totalStudentNotDonate,
                TotalStudent = students.Select(x => x.Student_Id).Distinct().Count(),
            };
        }

        private string BuildDailyMessage(AnnualSummaryDto annual, DailySummaryDto daily)
        {
            var message = new StringBuilder();

            message.AppendLine("📅 Daily Summary Donation");
            message.AppendLine($"Date : {daily.Date:dd/MMM/yyyy}");
            message.AppendLine();

            message.AppendLine("💰 Total Donate");
            message.AppendLine($"{daily.TodayDonation:N2} บาท");
            message.AppendLine();

            message.AppendLine("👥 Donors");
            message.AppendLine($"{daily.TodayDonatorCount} คน");
            message.AppendLine();

            message.AppendLine($"📊 Dashboard for School year {annual.SchoolYearName}");
            message.AppendLine();

            message.AppendLine($"Target : {annual.TotalExpectedAmount:N2} บาท");
            message.AppendLine($"Total of Donors : {annual.TotalStudent} คน");
            message.AppendLine();

            message.AppendLine($"Donated : {annual.TotalDonationYear:N2} บาท");
            message.AppendLine($"Total of Donors is completed : {annual.TotalStudentDonate} คน");
            message.AppendLine();

            message.AppendLine($"Outstanding : {annual.TotalOutstandingAmount:N2} บาท");
            message.AppendLine($"People Who Have Not Donated : {annual.TotalStudentNotDonate} คน");
            message.AppendLine();


            return message.ToString();
        }
    }
}