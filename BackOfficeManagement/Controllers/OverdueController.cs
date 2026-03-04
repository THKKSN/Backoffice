using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Globalization;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class OverdueController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OverdueController> _logger;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public OverdueController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<OverdueController> logger,

            IStringLocalizer<SharedResource> localizer,
            LanguageService lang
            )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

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
                await LoadDropdownsAsync();

                return View();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FollowUp Index Error.");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetOverdueList([FromBody] DataTableReq input)
        {
            try
            {
                if (!int.TryParse(input?.schoolyear, out var schoolYearId))
                    return BadRequest("Invalid school year");

                var keyword = input?.Search?.Trim() ?? string.Empty;

                // ==============================
                // 1) Grade ล่าสุดของ Student
                // ==============================
                var studentGradeMap = await _context.StudentHistoryClassData
                    .Where(x => x.School_Year_Id == schoolYearId)
                    .GroupBy(x => x.Student_Id)
                    .Select(g => new
                    {
                        g.Key,
                        GradeId = g
                            .OrderByDescending(x => x.IsCurrent)
                            .ThenByDescending(x => x.Term_Id)
                            .Select(x => (int?)x.Grade_Level_Id)
                            .FirstOrDefault()
                    })
                    .Where(x => x.GradeId.HasValue)
                    .ToDictionaryAsync(x => x.Key, x => x.GradeId!.Value);

                // ==============================
                // 2) ยอดที่ต้องจ่าย ต่อ Grade
                // ==============================
                var totalMap = await _context.DonateSettingData
                    .Where(x => x.SchoolYear_Id == schoolYearId && x.IsActive == true)
                    .GroupBy(x => x.Grade_Level_Id)
                    .Select(g => new
                    {
                        g.Key,
                        Total = g.Sum(x => (decimal?)x.Donate) ?? 0m
                    })
                    .ToDictionaryAsync(x => x.Key, x => x.Total);

                // ==============================
                // 3) ยอดที่จ่ายแล้ว ต่อ Student
                // ==============================
                var paidMap = await (
                    from d in _context.DonateData
                    join dt in _context.DonateDetailsData on d.Id equals dt.Donate_Id
                    where dt.SchoolYear_Id == schoolYearId && dt.Status == 1
                    group dt by d.Student_Id into g
                    select new
                    {
                        g.Key,
                        Paid = g.Sum(x => (decimal?)x.Donate) ?? 0m
                    }
                ).ToDictionaryAsync(x => x.Key, x => x.Paid);

                // ==============================
                // 4) Overdue List
                // ==============================
                var overdueRaw = await (
                    from o in _context.OverdueData
                    join st in _context.OverdueStatusData on o.Status equals st.Id
                    join h in _context.StudentHistoryClassData
                        on new { o.Student_Id, o.SchoolYear_Id }
                        equals new { h.Student_Id, SchoolYear_Id = h.School_Year_Id }
                    join sy in _context.SchoolYearData on h.School_Year_Id equals sy.Id
                    join g in _context.GradeLevelData on h.Grade_Level_Id equals g.Id
                    join r in _context.RoomData on h.Room_Id equals r.Id
                    join s in _context.StudentData on h.Student_Id equals s.Id.ToString()
                    join p in _context.PrefixData on s.Prefix_Id equals p.Id
                    where o.SchoolYear_Id == schoolYearId
                          && o.IsActive == true
                          && (
                                keyword == ""
                                || s.Code.Contains(keyword)
                                || (s.FirstName + " " + s.LastName).Contains(keyword)
                                || p.Abbreviation.Contains(keyword)
                             )
                    orderby o.OverDueDate
                    select new { o, st, sy, g, r, s, p }
                ).ToListAsync();

                var result = overdueRaw
                    .Select(x =>
                    {
                        var gradeId = studentGradeMap.GetValueOrDefault(x.o.Student_Id);
                        var total = totalMap.GetValueOrDefault(gradeId);
                        var paid = paidMap.GetValueOrDefault(x.o.Student_Id);

                        return new OverdueViewModel
                        {
                            Id = x.o.Id,
                            SchoolYear_Id = x.o.SchoolYear_Id,
                            SchoolYear = x.sy?.Name ?? "",

                            Student_Id = x.o.Student_Id,
                            Code = x.s?.Code ?? "",
                            StudentName = $"{x.p?.Abbreviation}{x.s?.FirstName} {x.s?.LastName}".Trim(),
                            GradeLevel = $"{x.g?.Name}/{x.r?.Name}",

                            OverDueDate = x.o.OverDueDate,
                            Description = x.o.Description ?? "",

                            TotalAmount = total,
                            PaidAmount = paid,
                            Balance = total - paid,

                            Status = x.o.Status,
                            StatusName = x.st?.Name ?? ""
                        };
                    })
                    .DistinctBy(x => x.Id)
                    .ToList();

                return Json(new
                {
                    draw = input?.Draw ?? 0,
                    recordsTotal = result.Count,
                    recordsFiltered = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOverdueList Error");
                return StatusCode(500);
            }
        }



        public async Task<IActionResult> Create()
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
                var model = new OverdueDTOModel();
                await LoadDropdownsAsync();
                return PartialView("_Create", model);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue Status List Index Error.");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsByGrade(int gradeId)
        {
            var students = await (
                from his in _context.StudentHistoryClassData
                join st in _context.StudentData on his.Student_Id equals st.Id.ToString()
                join pre in _context.PrefixData on st.Prefix_Id equals pre.Id
                where his.Grade_Level_Id == gradeId && his.IsCurrent == true
                orderby st.FirstName
                select new
                {
                    value = st.Id,
                    text = pre.Abbreviation + st.FirstName + " " + st.LastName
                }
            )
            .Distinct()
            .ToListAsync();

            return Json(students);
        }

        [HttpGet]
        public async Task<IActionResult> GetSchoolYearByUUID(string uuid)
        {
            var schoolYear = await (
                from history in _context.StudentHistoryClassData
                join sy in _context.SchoolYearData
                    on history.School_Year_Id equals sy.Id
                where history.Student_Id == uuid
                group sy by new { sy.Id, sy.Name } into g
                orderby g.Key.Id
                select new
                {
                    value = g.Key.Id,
                    text = g.Key.Name
                }
            ).ToListAsync();

            return Json(schoolYear);
        }

        [HttpGet]
        public async Task<IActionResult> GetOverdueDatatable(int schoolYearId, string studentId)
        {
            var gradeId = await _context.StudentHistoryClassData
            .Where(x => x.Student_Id == studentId
                    && x.School_Year_Id == schoolYearId)
            .OrderByDescending(x => x.Term_Id)
            .Select(x => x.Grade_Level_Id)
            .FirstOrDefaultAsync();

            // ยอดที่ต้องจ่ายทั้งปี
            var totalYearAmount = await _context.DonateSettingData
            .Where(s =>
                s.SchoolYear_Id == schoolYearId &&
                s.Grade_Level_Id == gradeId &&
                s.IsActive == true
            )
            .SumAsync(s => (decimal?)s.Donate) ?? 0m;

            // ยอดที่จ่ายแล้วทั้งปี
            var paidYearAmount = await (from donate in _context.DonateData
                                        join detail in _context.DonateDetailsData
                                            on donate.Id equals detail.Donate_Id
                                        where donate.Student_Id == studentId
                                              && detail.SchoolYear_Id == schoolYearId
                                              && detail.Status == 1
                                        select (decimal?)detail.Donate
                                        ).SumAsync() ?? 0m;


            var result = new[]
            {
                new
                {
                    term = "ทั้งปีการศึกษา",
                    paid = paidYearAmount, // ยอดที่บริจาค
                    total = totalYearAmount, //คงค้าง
                    balance = totalYearAmount - paidYearAmount
                }
            };

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OverdueCreate(OverdueDTOModel model)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user == null)
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var overdue = new OverdueDataModel
                {
                    Student_Id = model.Student_Id,
                    SchoolYear_Id = model.SchoolYear_Id,

                    Status = model.Status,
                    OverDueDate = model.OverDueDate,
                    Description = model.Description,

                    IsActive = true,
                    CreateBy = user.Id,
                    CreateDate = DateTime.Now
                };

                _context.OverdueData.Add(overdue);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue Create Error");
                return StatusCode(500, "Internal Server Error");
            }
        }


        private async Task LoadDropdownsAsync()
        {
            this.currentLang = this._lang.CurrentLang(HttpContext.Request);

            var prefix = await _context.PrefixData.ToListAsync();
            var disabilitytype = await _context.DisabilityTypeData.ToListAsync();
            var religion = await _context.ReligionData.ToListAsync();
            var provinces = await _context.ProvincesData.ToListAsync();
            var grandLevel = await _context.GradeLevelData.ToListAsync();
            var relationship = await _context.RelationshipData.ToListAsync();
            var parentStatus = await _context.ParentStatusData.ToListAsync();
            var schoolYear = await _context.SchoolYearData.OrderByDescending(_ => _.Name).ToListAsync();
            var student = await _context.StudentData.ToListAsync();
            var teacher = await _context.TeacherData.ToListAsync();
            var status = await _context.OverdueStatusData.ToListAsync();
            // ส่วนนี้เหมือนเดิม …
            ViewBag.PrefixList = prefix
                .Where(x => x.Abbreviation == "ด.ช." || x.Abbreviation == "ด.ญ.")
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Abbreviation,
                }).ToList();

            ViewBag.DisabilityTypeList = disabilitytype
                .Where(x => x.IsActive == true)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisabilityTypeName,
                }).ToList();

            ViewBag.ParentPrefixList = prefix
                .Where(x => x.Abbreviation != "ด.ช." && x.Abbreviation != "ด.ญ.")
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                }).ToList();

            ViewBag.ReligionList = religion.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
            }).ToList();

            ViewBag.ProvincesList = provinces.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
            }).ToList();

            ViewBag.GrandLevelList = grandLevel.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();

            ViewBag.RelationshipList = relationship.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name
            }).ToList();

            ViewBag.ParentStatusList = parentStatus.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name
            }).ToList();

            ViewBag.SchoolYearList = schoolYear.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();

            ViewBag.StudentList = student.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.FirstName + " " + x.LastName
            }).ToList();

            ViewBag.TeacherList = teacher.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.FirstName + " " + x.LastName
            }).ToList();

            ViewBag.StatusList = status.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
            }).ToList();

            ViewBag.TermList = new List<SelectListItem>();
        }

        public async Task<IActionResult> Update(int id)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return View("NotFound");

                var overdue = await _context.OverdueData
                    .FirstOrDefaultAsync(x => x.Id == id && x.IsActive == true);

                if (overdue == null)
                    return NotFound();

                var studentHistory = await _context.StudentHistoryClassData
                    .Where(x => x.Student_Id == overdue.Student_Id && x.IsCurrent == true)
                    .FirstOrDefaultAsync();

                var model = new OverdueDTOModel
                {
                    Id = overdue.Id,
                    Student_Id = overdue.Student_Id,
                    Description = overdue.Description,
                    OverDueDate = overdue.OverDueDate,
                    Status = overdue.Status,
                    GradeLevel_Id = studentHistory.Grade_Level_Id,
                    SchoolYear_Id = studentHistory.School_Year_Id // 🔥 สำคัญมาก
                };

                await LoadDropdownsAsync();

                return PartialView("_Update", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue Update Error");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(OverdueDTOModel model)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return View("NotFound");

                var overdue = await _context.OverdueData
                    .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsActive == true);

                if (overdue == null)
                    return NotFound();

                overdue.Id = model.Id;
                overdue.Student_Id = model.Student_Id;
                overdue.SchoolYear_Id = model.SchoolYear_Id;
                overdue.Status = model.Status;
                overdue.OverDueDate = model.OverDueDate;
                overdue.Description = model.Description;


                overdue.UpdateBy = user.Id;
                overdue.UpdateDate = DateTime.Now;

                _context.Update(overdue);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue Update Error");
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user == null)
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var overdue = await _context.OverdueData.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (overdue == null) return NotFound();

                overdue.IsActive = false;
                overdue.UpdateBy = user.Id;
                overdue.UpdateDate = DateTime.Now;

                _context.OverdueData.Update(overdue);
                await _context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue Create Error");
                return StatusCode(500, "Internal Server Error");
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                currentLang = _lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user == null)
                    return View("NotFound");

                // ==============================
                // 1) ข้อมูล Overdue + Student
                // ==============================
                var result = await (
                    from o in _context.OverdueData
                    join st in _context.OverdueStatusData on o.Status equals st.Id
                    join sy in _context.SchoolYearData on o.SchoolYear_Id equals sy.Id
                    join h in _context.StudentHistoryClassData
                        on new { o.Student_Id, o.SchoolYear_Id }
                        equals new { h.Student_Id, SchoolYear_Id = h.School_Year_Id }
                    join g in _context.GradeLevelData on h.Grade_Level_Id equals g.Id
                    join r in _context.RoomData on h.Room_Id equals r.Id
                    join s in _context.StudentData on h.Student_Id equals s.Id.ToString()
                    join p in _context.PrefixData on s.Prefix_Id equals p.Id
                    where o.Id == id
                    select new
                    {
                        o,
                        StatusName = st.Name,
                        SchoolYearName = sy.Name,
                        GradeId = h.Grade_Level_Id,
                        Classroom = g.Name + "/" + r.Name,
                        StudentName = p.Abbreviation + s.FirstName + " " + s.LastName,
                        s.Code
                    }
                ).FirstOrDefaultAsync();

                if (result == null)
                    return View("NotFound");

                // ==============================
                // 2) Total (ตาม Grade)
                // ==============================
                var totalAmount = await _context.DonateSettingData
                    .Where(x =>
                        x.SchoolYear_Id == result.o.SchoolYear_Id &&
                        x.Grade_Level_Id == result.GradeId &&
                        x.IsActive == true
                    )
                    .SumAsync(x => (decimal?)x.Donate) ?? 0m;

                // ==============================
                // 3) Paid (ตาม Student)
                // ==============================
                var paidAmount = await (
                    from d in _context.DonateData
                    join dt in _context.DonateDetailsData on d.Id equals dt.Donate_Id
                    where d.Student_Id == result.o.Student_Id
                          && dt.SchoolYear_Id == result.o.SchoolYear_Id
                          && dt.Status == 1
                    select (decimal?)dt.Donate
                ).SumAsync() ?? 0m;

                // ==============================
                // 4) ViewModel
                // ==============================

                string overdueDateText = "-";

                if (DateTime.TryParseExact(
                        result.o.OverDueDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var datetime))
                {
                    overdueDateText = datetime.ToString("dd MMM yyyy",
                        new CultureInfo("th-TH"));
                }
                var model = new OverdueViewModel
                {
                    Id = result.o.Id,
                    Description = result.o.Description ?? "",
                    StatusName = result.StatusName ?? "",
                    SchoolYear = result.SchoolYearName ?? "",
                    GradeLevel = result.Classroom ?? "",
                    StudentName = result.StudentName ?? "",
                    Code = result.Code ?? "",
                    OverDueDate = overdueDateText,

                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    Balance = totalAmount - paidAmount
                };

                return PartialView("_Detail", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Overdue Detail Error");
                return StatusCode(500);
            }
        }

    }

}