using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Interface;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Globalization;
using System.Text;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class DonateController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonateController> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");
        private readonly ILineMessageRepository _lineMessage;

        public DonateController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<DonateController> logger,

            IStringLocalizer<SharedResource> localizer,
            LanguageService lang,
            ILineMessageRepository imessage
            )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

            _localizer = localizer;
            _lang = lang;
            _lineMessage = imessage;
        }

        public async Task<IActionResult> Index(int? SearchSchoolYear)
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

                var schoolYear = await _context.SchoolYearData
                    .Where(x => x.IsActive == true)
                    .OrderByDescending(x => x.Name)
                    .ToListAsync();

                var filterSchoolYear = schoolYear.FirstOrDefault().Id;

                // Year dropdown
                ViewBag.SchoolYearList = schoolYear.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                });

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }


        public async Task<IActionResult> GetDonateListDataTable([FromBody] DataTableReq input)
        {
            var keyword = input.Search?.Trim();

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null || user.Id is null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            if (!Int32.TryParse(input.schoolyear, out int schoolyear))
            {
                return BadRequest("Invalid school year");
            }

            var result = await (from donate in _context.DonateData
                                join donatedt in _context.DonateDetailsData on donate.Id equals donatedt.Donate_Id
                                join student in _context.StudentData on donate.Student_Id equals student.Id.ToString()
                                join prefix in _context.PrefixData on student.Prefix_Id equals prefix.Id
                                join school in _context.SchoolYearData on donatedt.SchoolYear_Id equals school.Id
                                join term in _context.TermData on donatedt.Term_Id equals term.Id
                                join gradelevel in _context.GradeLevelData on donatedt.GradeLevel_Id equals gradelevel.Id
                                join channel in _context.DonateChannelData on donatedt.Channel_Id equals channel.Id
                                where donatedt.SchoolYear_Id == schoolyear && donatedt.Status == 1
                                && (
                                string.IsNullOrEmpty(keyword)
                                || student.Code.Contains(keyword)
                                || (student.FirstName + " " + student.LastName).Contains(keyword)
                                || student.FirstName.Contains(keyword)
                                || student.LastName.Contains(keyword)
                                || prefix.Abbreviation.Contains(keyword)
                                )
                                select new DonateViweDataTableModel
                                {
                                    Id = donate.Id,
                                    DonateDate = donatedt.DonateDate ?? DateTime.MinValue,
                                    UUID = student.Id.ToString(),
                                    Code = student.Code,
                                    FirstName = student.FirstName,
                                    LastName = student.LastName,
                                    FullName = $"{prefix.Abbreviation}{student.FirstName} {student.LastName}",
                                    GradeLevel = gradelevel.Name,
                                    Term = term.Name,
                                    StudyYear = school.Name,
                                    DonateChannel = channel.Donate_channel,
                                    Amount = donatedt.Donate ?? 0m
                                }).ToListAsync();

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

            _datalist = _datalist.OrderBy(x => x.DonateDate).ToList();


            int recordsTotal = _datalist?.Count ?? 0;
            var dataTable = new
            {
                draw = 1,
                recordsTotal = recordsTotal,
                recordsFiltered = 20,
                data = _datalist
            };
            return Json(dataTable);
        }

        [Route("Donate/Detail/{uuid}")]
        [HttpGet]
        public async Task<IActionResult> Detail(string uuid)
        {
            try
            {
                var info_student = await (from student in _context.StudentData
                                          join studenthistory in _context.StudentHistoryClassData on student.Id.ToString() equals studenthistory.Student_Id
                                          join gradelevel in _context.GradeLevelData on studenthistory.Grade_Level_Id equals gradelevel.Id
                                          join prefix in _context.PrefixData on student.Prefix_Id equals prefix.Id
                                          where student.Id.ToString() == uuid && studenthistory.IsCurrent == true
                                          select new
                                          {
                                              Code = student.Code,
                                              Prefix = prefix.Abbreviation,
                                              FirstName = student.FirstName,
                                              LastName = student.LastName,
                                              FullName = $"{prefix.Abbreviation}{student.FirstName} {student.LastName}",
                                              GradeLevel = gradelevel.Name,
                                          }).FirstOrDefaultAsync();

                var result = new DonateInfoViewModel
                {
                    UUID = uuid,
                    Code = info_student.Code,
                    Prefix = info_student.Prefix,
                    FirstName = info_student.FirstName,
                    LastName = info_student.LastName,
                    FullName = info_student.FullName,
                    GradeLevel = info_student.GradeLevel,
                };


                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        public async Task<IActionResult> GetDonateDetailListDataTable([FromBody] DataTableReq input)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null || user.Id is null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var result = await (from donate in _context.DonateData
                                join donatedt in _context.DonateDetailsData on donate.Id equals donatedt.Donate_Id
                                join student in _context.StudentData on donate.Student_Id equals student.Id.ToString()
                                join school in _context.SchoolYearData on donatedt.SchoolYear_Id equals school.Id
                                join term in _context.TermData on donatedt.Term_Id equals term.Id
                                join gradelevel in _context.GradeLevelData on donatedt.GradeLevel_Id equals gradelevel.Id
                                join channel in _context.DonateChannelData on donatedt.Channel_Id equals channel.Id
                                where donate.Student_Id == input.uuid && donatedt.Status == 1
                                select new DonateInfoTableModel
                                {
                                    Id = donate.Id,
                                    DonateDate = donatedt.DonateDate ?? DateTime.MinValue,
                                    Code = student.Code,
                                    FirstName = student.FirstName,
                                    LastName = student.LastName,
                                    FullName = $"{student.FirstName} {student.LastName}",
                                    GradeLevel = gradelevel.Name,
                                    Term = term.Name,
                                    StudyYear = school.Name,
                                    DonateChannel = channel.Donate_channel,
                                    Amount = donatedt.Donate ?? 0m
                                }).ToListAsync();

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

            _datalist = _datalist.OrderBy(x => x.DonateDate).ToList();


            int recordsTotal = _datalist?.Count ?? 0;
            var dataTable = new
            {
                draw = 1,
                recordsTotal = recordsTotal,
                recordsFiltered = 20,
                data = _datalist
            };
            return Json(dataTable);
        }


        [HttpGet]
        public async Task<IActionResult> OutstandingsByUuid(string uuid)
        {
            try
            {
                // ⬅️ 1) ดึงข้อมูลนักเรียนก่อน
                var studentInfo = await (
                    from student in _context.StudentData
                    join studenthistory in _context.StudentHistoryClassData
                        on student.Id.ToString() equals studenthistory.Student_Id
                    join grade in _context.GradeLevelData
                        on studenthistory.Grade_Level_Id equals grade.Id
                    join prefix in _context.PrefixData
                        on student.Prefix_Id equals prefix.Id
                    where student.Id.ToString() == uuid && studenthistory.IsCurrent == true
                    select new DonateInfoViewModel
                    {
                        UUID = uuid,
                        Code = student.Code,
                        Prefix = prefix.Abbreviation,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        FullName = prefix.Abbreviation + student.FirstName + " " + student.LastName,
                        GradeLevel = grade.Name,
                        StartDate = student.Start_Date != null
                                        ? student.Start_Date.Value.ToString("dd MMMM yyyy", thTHCulture)
                                        : "-"
                    }
                ).FirstOrDefaultAsync();

                if (studentInfo == null)
                    return View("NotFound");

                // ⬅️ 2) ดึงข้อมูลค้างจ่ายทั้งหมด
                var outstandings = await (from setting in _context.DonateSettingData

                                          join history in _context.StudentHistoryClassData
                                              on new { g = setting.Grade_Level_Id, t = setting.Term_Id, y = setting.SchoolYear_Id }
                                              equals new { g = history.Grade_Level_Id, t = history.Term_Id, y = history.School_Year_Id }

                                          join term in _context.TermData on history.Term_Id equals term.Id
                                          join year in _context.SchoolYearData on history.School_Year_Id equals year.Id
                                          join grade in _context.GradeLevelData on history.Grade_Level_Id equals grade.Id
                                          join room in _context.RoomData on history.Room_Id equals room.Id

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

                                          where history.Student_Id == uuid

                                          select new OutstandingDonationViewModel
                                          {
                                              StudentYear = year.Name ?? "",
                                              Term = term.Name ?? "",
                                              Term_Id = term.Id,
                                              TotalAmount = individual.OverrideAmount ?? setting.Donate ?? 0m,

                                              Grade_Level = grade.Name,
                                              Room = room.Name,

                                              DonateAmount = (from donate in _context.DonateData
                                                              join donatedt in _context.DonateDetailsData
                                                                  on donate.Id equals donatedt.Donate_Id
                                                              where donate.Student_Id == uuid
                                                                  && donatedt.SchoolYear_Id == setting.SchoolYear_Id
                                                                  && donatedt.Term_Id == setting.Term_Id
                                                                  && donatedt.Status == 1
                                                              select (decimal?)donatedt.Donate
                                              ).Sum() ?? 0
                                          }).OrderByDescending(x => x.StudentYear).ThenByDescending(x => x.Term).ToListAsync();


                // ⬅️ 3) รวม model
                var vm = new OutstandingsByUuidVM
                {
                    Student = studentInfo,
                    Outstandings = outstandings
                };

                return PartialView("_OutstandingsByUuid", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        public async Task<IActionResult> DonateFrom()
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

                var model = new DonateViewFromModel();
                await LoadDropdownsAsync();

                return PartialView("_DonateFrom", model);
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

            ViewBag.TermList = new List<SelectListItem>();
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
        public async Task<IActionResult> GetSchoolYears(string studentId)
        {
            var schoolYears = await (
                from his in _context.StudentHistoryClassData
                join sy in _context.SchoolYearData on his.School_Year_Id equals sy.Id
                where his.Student_Id == studentId
                orderby sy.Name descending
                select new
                {
                    value = sy.Id,
                    text = sy.Name
                }
            )
            .Distinct()
            .ToListAsync();

            return Json(schoolYears);
        }

        [HttpGet]
        public async Task<IActionResult> GetTerms(int gradeId, int schoolYearId, string studentId)
        {
            var terms = await (
                from his in _context.StudentHistoryClassData
                join t in _context.TermData on his.Term_Id equals t.Id
                where his.Grade_Level_Id == gradeId
                   && his.Student_Id == studentId
                   && his.School_Year_Id == schoolYearId
                select new
                {
                    value = t.Id,
                    text = t.Name
                }
            ).Distinct().ToListAsync();

            return Json(terms);
        }

        [HttpGet]
        public async Task<IActionResult> GetTermCards(int schoolYearId, string studentId)
        {
            var result = await (from his in _context.StudentHistoryClassData
                                join t in _context.TermData on his.Term_Id equals t.Id

                                join setting in _context.DonateSettingData
                                    on new { g = his.Grade_Level_Id, y = his.School_Year_Id, t = his.Term_Id }
                                    equals new { g = setting.Grade_Level_Id, y = setting.SchoolYear_Id, t = setting.Term_Id }

                                    // LEFT JOIN donate_setting_individual
                                join individual in _context.DonateSettingIndividualData
                                    on new { s = his.Student_Id, y = his.School_Year_Id, t = his.Term_Id }
                                    equals new { s = individual.Student_Id, y = individual.SchoolYear_Id, t = individual.Term_Id }
                                    into individualGroup
                                from individual in individualGroup.DefaultIfEmpty()

                                where his.School_Year_Id == schoolYearId
                                   && his.Student_Id == studentId

                                select new OutstandingDonationViewModel
                                {
                                    Term_Id = t.Id,
                                    Term = t.Name,

                                    // 👇 ตรงนี้คือ magic
                                    TotalAmount = individual != null && individual.OverrideAmount != null
                                        ? individual.OverrideAmount.Value
                                        : (setting.Donate ?? 0m),

                                    DonateAmount = (
                                        from donate in _context.DonateData
                                        join donatedt in _context.DonateDetailsData on donate.Id equals donatedt.Donate_Id
                                        where donate.Student_Id == studentId
                                              && donatedt.SchoolYear_Id == schoolYearId
                                              && donatedt.Term_Id == t.Id
                                              && donatedt.Status == 1
                                        select donatedt.Donate ?? 0
                                    ).Sum()
                                }).ToListAsync();

            return PartialView("_TermCards", result);
        }


        [HttpPost]
        public async Task<IActionResult> Donate(DonateViewFromModel model, IFormFile? EvidenceFile)
        {
            try
            {
                var datetimeNow = DateTime.Now;
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user?.Id == null)
                {
                    return Json(new { success = false, message = "User cannot be found!" });
                }

                // Validate ชื่อผู้บริจาค
                if (string.IsNullOrWhiteSpace(model.Donate_Name))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "กรุณากรอกชื่อผู้บริจาค" });
                    }

                    await LoadDropdownsAsync();
                    ModelState.AddModelError("Donate_Name", "กรุณากรอกชื่อผู้บริจาค");
                    return PartialView("_DonateFrom", model);
                }

                string? evidenceFileName = null;
                string? evidenceFullPath = null;

                if (EvidenceFile?.Length > 0)
                {
                    var yearName = await _context.SchoolYearData
                        .Where(x => x.Id == model.School_Year_Id)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync() ?? "UnknownYear";

                    var safeFolder = string.Concat(yearName.Split(Path.GetInvalidFileNameChars()))
                                            .Replace(" ", "_")
                                            .Trim();

                    var monthFolderName = DateTime.Now.ToString("yyyy-MM");

                    var physicalYearFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", safeFolder, monthFolderName);

                    Directory.CreateDirectory(physicalYearFolder);

                    evidenceFileName = $"{Guid.NewGuid()}_{EvidenceFile.FileName}".Replace(" ", "_");

                    var physicalFilePath = Path.Combine(physicalYearFolder, evidenceFileName);
                    using var stream = new FileStream(physicalFilePath, FileMode.Create);
                    await EvidenceFile.CopyToAsync(stream);

                    evidenceFullPath = $"uploads/{safeFolder}/{monthFolderName}/{evidenceFileName}";
                }


                // Get Channel
                var channel = await _context.DonateChannelData
                    .Where(x => x.Donate_channel == "Walk In")
                    .FirstOrDefaultAsync();

                if (channel is null)
                {
                    return Json(new { success = false, message = "ไม่พบช่องทางการบริจาค" });
                }

                // Save main donate data
                var donate = new DonateDataModel
                {
                    School_Year_Id = model.School_Year_Id,
                    Student_Id = model.Student_Id,
                    CreateBy = user.Id,
                    CreateDate = datetimeNow
                };

                _context.DonateData.Add(donate);
                await _context.SaveChangesAsync();

                var historyclass = await _context.StudentHistoryClassData.Where(x => x.School_Year_Id == donate.School_Year_Id && x.Student_Id == model.Student_Id).FirstOrDefaultAsync();

                // Save donate details
                var donateDetails = new DonateDetailsDataModel
                {
                    Donate_Id = donate.Id,
                    Donate = model.DonateAmount,
                    Donate_Name = model.Donate_Name,
                    DonateDate = model.DonateDate,
                    EvidenceImage = evidenceFullPath,
                    Status = 1,
                    Channel_Id = channel.Id,
                    CreateBy = user.Id,
                    CreateDate = datetimeNow,
                    SchoolYear_Id = model.School_Year_Id ?? 0,
                    Term_Id = model.Term_Id ?? 0,
                    GradeLevel_Id = historyclass?.Grade_Level_Id ?? 0,
                    Teacher_Id = model.Teacher_Id,
                };

                _context.DonateDetailsData.Add(donateDetails);
                await _context.SaveChangesAsync();

                // ส่ง LINE แจ้งเตือน
                var groupId = await _context.LineConfig.Where(x => x.channel_id == "2008848821").Select(x => x.line_group_id).FirstOrDefaultAsync();

                var result = await (
                                            from ddt in _context.DonateDetailsData
                                            join d in _context.DonateData on ddt.Donate_Id equals d.Id
                                            join student in _context.StudentData on d.Student_Id equals student.Id.ToString()
                                            join prefix in _context.PrefixData on student.Prefix_Id equals prefix.Id
                                            join history in _context.StudentHistoryClassData on student.Id.ToString() equals history.Student_Id
                                            join school in _context.SchoolYearData on history.School_Year_Id equals school.Id
                                            join grade in _context.GradeLevelData on history.Grade_Level_Id equals grade.Id
                                            join room in _context.RoomData on history.Room_Id equals room.Id

                                            join teacher in _context.TeacherData on ddt.Teacher_Id equals teacher.Id
                                            join prefixt in _context.PrefixData on teacher.Prefix_Id equals prefixt.Id
                                            where history.IsCurrent == true && student.Id.ToString() == model.Student_Id

                                            select new
                                            {
                                                FullName = prefix.Abbreviation + student.FirstName + " " + student.LastName,
                                                SchoolYearName = school.Name,
                                                GradeLevel = grade.Name + "/" + room.Name,
                                                TeacherFullName = prefixt.Name + teacher.FirstName + " " + teacher.LastName
                                            }
                                        ).FirstOrDefaultAsync();

                var message = new StringBuilder();
                message.AppendLine("📢 มีการบริจาคใหม่");
                message.AppendLine($"ผู้บริจาค :");
                message.AppendLine($"{donateDetails.Donate_Name}");
                message.AppendLine($"นักเรียน :");
                message.AppendLine($"{result.FullName}");
                message.AppendLine($"ชั้น :");
                message.AppendLine($"{result.GradeLevel}");
                message.AppendLine($"ปีการศึกษา :");
                message.AppendLine($"{result.SchoolYearName}");
                message.AppendLine($"จำนวนเงิน :");
                message.AppendLine($"{donateDetails.Donate:N2} บาท");
                message.AppendLine($"วันที่เวลาการบริจาค :");
                message.AppendLine($"{donateDetails.DonateDate?.ToString("dd/MMM/yyyy HH:mm")}");
                message.AppendLine($"ช่องทางการบริจาค :");
                message.AppendLine($"{channel.Donate_channel}");
                message.AppendLine($"คุณครูผู้รับเงิน :");
                message.AppendLine($"{result.TeacherFullName}");
                message.AppendLine($"ผู้ทำรายการ :");
                message.AppendLine($"{user.FirstName} {user.LastName}");
                message.AppendLine($"วันเวลาที่ทำรายการ :");
                message.AppendLine($"{donateDetails.CreateDate?.ToString("dd/MMM/yyyy HH:mm")}");

                var sendLine = await _lineMessage
                    .SendMessageToGroup(groupId, message.ToString());

                // เก็บสถานะ (ไม่ให้กระทบ flow หลัก)
                donateDetails.SendLine = sendLine;
                donateDetails.SendLineDate = DateTime.Now;

                _context.DonateDetailsData.Update(donateDetails);
                await _context.SaveChangesAsync();


                // AJAX Response
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var redirectUrl = Url.Action("Index");
                    return Json(new { success = true, message = "บันทึกข้อมูลสำเร็จ!", redirect = redirectUrl });
                }

                // fallback
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการบันทึกข้อมูล" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> DonateDetail(int id)
        {
            try
            {
                var result = await (
                    from donate in _context.DonateData
                    join donatedt in _context.DonateDetailsData on donate.Id equals donatedt.Donate_Id
                    join student in _context.StudentData on donate.Student_Id equals student.Id.ToString()
                    join prefix in _context.PrefixData on student.Prefix_Id equals prefix.Id
                    join teacher in _context.TeacherData on donatedt.Teacher_Id equals teacher.Id into teacherGroup
                    from teacher in teacherGroup.DefaultIfEmpty()
                    join pft in _context.PrefixData on teacher.Prefix_Id equals pft.Id
                    join school in _context.SchoolYearData on donatedt.SchoolYear_Id equals school.Id
                    join term in _context.TermData on donatedt.Term_Id equals term.Id
                    join gradelevel in _context.GradeLevelData on donatedt.GradeLevel_Id equals gradelevel.Id
                    join channel in _context.DonateChannelData on donatedt.Channel_Id equals channel.Id
                    join user in _context.Users on donatedt.CreateBy equals user.Id into userGroup
                    from user in userGroup.DefaultIfEmpty()
                    join lineUser in _context.LineRegistration on donatedt.CreateBy equals lineUser.LineUserId into lineGroup
                    from lineUser in lineGroup.DefaultIfEmpty()

                    where donatedt.Donate_Id == id && donatedt.Status == 1

                    select new DonateDetailsViewModel
                    {
                        Id = donate.Id,
                        DonateDate = donatedt.DonateDate ?? DateTime.MinValue,
                        Donate_Name = donatedt.Donate_Name,

                        Code = student.Code,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        FullName = $"{prefix.Abbreviation}{student.FirstName} {student.LastName}",

                        GradeLevel = gradelevel.Name,
                        Term = term.Name,
                        SchoolYear = school.Name,
                        Channel_Name = channel.Donate_channel,
                        Donate = donatedt.Donate ?? 0m,
                        EvidenceImage = donatedt.EvidenceImage,
                        Teacher_Name = teacher.Id != null ? $"{pft.Name}{teacher.FirstName} {teacher.LastName}" : "-",

                        CreatedBy = donatedt.Channel_Id == 3
                                        ? $"{user.FirstName} {user.LastName}"
                                        : donatedt.Channel_Id == 2
                                        ? $"{lineUser.DisplayName}"
                                        : "-"
                    }
                ).FirstOrDefaultAsync();

                return PartialView("_DonateDetail", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

    }
}
