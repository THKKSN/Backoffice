using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using BackOfficeManagement.Data;
using BackOfficeManagement.Interface;
using BackOfficeLibrary;

namespace BackOfficeManagement.Services
{
    public class DashboardService : IDashboardRepo
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;

        public DashboardService(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ApplicationDbContext context
            )
        {
            _clientFactory = clientFactory;
            _context = context;

        }


        public async Task<List<TopNotDonateVM>> GetTopNotDonateAsync(DataTableReq input)
        {
            // 1) หา history ล่าสุดแบบเดิม
            var latestHistoryQuery =
                from h in _context.StudentHistoryClassData
                where h.School_Year_Id == input.schoolYearId
                where !_context.StudentHistoryClassData.Any(h2 =>
                    h2.Student_Id == h.Student_Id &&
                    h2.School_Year_Id == h.School_Year_Id &&
                    (
                        h2.Term_Id > h.Term_Id ||
                        (h2.Term_Id == h.Term_Id && h2.Id > h.Id)
                    )
                )
                select h;

            if (input.gradeLevelId.HasValue)
            {
                latestHistoryQuery = latestHistoryQuery
                    .Where(h => h.Grade_Level_Id == input.gradeLevelId.Value);
            }

            var latestHistory = await latestHistoryQuery.ToListAsync();


            //----------------------------------------------------------------------
            // 2) ดึงยอดบริจาคจริง (แยกตาม student/year/grade/term)
            //----------------------------------------------------------------------
            var donateList = await (
    from sh in _context.StudentHistoryClassData
    where sh.School_Year_Id == input.schoolYearId

    join d in _context.DonateData
        on sh.Student_Id equals d.Student_Id into dg
    from d in dg.DefaultIfEmpty()

    join dd in _context.DonateDetailsData
        on new
        {
            DonateId = d.Id,
            SchoolYear_Id = sh.School_Year_Id,
            Term_Id = sh.Term_Id
        }
        equals new
        {
            DonateId = dd.Donate_Id,
            SchoolYear_Id = (int?)dd.SchoolYear_Id,
            Term_Id = (int?)dd.Term_Id
        }
        into ddg
    from dd in ddg
        .Where(x => x == null || x.Status == 1)
        .DefaultIfEmpty()

    group dd by new
    {
        sh.Student_Id,
        sh.School_Year_Id,
        sh.Grade_Level_Id,
        sh.Term_Id
    } into g

    select new
    {
        g.Key.Student_Id,
        g.Key.School_Year_Id,
        g.Key.Grade_Level_Id, // ใช้ grade จาก history
        g.Key.Term_Id,
        TotalDonate = g.Sum(x => x != null ? x.Donate : 0)
    }
).ToListAsync();


            //----------------------------------------------------------------------
            // 3) ดึง DonateSetting ต่อ term
            //----------------------------------------------------------------------
            var settingList = await _context.DonateSettingData
     .Where(x => x.SchoolYear_Id == input.schoolYearId)
     .GroupBy(x => new { x.SchoolYear_Id, x.Grade_Level_Id, x.Term_Id })
     .Select(g => new
     {
         g.Key.SchoolYear_Id,
         g.Key.Grade_Level_Id,
         g.Key.Term_Id,
         DonateSetting = g.Sum(x => x.Donate)
     })
     .ToListAsync();


            //----------------------------------------------------------------------
            // 4) JOIN → คำนวณ Diff แบบเทอมต่อเทอม
            //----------------------------------------------------------------------
            var resultByTerm =
    from d in donateList
    join s in settingList
        on new { scy = d.School_Year_Id, d.Grade_Level_Id, d.Term_Id }
        equals new { scy = s.SchoolYear_Id, s.Grade_Level_Id, s.Term_Id }
    select new
    {
        d.Student_Id,
        d.School_Year_Id,
        d.Grade_Level_Id,
        d.Term_Id,
        d.TotalDonate,
        s.DonateSetting,
        DiffDonate = s.DonateSetting - d.TotalDonate
    };


            // เอาเฉพาะเทอมที่จ่ายไม่ครบ
            var filteredByTerm = resultByTerm
                .Where(x => x.DonateSetting > x.TotalDonate)
                .ToList();



            //----------------------------------------------------------------------
            // 5) รวมยอดต่อ "นักเรียน" ทั้งปี
            //----------------------------------------------------------------------
            var totalByStudent = filteredByTerm
                .GroupBy(x => new { x.Student_Id, x.School_Year_Id, x.Grade_Level_Id })
                .Select(g => new
                {
                    g.Key.Student_Id,
                    g.Key.School_Year_Id,
                    g.Key.Grade_Level_Id,
                    TotalDonate = g.Sum(x => x.TotalDonate),
                    TotalSetting = g.Sum(x => x.DonateSetting),
                    TotalDiff = g.Sum(x => x.DiffDonate)
                })
                .OrderByDescending(x => x.TotalDiff)
                .ToList();


            //----------------------------------------------------------------------
            // 6) JOIN กลับไปหา room/grade/name เพื่อสร้าง VM
            //----------------------------------------------------------------------
            var studentIds = totalByStudent.Select(x => x.Student_Id).ToList();
            var students = await _context.StudentData
                .Where(s => studentIds.Contains(s.Id.ToString()))
                .ToListAsync();

            var result = (
                from t in totalByStudent
                join h in latestHistory on t.Student_Id equals h.Student_Id
                join stu in students on t.Student_Id equals stu.Id.ToString()
                join g in _context.GradeLevelData on t.Grade_Level_Id equals g.Id
                join r in _context.RoomData on h.Room_Id equals r.Id
                join p in _context.PrefixData on stu.Prefix_Id equals p.Id
                select new TopNotDonateVM
                {
                    Code = stu.Code,
                    StudentName = $"{p.Abbreviation}{stu.FirstName} {stu.LastName}",
                    GradeLevel_Room = $"{g.Name}/{r.Name}",
                    Paid = t.TotalDonate,
                    Required = t.TotalSetting,
                    Remaining = t.TotalDiff
                }
            )
            .OrderByDescending(x => x.Remaining)
            .ToList();

            var Remaining = result.GroupBy(result => result.Remaining)
                .Select(group => new
                {
                    Remaining = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Remaining)
                .ToList();

            if (!String.IsNullOrEmpty(input.FilterBy))
            {
                result = result.Where(r => r.Remaining == decimal.Parse(input.FilterBy)).ToList();
            }
            return result; // List<TopNotDonateVM>
        }

        public async Task<List<TopDonateVM>> GetTopDonateAsync(DataTableReq input)
        {
            var latestHistory =
                from h in _context.StudentHistoryClassData
                where h.School_Year_Id == input.schoolYearId
                join h2 in _context.StudentHistoryClassData
                    on new { h.Student_Id, h.School_Year_Id }
                    equals new { h2.Student_Id, h2.School_Year_Id }
                    into gj
                from h2 in gj.DefaultIfEmpty()
                where h2 == null
                      || h.Term_Id > h2.Term_Id
                      || (h.Term_Id == h2.Term_Id && h.Id > h2.Id)
                select h;

            // Filter gradeLevelId
            if (input.gradeLevelId.HasValue)
            {
                latestHistory = latestHistory
                    .Where(h => h.Grade_Level_Id == input.gradeLevelId.Value);
            }

            // 2) รวมยอดบริจาค
            var donateQuery =
                from dd in _context.DonateDetailsData
                join d in _context.DonateData on dd.Donate_Id equals d.Id
                where dd.SchoolYear_Id == input.schoolYearId && dd.Status == 1
                group dd by d.Student_Id into g
                select new
                {
                    StudentId = g.Key,
                    TotalDonate = g.Sum(x => x.Donate)
                };

            // 3) Join ทุกอย่างเพื่อสร้าง TopDonateVM
            var query =
                from d in donateQuery
                join h in latestHistory on d.StudentId equals h.Student_Id
                join s in _context.StudentData on d.StudentId equals s.Id.ToString()
                join g in _context.GradeLevelData on h.Grade_Level_Id equals g.Id
                join r in _context.RoomData on h.Room_Id equals r.Id
                join p in _context.PrefixData on s.Prefix_Id equals p.Id
                orderby d.TotalDonate descending
                select new TopDonateVM
                {
                    StudentId = d.StudentId,
                    Code = s.Code,
                    StudentName = p.Abbreviation + s.FirstName + " " + s.LastName,
                    TotalDonate = d.TotalDonate,
                    GradeLevel_Room = g.Name + "/" + r.Name
                };

            var result = await query.ToListAsync();
            return result;
        }

        public async Task<List<StudentDonationSummaryVM>> GetStudentDonationSummaryAsync(DataTableReq input)
        {

            var outstandings = await (from history in _context.StudentHistoryClassData

                                      join year in _context.SchoolYearData
                                          on history.School_Year_Id equals year.Id

                                      join term in _context.TermData
                                          on history.Term_Id equals term.Id

                                      join grade in _context.GradeLevelData
                                          on history.Grade_Level_Id equals grade.Id

                                      join room in _context.RoomData
                                          on history.Room_Id equals room.Id

                                      join student in _context.StudentData
                                          on history.Student_Id equals student.Id.ToString()

                                      // 🔥 LEFT JOIN Individual ก่อน
                                      join individual in _context.DonateSettingIndividualData
                                              .Where(x => x.IsActive == true)
                                          on new
                                          {
                                              s = history.Student_Id,
                                              y = history.School_Year_Id,
                                              t = history.Term_Id
                                          }
                                          equals new
                                          {
                                              s = individual.Student_Id,
                                              y = individual.SchoolYear_Id,
                                              t = individual.Term_Id
                                          }
                                          into individualGroup
                                      from individual in individualGroup.DefaultIfEmpty()

                                          // 🔥 LEFT JOIN Setting กลาง
                                      join setting in _context.DonateSettingData
                                          on new
                                          {
                                              g = history.Grade_Level_Id,
                                              y = history.School_Year_Id,
                                              t = history.Term_Id
                                          }
                                          equals new
                                          {
                                              g = setting.Grade_Level_Id,
                                              y = setting.SchoolYear_Id,
                                              t = setting.Term_Id
                                          }
                                          into settingGroup
                                      from setting in settingGroup.DefaultIfEmpty()

                                      where history.School_Year_Id == input.schoolYearId && student.Status != 3

                                      select new
                                      {
                                          School_Year_Id = year.Id,
                                          StudentYear = year.Name ?? "",
                                          Code = student.Code,
                                          StudentName = student.FirstName + " " + student.LastName,
                                          Term = term.Name ?? "",
                                          Term_Id = term.Id,
                                          Grade_Level = grade.Name,
                                          Room = room.Name,

                                          // 🔥 PRIORITY LOGIC
                                          TotalAmount =
                                              individual != null
                                                  ? (individual.OverrideAmount ?? 0m)
                                                  : (setting != null ? setting.Donate ?? 0m : 0m),

                                          DonateAmount = (
                                              from donate in _context.DonateData
                                              join donatedt in _context.DonateDetailsData
                                                  on donate.Id equals donatedt.Donate_Id
                                              where donate.Student_Id == history.Student_Id
                                                  && donatedt.SchoolYear_Id == history.School_Year_Id
                                                  && donatedt.Term_Id == history.Term_Id
                                                  && donatedt.Status == 1
                                              select (decimal?)donatedt.Donate
                                          ).Sum() ?? 0m
                                      }).OrderByDescending(x => x.StudentYear).ThenByDescending(x => x.Term).ToListAsync();


            var result = outstandings.GroupBy(x => new { x.School_Year_Id, x.Code }).Select(g => new StudentDonationSummaryVM
            {

                Code = g.Key.Code,
                StudentName = g.First().StudentName,
                GradeLevelRoom = $"{g.First().Grade_Level}/{g.First().Room}",
                Paid = g.Sum(x => x.DonateAmount),
                Required = g.Sum(x => x.TotalAmount),
                Remaining = g.Sum(x => x.TotalAmount) - g.Sum(x => x.DonateAmount)
            });

            return result
                .OrderByDescending(x => x.Remaining) // เรียงลำดับคนค้างเยอะที่สุดขึ้นก่อน
                .ThenBy(x => x.Code)
                .ToList();
        }
    }
}