using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeManagement.Interface;
using BackOfficeLibrary;
using System.Globalization;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        private readonly IDashboardRepo _dashboard;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<DashboardController> logger,
            IDashboardRepo dashboard,
            IStringLocalizer<SharedResource> localizer,
            LanguageService lang
            )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _dashboard = dashboard;
            _localizer = localizer;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }

                var schoolYears = await _context.SchoolYearData
                    .OrderByDescending(x => x.Name)
                    .Select(x => new SchoolYearOption
                    {
                        Value = x.Id,
                        Text = x.Name
                    })
                    .ToListAsync();

                var vm = new DashboardViewModel
                {
                    SchoolYears = schoolYears,
                    SelectedSchoolYear = schoolYears.Any() ? schoolYears.First().Value : 0
                };


                ViewBag.gradeLevelsList = await _context.GradeLevelData
                .Where(g => g.IsActive == true)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }
        public async Task<IActionResult> AnnualSummary(int schoolYear)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                    return Unauthorized(new { message = "User cannot be found!" });

                var year = await _context.SchoolYearData
                    .FirstOrDefaultAsync(x => x.Id == schoolYear);

                if (year == null)
                    return NotFound(new { message = "Year cannot be found!" });

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
                                .Where(sh => sh.School_Year_Id == schoolYear)
                                .Join(
                                    _context.StudentData.Where(s => s.Status != 3),
                                    sh => sh.Student_Id,
                                    s => s.Id.ToString(),
                                    (sh, s) => new
                                    {
                                        sh.Student_Id,
                                        sh.Grade_Level_Id,
                                        sh.Term_Id,
                                        s.Code,
                                        s.FirstName,
                                        s.LastName,
                                        s.Status
                                    })
                                .ToListAsync();

                var donateSettingIndividual = await _context.DonateSettingIndividualData
                .Where(d => d.SchoolYear_Id == schoolYear && d.IsActive == true)
                .Select(d => new
                {
                    d.Student_Id,
                    d.GradeLevel_Id,
                    d.Term_Id,
                    d.OverrideAmount
                }).ToListAsync();

                var donateSettings = await _context.DonateSettingData
                    .Where(d => d.SchoolYear_Id == schoolYear && d.IsActive == true)
                    .Select(d => new
                    {
                        d.Grade_Level_Id,
                        d.Term_Id,
                        d.Donate
                    }).ToListAsync();

                decimal totalExpectedAmount = (
                    from sh in students

                    join ds in donateSettings
                        on new
                        {
                            GradeLevel_Id = sh.Grade_Level_Id,
                            Term_Id = sh.Term_Id
                        }
                        equals new
                        {
                            GradeLevel_Id = ds.Grade_Level_Id,
                            Term_Id = ds.Term_Id
                        }

                    join dsin in donateSettingIndividual
                        on new
                        {
                            Student_Id = sh.Student_Id,
                            GradeLevel_Id = sh.Grade_Level_Id,
                            Term_Id = sh.Term_Id
                        }
                        equals new
                        {
                            Student_Id = dsin.Student_Id,
                            GradeLevel_Id = dsin.GradeLevel_Id,
                            Term_Id = dsin.Term_Id
                        }
                        into dsinGroup

                    from dsin in dsinGroup.DefaultIfEmpty()

                    select dsin?.OverrideAmount ?? ds.Donate ?? 0m
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
                            on new
                            {
                                GradeLevel_Id = sh.Grade_Level_Id,
                                Term_Id = sh.Term_Id
                            }
                            equals new
                            {
                                GradeLevel_Id = ds.Grade_Level_Id,
                                Term_Id = ds.Term_Id
                            }

                        join dsin in donateSettingIndividual
                            on new
                            {
                                Student_Id = sh.Student_Id,
                                GradeLevel_Id = sh.Grade_Level_Id,
                                Term_Id = sh.Term_Id
                            }
                            equals new
                            {
                                Student_Id = dsin.Student_Id,
                                GradeLevel_Id = dsin.GradeLevel_Id,
                                Term_Id = dsin.Term_Id
                            }
                            into dsinGroup

                        from dsin in dsinGroup.DefaultIfEmpty()

                        select dsin?.OverrideAmount ?? ds.Donate ?? 0m
                    ).Sum();

                    if (donated >= expected)
                        totalStudentDonate++;
                    else
                        totalStudentNotDonate++;
                }

                return Ok(new
                {
                    Year = schoolYear,
                    SchoolYearName = year.Name,

                    TotalStudent = students.Select(x => x.Student_Id).Distinct().Count(),
                    TotalStudentDonate = totalStudentDonate,
                    TotalStudentNotDonate = totalStudentNotDonate,

                    TotalDonationYear = totalYearDonation,
                    Term1Donation = term1Donation,
                    Term2Donation = term2Donation,

                    TotalExpectedAmount = totalExpectedAmount,
                    TotalOutstandingAmount = totalOutstandingAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { message = "Something went wrong." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DonutDashboard(int schoolYear, int? gradeLevelId)
        {
            // =========================
            // 0) Term
            // =========================
            var termData = await _context.TermData
                .Where(t => t.SchoolYear_Id == schoolYear)
                .ToListAsync();

            var term1Id = termData.FirstOrDefault(x => x.Name.Trim() == "1")?.Id;
            var term2Id = termData.FirstOrDefault(x => x.Name.Trim() == "2")?.Id;

            // =========================
            // 1) Donate per student / term
            //    ❗ JOIN DonateDetail โดยไม่ใช้ Grade_Level_Id
            // =========================
            var donateList = await (from sh in _context.StudentHistoryClassData
                                    where sh.School_Year_Id == schoolYear
                                    && (gradeLevelId == null || sh.Grade_Level_Id == gradeLevelId)
                                    join student in _context.StudentData on sh.Student_Id equals student.Id.ToString()
                                    where student.Status != 3

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
                                    from dd in ddg.Where(x => x == null || x.Status == 1).DefaultIfEmpty()

                                    group dd by new
                                    {
                                        sh.Student_Id,
                                        sh.Term_Id,
                                        sh.Grade_Level_Id
                                    }
                                    into g
                                    select new
                                    {
                                        g.Key.Student_Id,
                                        g.Key.Term_Id,
                                        g.Key.Grade_Level_Id,
                                        TotalDonate = g.Sum(x => x != null ? x.Donate : 0)
                                    }).ToListAsync();

            // =========================
            // 2) Donate setting
            // =========================
            var settingIndividualList = await _context.DonateSettingIndividualData
                            .Where(x => x.SchoolYear_Id == schoolYear && x.IsActive == true
                                && (gradeLevelId == null || x.GradeLevel_Id == gradeLevelId))
                            .GroupBy(x => new
                            {
                                x.Student_Id,
                                x.Term_Id,
                                x.GradeLevel_Id
                            })
                            .Select(g => new
                            {
                                g.Key.Student_Id,
                                g.Key.Term_Id,
                                g.Key.GradeLevel_Id,
                                DonateSetting = g.Sum(x => x.OverrideAmount)
                            })
                            .ToListAsync();

            var settingList = await _context.DonateSettingData
                            .Where(x => x.SchoolYear_Id == schoolYear
                                && (gradeLevelId == null || x.Grade_Level_Id == gradeLevelId))
                            .GroupBy(x => new
                            {
                                x.Term_Id,
                                x.Grade_Level_Id
                            })
                            .Select(g => new
                            {
                                g.Key.Term_Id,
                                g.Key.Grade_Level_Id,
                                DonateSetting = g.Sum(x => x.Donate)
                            })
                            .ToListAsync();


            // =========================
            // 3) Join donate + setting (LEFT JOIN)
            // =========================
            var detail = from d in donateList

                             // join default setting
                         join s in settingList
                             on new
                             {
                                 Term_Id = d.Term_Id,
                                 GradeLevel_Id = d.Grade_Level_Id
                             }
                             equals new
                             {
                                 Term_Id = s.Term_Id,
                                 GradeLevel_Id = s.Grade_Level_Id
                             }
                             into sg

                         from s in sg.DefaultIfEmpty()

                             // join individual override (LEFT JOIN)
                         join sin in settingIndividualList
                             on new
                             {
                                 Student_Id = d.Student_Id,
                                 Term_Id = d.Term_Id,
                                 GradeLevel_Id = d.Grade_Level_Id
                             }
                             equals new
                             {
                                 Student_Id = sin.Student_Id,
                                 Term_Id = sin.Term_Id,
                                 GradeLevel_Id = sin.GradeLevel_Id
                             }
                             into sing

                         from sin in sing.DefaultIfEmpty()

                         let requiredAmount = sin?.DonateSetting ?? s?.DonateSetting ?? 0m

                         select new
                         {
                             d.Student_Id,
                             d.Term_Id,
                             d.Grade_Level_Id,
                             d.TotalDonate,
                             DonateSetting = requiredAmount,
                             DiffDonate = requiredAmount - d.TotalDonate
                         };


            // =========================
            // 4) Summary ต่อเทอม
            // =========================
            var summary = detail
                .GroupBy(x => x.Term_Id)
                .Select(g => new
                {
                    Term_Id = g.Key,

                    CountStudent = g
                        .Select(x => x.Student_Id)
                        .Distinct()
                        .Count(),

                    PaidCount = g
                        .Where(x => x.TotalDonate >= x.DonateSetting && x.DonateSetting > 0)
                        .Select(x => x.Student_Id)
                        .Distinct()
                        .Count(),

                    TotalDonate = g.Sum(x => x.TotalDonate),

                    ExpectedAmount = g.Sum(x => x.DonateSetting)
                })
                .ToList();


            var term1 = summary.FirstOrDefault(x => x.Term_Id == term1Id);
            var term2 = summary.FirstOrDefault(x => x.Term_Id == term2Id);

            // =========================
            // 5) JSON
            // =========================
            return Json(new
            {
                term1 = new
                {
                    totalStudents = term1?.CountStudent ?? 0,
                    paidStudents = term1?.PaidCount ?? 0,
                    unpaidStudents =
                        (term1?.CountStudent ?? 0) - (term1?.PaidCount ?? 0),
                    expectedAmount = term1?.ExpectedAmount ?? 0,
                    totalPaid = term1?.TotalDonate ?? 0
                },
                term2 = new
                {
                    totalStudents = term2?.CountStudent ?? 0,
                    paidStudents = term2?.PaidCount ?? 0,
                    unpaidStudents =
                        (term2?.CountStudent ?? 0) - (term2?.PaidCount ?? 0),
                    expectedAmount = term2?.ExpectedAmount ?? 0,
                    totalPaid = term2?.TotalDonate ?? 0
                }
            });
        }

        public async Task<IActionResult> TopDonate([FromBody] DataTableReq input)
        {
            // const int TOP_COUNT = 10;

            // 1) หา record ประวัติห้องเรียนล่าสุดต่อ Student (Self Join)
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
                    StudentName = p.Abbreviation + s.FirstName + " " + s.LastName,
                    TotalDonate = d.TotalDonate,
                    GradeLevel_Room = g.Name + "/" + r.Name
                };

            var result = await query.ToListAsync();
            if ((result == null) || (!result.Any()))
            {
                var dataNull = new
                {
                    draw = 1,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new object[] { }
                };
                return Json(dataNull);
            }

            var resultlist_tour = result;
            var _datalist = result;

            int recordsTotal = _datalist?.Count ?? 0;
            var dataTable = new
            {
                draw = 1,
                recordsTotal = recordsTotal,
                recordsFiltered = 20,
                data = _datalist
            };
            return Json(dataTable);
            // return PartialView("_TopDonate", result);
        }

        public async Task<IActionResult> TopNotDonate([FromBody] DataTableReq input)
        {
            // 1) หา history ล่าสุดแบบเดิม
            var latestHistoryQuery = from h in _context.StudentHistoryClassData
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
            var donateList = await (from sh in _context.StudentHistoryClassData
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
                                    }).ToListAsync();


            //----------------------------------------------------------------------
            // 3) ดึง DonateSetting ต่อ term
            //----------------------------------------------------------------------
            var settingIndividualList = await _context.DonateSettingIndividualData
                .Where(x => x.SchoolYear_Id == input.schoolYearId && x.IsActive == true)
                .Select(x => new
                {
                    x.Student_Id,
                    x.SchoolYear_Id,
                    x.GradeLevel_Id,
                    x.Term_Id,
                    x.OverrideAmount
                })
                .ToListAsync();

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
            var resultByTerm = from d in donateList

                               join s in settingList
                                   on new
                                   {
                                       SchoolYear_Id = d.School_Year_Id,
                                       GradeLevel_Id = d.Grade_Level_Id,
                                       Term_Id = d.Term_Id
                                   }
                                   equals new
                                   {
                                       SchoolYear_Id = s.SchoolYear_Id,
                                       GradeLevel_Id = s.Grade_Level_Id,
                                       Term_Id = s.Term_Id
                                   }

                               join sin in settingIndividualList
                                   on new
                                   {
                                       Student_Id = d.Student_Id,
                                       SchoolYear_Id = d.School_Year_Id,
                                       GradeLevel_Id = d.Grade_Level_Id,
                                       Term_Id = d.Term_Id
                                   }
                                   equals new
                                   {
                                       Student_Id = sin.Student_Id,
                                       SchoolYear_Id = sin.SchoolYear_Id,
                                       GradeLevel_Id = sin.GradeLevel_Id,
                                       Term_Id = sin.Term_Id
                                   }
                                   into sinGroup

                               from sin in sinGroup.DefaultIfEmpty()

                               let requiredAmount = sin?.OverrideAmount ?? s.DonateSetting ?? 0m

                               select new
                               {
                                   d.Student_Id,
                                   d.School_Year_Id,
                                   d.Grade_Level_Id,
                                   d.Term_Id,
                                   d.TotalDonate,
                                   DonateSetting = requiredAmount,
                                   DiffDonate = requiredAmount - d.TotalDonate
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
                .Where(s => studentIds.Contains(s.Id.ToString()) && s.Status != 3)
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
                    StudentId = stu.Id.ToString(),
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

            if ((result == null) || (!result.Any()))
            {
                var dataNull = new
                {
                    draw = 1,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new object[] { },
                    remaining = new object[] { }
                };
                return Json(dataNull);
            }

            var resultlist_tour = result;
            var _datalist = result;

            int recordsTotal = _datalist?.Count ?? 0;
            var dataTable = new
            {
                draw = 1,
                recordsTotal = recordsTotal,
                recordsFiltered = 20,
                data = _datalist,
                remaining = Remaining
            };
            return Json(dataTable);
        }

        [HttpPost]
        public async Task<IActionResult> ExportTopNotDonateToSheet([FromBody] ExportDonateReq input)
        {
            // 1. map SchoolYearId -> Year (2568)
            var schoolYearMap = await _context.SchoolYearData
                .Where(x => input.SchoolYearIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var payload = new List<object>();

            foreach (var schoolYearId in input.SchoolYearIds)
            {
                if (!schoolYearMap.TryGetValue(schoolYearId, out var year))
                    continue;
                var req = new DataTableReq
                {
                    schoolYearId = schoolYearId,
                    gradeLevelId = input.GradeLevelId,
                    FilterBy = null
                };
                var result = await _dashboard.GetTopNotDonateAsync(req);


                if (result == null || !result.Any())
                    continue;

                payload.Add(new
                {
                    schoolYear = year,
                    rows = result.Select(x => new
                    {
                        code = x.Code.ToString(),
                        studentName = x.StudentName,
                        gradeRoom = x.GradeLevel_Room,
                        paid = x.Paid,
                        required = x.Required,
                        remaining = x.Remaining
                    }).ToList()
                });
            }

            if (!payload.Any())
                return Ok(new { success = false, message = "No data to export" });

            var json = JsonConvert.SerializeObject(payload);

            using var client = new HttpClient();
            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            await client.PostAsync(
                "https://script.google.com/macros/s/AKfycbzTcro9b1D9ffruDsqrW24B64L5AzIaYfmlJbeIAySV7A6fSxxrKMRk7NOtX5PAaxWy/exec",
                content
            );

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ExportTopDonateToSheet([FromBody] ExportDonateReq input)
        {
            // 1. map SchoolYearId -> Year (2568)
            var schoolYearMap = await _context.SchoolYearData
                .Where(x => input.SchoolYearIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var payload = new List<object>();

            foreach (var schoolYearId in input.SchoolYearIds)
            {
                if (!schoolYearMap.TryGetValue(schoolYearId, out var year))
                    continue;
                var req = new DataTableReq
                {
                    schoolYearId = schoolYearId,
                    gradeLevelId = input.GradeLevelId,
                    FilterBy = null
                };
                var result = await _dashboard.GetTopDonateAsync(req);


                if (result == null || !result.Any())
                    continue;

                payload.Add(new
                {
                    schoolYear = year,
                    rows = result.Select(x => new
                    {
                        code = x.Code.ToString(),
                        studentName = x.StudentName,
                        gradeRoom = x.GradeLevel_Room,
                        totalDonate = x.TotalDonate,
                    }).ToList()
                });
            }

            if (!payload.Any())
                return Ok(new { success = false, message = "No data to export" });

            var json = JsonConvert.SerializeObject(payload);

            using var client = new HttpClient();
            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            await client.PostAsync(
                "https://script.google.com/macros/s/AKfycbzN7HKReOPvTouWpFi_0yAVoGzFsEzw0TPjfQSaSspzEP2V_0ZJkTBfCWlBJTET4_a5/exec",
                content
            );

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ExportToSheet([FromBody] ExportDonateReq input)
        {
            // 1. map SchoolYearId -> Year (2568)
            var schoolYearMap = await _context.SchoolYearData
                .Where(x => input.SchoolYearIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var payload = new List<object>();

            foreach (var schoolYearId in input.SchoolYearIds)
            {
                if (!schoolYearMap.TryGetValue(schoolYearId, out var year))
                    continue;
                var req = new DataTableReq
                {
                    schoolYearId = schoolYearId,
                    gradeLevelId = input.GradeLevelId,
                    FilterBy = null
                };
                var result = await _dashboard.GetStudentDonationSummaryAsync(req);


                if (result == null || !result.Any())
                    continue;

                payload.Add(new
                {
                    schoolYear = year,
                    rows = result.Select(x => new
                    {
                        code = x.Code.ToString(),
                        studentName = x.StudentName,
                        gradeRoom = x.GradeLevelRoom,
                        paid = x.Paid,
                        required = x.Required,
                        remaining = x.Remaining
                    }).ToList()
                });
            }

            if (!payload.Any())
                return Ok(new { success = false, message = "No data to export" });

            var json = JsonConvert.SerializeObject(payload);

            using var client = new HttpClient();
            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            await client.PostAsync(
                "https://script.google.com/macros/s/AKfycbyg2YxgX1DXjuobtYSDedCi6P_TVanWITOhRqk7UQDP83b-paDEORORPENAcY8s7V5e/exec",
                content
            );

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportDonateReq input)
        {
            // map SchoolYearId -> Year
            var schoolYearMap = await _context.SchoolYearData
                .Where(x => input.SchoolYearIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            bool hasData = false;

            foreach (var schoolYearId in input.SchoolYearIds)
            {
                if (!schoolYearMap.TryGetValue(schoolYearId, out var year))
                    continue;

                var req = new DataTableReq
                {
                    schoolYearId = schoolYearId,
                    gradeLevelId = input.GradeLevelId,
                    FilterBy = null
                };

                var result = await _dashboard.GetStudentDonationSummaryAsync(req);
                if (result == null || !result.Any())
                    continue;

                hasData = true;

                var ws = workbook.Worksheets.Add($"ปี {year}");

                // Header
                ws.Cell(1, 1).Value = "รหัสนักเรียน";
                ws.Cell(1, 2).Value = "ชื่อนักเรียน";
                ws.Cell(1, 3).Value = "ชั้น/ห้อง";
                ws.Cell(1, 4).Value = "ชำระแล้ว";
                ws.Cell(1, 5).Value = "ยอดที่ต้องชำระ";
                ws.Cell(1, 6).Value = "คงค้าง";

                ws.Range(1, 1, 1, 6).Style
                    .Font.SetBold()
                    .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);

                int row = 2;
                foreach (var x in result)
                {
                    ws.Cell(row, 1).Value = x.Code;
                    ws.Cell(row, 2).Value = x.StudentName;
                    ws.Cell(row, 3).Value = x.GradeLevelRoom;
                    ws.Cell(row, 4).Value = x.Paid;
                    ws.Cell(row, 5).Value = x.Required;
                    ws.Cell(row, 6).Value = x.Remaining;
                    row++;
                }

                ws.Columns().AdjustToContents();
                ws.Column(6).Style.Font.FontColor =
                    ClosedXML.Excel.XLColor.Red;
            }

            if (!hasData)
                return Ok(new { success = false, message = "No data to export" });

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var years = string.Join("_", schoolYearMap.Values);
            var fileName = $"Donation_Report_{years}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            Response.Headers["Content-Disposition"] =
                $"attachment; filename=\"{fileName}\"";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );

        }
        // ชื่อไฟล์แปลก Donation_Report_2568_25690205_160006.xlsx; filename_=UTF-8''Donation_Report_2568_25690205_160006.xlsx

        [HttpGet]
        public async Task<IActionResult> GetTermBySchoolYear(int schoolYearId)
        {
            var terms = await _context.TermData
                .Where(t => t.SchoolYear_Id == schoolYearId)
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    id = t.Id,
                    text = t.Name
                })
                .ToListAsync();

            return Json(terms);
        }
        int GetGradeOrder(string gradeName)
        {
            if (gradeName.StartsWith("อ."))
                return 0 * 100 + int.Parse(gradeName.Replace("อ.", ""));

            if (gradeName.StartsWith("ป."))
                return 1 * 100 + int.Parse(gradeName.Replace("ป.", ""));
            return 999;
        }


        [HttpGet]
        public async Task<IActionResult> GraphRoomDashboard(int schoolYear, int? termId)
        {
            if (schoolYear == 0)
                return Json(new List<object>());

            bool isYearMode = !termId.HasValue || termId == 0;

            // 1) Student history
            var histories = await (
                from h in _context.StudentHistoryClassData
                join student in _context.StudentData on h.Student_Id equals student.Id.ToString()
                where student.Status != 3
                join g in _context.GradeLevelData on h.Grade_Level_Id equals g.Id
                join r in _context.RoomData on h.Room_Id equals r.Id
                where h.School_Year_Id == schoolYear
                && (isYearMode || h.Term_Id == termId)
                select new
                {
                    h.Student_Id,
                    h.Grade_Level_Id,
                    h.Term_Id,
                    GradeName = g.Name,
                    RoomName = r.Name
                }
            )
            .GroupBy(x => x.Student_Id)
            .Select(g => g
                .First()
            )
            .ToListAsync();


            if (!histories.Any())
                return Json(new List<object>());

            var studentIds = histories.Select(x => x.Student_Id).Distinct().ToList();

            // 2) Donate (รวมตาม mode)
            var donateList = await (
                from d in _context.DonateData
                join dd in _context.DonateDetailsData on d.Id equals dd.Donate_Id
                where studentIds.Contains(d.Student_Id)
                      && dd.SchoolYear_Id == schoolYear
                      && dd.Status == 1
                      && (isYearMode || dd.Term_Id == termId)
                group dd by d.Student_Id into g
                select new
                {
                    StudentId = g.Key,
                    TotalDonate = g.Sum(x => x.Donate)
                }
            ).ToListAsync();

            // 3) DonateSetting (Required)
            var settingIndividualList = await _context.DonateSettingIndividualData
                            .Where(x => x.SchoolYear_Id == schoolYear && x.IsActive == true)
                            .Select(x => new
                            {
                                x.Student_Id,
                                x.SchoolYear_Id,
                                x.GradeLevel_Id,
                                x.Term_Id,
                                x.OverrideAmount
                            })
                            .ToListAsync();

            var settingList = await _context.DonateSettingData
                .Where(x =>
                    x.SchoolYear_Id == schoolYear &&
                    (isYearMode || x.Term_Id == termId)
                )
                .GroupBy(x => x.Grade_Level_Id)
                .Select(g => new
                {
                    GradeLevelId = g.Key,
                    Required = g.Sum(x => x.Donate)
                })
                .ToListAsync();

            // 4) คำนวณต่อ student
            var studentResult = from h in histories

                                join s in settingList
                                    on h.Grade_Level_Id equals s.GradeLevelId
                                    into sg
                                from s in sg.DefaultIfEmpty()

                                join sin in settingIndividualList
                                    on new
                                    {
                                        Student_Id = h.Student_Id,
                                        GradeLevel_Id = h.Grade_Level_Id,
                                        Term_Id = isYearMode ? (int?)null : h.Term_Id
                                    }
                                    equals new
                                    {
                                        Student_Id = sin.Student_Id,
                                        GradeLevel_Id = sin.GradeLevel_Id,
                                        Term_Id = isYearMode ? (int?)null : sin.Term_Id
                                    }
                                    into sing
                                from sin in sing.DefaultIfEmpty()

                                    // LEFT JOIN donate
                                join d in donateList
                                    on h.Student_Id equals d.StudentId
                                    into dg
                                from d in dg.DefaultIfEmpty()

                                let totalDonate = d?.TotalDonate ?? 0m

                                let requiredAmount =
                                    sin?.OverrideAmount
                                    ?? s?.Required
                                    ?? 0m

                                select new
                                {
                                    h.GradeName,
                                    h.RoomName,
                                    PaidAmount = totalDonate <= requiredAmount ? totalDonate : requiredAmount,
                                    UnpaidAmount = (requiredAmount - totalDonate) > 0m
                                        ? requiredAmount - totalDonate
                                        : 0m,
                                    IsPaid = totalDonate >= requiredAmount && requiredAmount > 0
                                };

            // 5) รวมเป็นรายห้อง
            var result = studentResult
                .GroupBy(x => new { x.GradeName, x.RoomName })
                .Select(g =>
                {
                    var totalStudents = g.Count();
                    var paidCount = g.Count(x => x.IsPaid);
                    return new
                    {
                        GradeName = g.Key.GradeName,
                        RoomName = g.Key.RoomName,
                        GradeOrder = GetGradeOrder(g.Key.GradeName),
                        RoomOrder = int.TryParse(g.Key.RoomName, out var r) ? r : 0,

                        PaidAmount = g.Sum(x => x.PaidAmount),
                        UnpaidAmount = g.Sum(x => x.UnpaidAmount),

                        TotalStudents = totalStudents,
                        PaidStudents = paidCount,
                        UnpaidStudents = totalStudents - paidCount,

                        PaidPercent = totalStudents == 0 ? 0 :
                            Math.Round((paidCount * 100m) / totalStudents, 2),

                        UnpaidPercent = totalStudents == 0 ? 0 :
                            Math.Round(((totalStudents - paidCount) * 100m) / totalStudents, 2)
                    };
                })
                .OrderBy(x => x.GradeOrder)
                .ThenBy(x => x.RoomOrder)
                .Select(x => new
                {
                    roomName = $"{x.GradeName}/{x.RoomName}",
                    paid = x.PaidAmount,
                    unpaid = x.UnpaidAmount,
                    totalStudents = x.TotalStudents,
                    paidStudents = x.PaidStudents,
                    unpaidStudents = x.UnpaidStudents,
                    paidPercent = x.PaidPercent,
                    unpaidPercent = x.UnpaidPercent
                }).ToList();

            return Json(result);
        }
    }
}