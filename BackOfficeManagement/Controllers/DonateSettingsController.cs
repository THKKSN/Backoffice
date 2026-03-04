using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class DonateSettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonateSettingsController> _logger;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public DonateSettingsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<DonateSettingsController> logger,

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
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user == null)
                    return View("NotFound");

                await LoadDropdownsAsync();

                ViewBag.defaultDataCount = await _context.DonateSettingData
                    .CountAsync(x => x.IsActive == true);

                ViewBag.individualDataCount = await _context.DonateSettingIndividualData
                    .CountAsync(x => x.IsActive == true);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDonateSettingsTable([FromBody] DonateSettingsFilterModel request)
        {
            int? schoolYearFilter = null;

            if (!string.IsNullOrEmpty(request.SchoolYear) && int.TryParse(request.SchoolYear, out int sy))
            {
                schoolYearFilter = sy;
            }

            var query =
                from d in _context.DonateSettingData
                join syear in _context.SchoolYearData on d.SchoolYear_Id equals syear.Id into syGroup
                from syr in syGroup.DefaultIfEmpty()
                join t in _context.TermData on d.Term_Id equals t.Id into tGroup
                from term in tGroup.DefaultIfEmpty()
                join g in _context.GradeLevelData on d.Grade_Level_Id equals g.Id into gGroup
                from grade in gGroup.DefaultIfEmpty()
                select new
                {
                    id = d.Id,
                    donate = d.Donate,
                    schoolYearId = d.SchoolYear_Id,
                    schoolYear = syr != null ? syr.Name : "",
                    termId = d.Term_Id,
                    term = term != null ? term.Name : "",
                    gradeLevelId = d.Grade_Level_Id,
                    gradeLevel = grade != null ? grade.Name : ""
                };

            // ----- Apply Filter -----
            if (schoolYearFilter.HasValue)
                query = query.Where(x => x.schoolYearId == schoolYearFilter.Value);

            var result = await query.ToListAsync();

            return Json(new { data = result });
        }

        [HttpPost]
        public async Task<IActionResult> GetDonateSettingIndividualTable([FromBody] DonateSettingsFilterModel request)
        {
            int? schoolYearFilter = null;

            if (!string.IsNullOrEmpty(request.SchoolYear) && int.TryParse(request.SchoolYear, out int sy))
            {
                schoolYearFilter = sy;
            }

            var query =
                from d in _context.DonateSettingIndividualData
                join student in _context.StudentData on d.Student_Id equals student.Id.ToString()
                join prefix in _context.PrefixData on student.Prefix_Id equals prefix.Id
                join syear in _context.SchoolYearData on d.SchoolYear_Id equals syear.Id into syGroup
                from syr in syGroup.DefaultIfEmpty()
                join t in _context.TermData on d.Term_Id equals t.Id into tGroup
                from term in tGroup.DefaultIfEmpty()
                join g in _context.GradeLevelData on d.GradeLevel_Id equals g.Id into gGroup
                from grade in gGroup.DefaultIfEmpty()

                where d.IsActive == true
                select new
                {
                    id = d.Id,
                    studentName = $"{prefix.Abbreviation}{student.FirstName} {student.LastName}",
                    overrideAmount = d.OverrideAmount,
                    schoolYearId = d.SchoolYear_Id,
                    schoolYear = syr != null ? syr.Name : "",
                    termId = d.Term_Id,
                    term = term != null ? term.Name : "",
                    gradeLevelId = d.GradeLevel_Id,
                    gradeLevel = grade != null ? grade.Name : "",
                    remark = d.Remark
                };

            // ----- Apply Filter -----
            if (schoolYearFilter.HasValue)
                query = query.Where(x => x.schoolYearId == schoolYearFilter.Value);

            var result = await query.ToListAsync();

            return Json(new { data = result });
        }

        public async Task<IActionResult> CreateIndividual()
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }
                await LoadDropdownsAsync();

                var model = new DonateIndividualViewModel();
                return PartialView("_CreateIndividual", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }
                await LoadDropdownsAsync();

                var model = new DonateSettingViewwModel();
                return PartialView("_Create", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        private async Task LoadDropdownsAsync()
        {
            this.currentLang = this._lang.CurrentLang(HttpContext.Request);

            var schoolYear = await _context.SchoolYearData.ToListAsync();
            var grandLevel = await _context.GradeLevelData.ToListAsync();
            var term = await _context.TermData.ToListAsync();
            var studentData = await (from student in _context.StudentData
                                     join prefix in _context.PrefixData
                                         on student.Prefix_Id equals prefix.Id
                                     select new StudantViewModel
                                     {
                                         Id = student.Id.ToString(),
                                         IsActive = student.IsActive,
                                         Status = student.Status,
                                         Student_Name = $"{prefix.Name}{student.FirstName} {student.LastName}"
                                     }
            ).ToListAsync();

            ViewBag.SchoolYearList = schoolYear.Where(x => x.IsActive == true && x.Status == 1).OrderByDescending(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();

            ViewBag.TermList = new List<SelectListItem>();

            ViewBag.StudentList = studentData.Where(x => x.IsActive == true && (x.Status != 3 && x.Status != 4))
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Student_Name
            }
            ).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetTerms(int schoolYearId)
        {
            // ดึง DonateSetting ของปีนั้น
            var existingDonateSettings = await _context.DonateSettingData
                .Where(x => x.SchoolYear_Id == schoolYearId)
                .ToListAsync();

            // Term ที่ยังไม่มีใน DonateSetting
            var allTerms = await _context.TermData
                .Where(x => x.SchoolYear_Id == schoolYearId && x.IsActive == true).Select(x => new
                {
                    value = x.Id,
                    text = x.Name
                })
                .ToListAsync();


            return Json(new { status = 200, termsList = allTerms });
        }

        [HttpGet]
        public async Task<IActionResult> GetGradeLevelForDonate(int schoolYearId, int termId)
        {
            var existingSettings = await _context.DonateSettingData
                .Where(d => d.SchoolYear_Id == schoolYearId && d.Term_Id == termId)
                .ToListAsync();

            var allGrades = await _context.GradeLevelData.ToListAsync();

            var availableGrades = allGrades
                .Where(g => !existingSettings.Any(d => d.Grade_Level_Id == g.Id))
                .Select(x => new
                {
                    value = x.Id,
                    text = x.Name
                }).ToList();

            return Json(new { status = 200, gradeList = availableGrades });
        }

        [HttpGet]
        public async Task<IActionResult> GetSchoolYearIndividual(string student_Id)
        {
            // ดึง DonateSetting ของปีนั้น
            var existingDonateSettings = await _context.DonateSettingIndividualData
                .Where(x => x.Student_Id == student_Id)
                .ToListAsync();

            var schoolYear = await _context.SchoolYearData
                .Where(x => x.IsActive == true).Select(x => new
                {
                    value = x.Id,
                    text = x.Name
                })
                .ToListAsync();


            return Json(new { status = 200, termsList = schoolYear });
        }

        [HttpGet]
        public async Task<IActionResult> GetTermIndividual(int schoolYearId)
        {
            // ดึง DonateSetting ของปีนั้น
            var existingDonateSettings = await _context.DonateSettingIndividualData
                .Where(x => x.SchoolYear_Id == schoolYearId)
                .ToListAsync();

            // Term ที่ยังไม่มีใน DonateSetting
            var allTerms = await _context.TermData
                .Where(x => x.SchoolYear_Id == schoolYearId && x.IsActive == true).Select(x => new
                {
                    value = x.Id,
                    text = x.Name
                })
                .ToListAsync();


            return Json(new { status = 200, termsList = allTerms });
        }

        [HttpGet]
        public async Task<IActionResult> GetGradeLevelIndividual(int schoolYearId, int termId, string studentId)
        {
            var studentHistory = await _context.StudentHistoryClassData
                .Where(sh => sh.Student_Id == studentId
                          && sh.School_Year_Id == schoolYearId
                          && sh.Term_Id == termId)
                .ToListAsync();

            if (!studentHistory.Any())
            {
                return Json(new { status = 404, message = "No student history found" });
            }

            var existingSettings = await _context.DonateSettingIndividualData
                .Where(d => d.SchoolYear_Id == schoolYearId
                         && d.Term_Id == termId
                         && d.Student_Id == studentId)
                .Select(d => d.GradeLevel_Id)
                .ToListAsync();

            var gradeIds = studentHistory
                .Select(x => x.Grade_Level_Id)
                .Distinct()
                .Where(id => !existingSettings.Contains(id))
                .ToList();

            var gradeList = await _context.GradeLevelData
                .Where(g => gradeIds.Contains(g.Id))
                .Select(g => new
                {
                    value = g.Id,
                    text = g.Name
                })
                .ToListAsync();

            return Json(new { status = 200, gradeList });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonateSettingViewwModel model)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }

                var newDonateSetting = new DonateSettingDataModel
                {
                    Donate = model.Donate,
                    SchoolYear_Id = model.SchoolYear_Id,
                    Term_Id = model.Term_Id,
                    Grade_Level_Id = model.Grade_Level_Id,
                    IsActive = true,
                    CreateBy = user.Id,
                    CreateDate = DateTime.Now
                };

                _context.DonateSettingData.Add(newDonateSetting);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIndividual(DonateIndividualViewModel model)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }

                var newDonateSettingIndividual = new DonateSettingIndividualDataModel
                {
                    OverrideAmount = model.OverrideAmount,
                    SchoolYear_Id = model.SchoolYear_Id,
                    Student_Id = model.Student_Id ?? "null",
                    Term_Id = model.Term_Id,
                    GradeLevel_Id = model.GradeLevel_Id,
                    Remark = model.Remark,
                    IsActive = true,
                    CreateBy = user.Id,
                    CreateDate = DateTime.Now
                };

                _context.DonateSettingIndividualData.Add(newDonateSettingIndividual);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        public async Task<IActionResult> UpdateIndividual(int id)
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

                var DonateSettingIndividual = await _context.DonateSettingIndividualData
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (DonateSettingIndividual == null)
                    return View("NotFound");

                // ⭐ โหลดดรอปดาวน์แบบมีค่าเดิม
                await LoadDropdownsUpdateAsync(
                    schoolYearId: DonateSettingIndividual.SchoolYear_Id,
                    termId: DonateSettingIndividual.Term_Id,
                    studentId: DonateSettingIndividual.Student_Id,
                    isIndividual: true
                );



                var model = new DonateIndividualViewModel
                {
                    Id = id,
                    SchoolYear_Id = DonateSettingIndividual.SchoolYear_Id,
                    Student_Id = DonateSettingIndividual.Student_Id,
                    Term_Id = DonateSettingIndividual.Term_Id,
                    GradeLevel_Id = DonateSettingIndividual.GradeLevel_Id,
                    OverrideAmount = DonateSettingIndividual.OverrideAmount,
                    Remark = DonateSettingIndividual.Remark
                };

                return PartialView("_UpdateIndividual", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }


        public async Task<IActionResult> Update(int id)
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

                var DonateSetting = await _context.DonateSettingData
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (DonateSetting == null)
                    return View("NotFound");

                // ⭐ โหลดดรอปดาวน์แบบมีค่าเดิม
                await LoadDropdownsUpdateAsync(
                    schoolYearId: DonateSetting.SchoolYear_Id,
                    termId: DonateSetting.Term_Id,
                    donateSettingId: DonateSetting.Id,
                    isIndividual: false
                );

                var model = new DonateSettingViewwModel
                {
                    Id = id,
                    SchoolYear_Id = DonateSetting.SchoolYear_Id,
                    Term_Id = DonateSetting.Term_Id,
                    Grade_Level_Id = DonateSetting.Grade_Level_Id,
                    Donate = DonateSetting.Donate,
                };

                return PartialView("_Update", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        private async Task LoadDropdownsUpdateAsync(int? schoolYearId = null, int? termId = null, int? donateSettingId = null, string studentId = null, bool isIndividual = false)
        {
            this.currentLang = this._lang.CurrentLang(HttpContext.Request);

            // ---------------- SchoolYear ----------------
            ViewBag.SchoolYearList = await _context.SchoolYearData
                .Where(x => x.IsActive == true && x.Status == 1)
                .OrderByDescending(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToListAsync();

            // ---------------- Student ----------------
            if (isIndividual)
            {
                ViewBag.StudentList = await (
                    from student in _context.StudentData
                    join prefix in _context.PrefixData
                        on student.Prefix_Id equals prefix.Id
                    where student.IsActive == true && (student.Status != 3 && student.Status != 4)
                    select new SelectListItem
                    {
                        Value = student.Id.ToString(),
                        Text = prefix.Name + student.FirstName + " " + student.LastName
                    }).ToListAsync();
            }

            // ---------------- Term ----------------
            if (schoolYearId.HasValue)
            {
                ViewBag.TermList = await _context.TermData
                    .Where(x => x.SchoolYear_Id == schoolYearId && x.IsActive == true)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    }).ToListAsync();
            }
            else
            {
                ViewBag.TermList = new List<SelectListItem>();
            }

            // ---------------- Grade ----------------
            if (schoolYearId.HasValue && termId.HasValue)
            {
                if (isIndividual && !string.IsNullOrEmpty(studentId))
                {
                    ViewBag.GradeList = await (
                        from sh in _context.StudentHistoryClassData
                        join g in _context.GradeLevelData
                            on sh.Grade_Level_Id equals g.Id
                        where sh.Student_Id == studentId
                           && sh.School_Year_Id == schoolYearId
                           && sh.Term_Id == termId
                        select new SelectListItem
                        {
                            Value = g.Id.ToString(),
                            Text = g.Name
                        }).Distinct().ToListAsync();
                }
                else
                {
                    var existingGradeIds = await _context.DonateSettingData
                        .Where(x => x.SchoolYear_Id == schoolYearId && x.Term_Id == termId &&
                                    (donateSettingId == null || x.Id != donateSettingId))
                        .Select(x => x.Grade_Level_Id).ToListAsync();

                    ViewBag.GradeList = await _context.GradeLevelData
                        .Where(g => !existingGradeIds.Contains(g.Id))
                        .Select(g => new SelectListItem
                        {
                            Value = g.Id.ToString(),
                            Text = g.Name
                        }).ToListAsync();

                }
            }
            else
            {
                ViewBag.GradeList = new List<SelectListItem>();
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DonateSettingViewwModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null) return View("NotFound");

                var entity = await _context.DonateSettingData.FirstOrDefaultAsync(d => d.Id == model.Id);
                if (entity == null) return View("NotFound");

                // CHECK DUPLICATE EXCEPT CURRENT
                bool exists = await _context.DonateSettingData.AnyAsync(d =>
                    d.Id != model.Id &&
                    d.SchoolYear_Id == model.SchoolYear_Id &&
                    d.Term_Id == model.Term_Id &&
                    d.Grade_Level_Id == model.Grade_Level_Id
                );

                if (exists)
                {
                    ModelState.AddModelError("", "ข้อมูลนี้มีอยู่ในระบบแล้ว");
                    await LoadDropdownsUpdateAsync(
                        schoolYearId: model.SchoolYear_Id,
                        termId: model.Term_Id,
                        donateSettingId: model.Id,
                        isIndividual: false
                    );
                    return View(model);
                }

                // UPDATE
                entity.SchoolYear_Id = model.SchoolYear_Id;
                entity.Term_Id = model.Term_Id;
                entity.Grade_Level_Id = model.Grade_Level_Id;
                entity.Donate = model.Donate;
                entity.UpdateBy = user.Id;
                entity.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateIndividual(DonateIndividualViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null) return View("NotFound");

                var entity = await _context.DonateSettingIndividualData.FirstOrDefaultAsync(d => d.Id == model.Id);
                if (entity == null) return View("NotFound");

                // CHECK DUPLICATE EXCEPT CURRENT
                bool exists = await _context.DonateSettingIndividualData.AnyAsync(d =>
                    d.Id != model.Id &&
                    d.Student_Id == model.Student_Id &&
                    d.SchoolYear_Id == model.SchoolYear_Id &&
                    d.Term_Id == model.Term_Id &&
                    d.GradeLevel_Id == model.GradeLevel_Id
                );

                if (exists)
                {
                    ModelState.AddModelError("", "ข้อมูลนี้มีอยู่ในระบบแล้ว");
                    await LoadDropdownsUpdateAsync(
                        schoolYearId: model.SchoolYear_Id,
                        termId: model.Term_Id,
                        donateSettingId: model.Id,
                        studentId: model.Student_Id,
                        isIndividual: true
                    );
                    return View(model);
                }

                // UPDATE
                entity.Student_Id = model.Student_Id;
                entity.SchoolYear_Id = model.SchoolYear_Id;
                entity.Term_Id = model.Term_Id;
                entity.GradeLevel_Id = model.GradeLevel_Id;
                entity.OverrideAmount = model.OverrideAmount;
                entity.Remark = model.Remark;
                entity.UpdateBy = user.Id;
                entity.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }
        // public async Task<IActionResult> DeleteIndividual(int id)
        // {
        //     try
        //     {
        //         var user = await _userManager.GetUserAsync(HttpContext.User);
        //         if (user == null) return NotFound();

        //         var entity = await _context.DonateSettingIndividualData
        //             .AsNoTracking()
        //             .FirstOrDefaultAsync(d => d.Id == id && d.IsActive == true);

        //         if (entity == null) return NotFound();

        //         var model = new DonateIndividualViewModel
        //         {
        //             Id = entity.Id,
        //             SchoolYear_Id = entity.SchoolYear_Id,
        //             Student_Id = entity.Student_Id,
        //             Term_Id = entity.Term_Id,
        //             GradeLevel_Id = entity.GradeLevel_Id,
        //             OverrideAmount = entity.OverrideAmount,
        //             Remark = entity.Remark
        //         };

        //         return View(model);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, ex.Message);
        //         return StatusCode(500);
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> DeleteIndividual(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null) return NotFound();

                var entity = await _context.DonateSettingIndividualData
                    .FirstOrDefaultAsync(d => d.Id == id && d.IsActive == true);

                if (entity == null) return NotFound();

                entity.IsActive = false;
                entity.UpdateBy = user.Id;
                entity.UpdateDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }
    }
}
