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
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace BackOfficeManagement.Controllers
{
    [AllowAnonymous]
    public class LineOAController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LineOAController> _logger;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");
        private readonly ILineMessageRepository _lineMessage;

        public LineOAController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<LineOAController> logger,

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

        [Route("LineOA/Loading")]
        [HttpGet]
        public async Task<IActionResult> Loading(int id)
        {
            var lineconfig = await _context.LineConfig
                                           .Where(x => x.id == id)
                                           .FirstOrDefaultAsync();

            if (lineconfig != null)
            {
                ViewBag.LineLIFFID = lineconfig.line_liff_id;
                return View();
            }

            return NotFound("Channel ID not found.");
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {

            return View();
        }

        // public async Task<IActionResult> DonateStudent(string uuid, string? message)
        // {
        //     var lineUser = await _context.LineRegistration
        //                     .FirstOrDefaultAsync(x => x.UserId == uuid);
        //     return View(new LineDonateViewModel
        //     {
        //         LineUserId = lineUser.LineUserId,
        //         Uuid = uuid,
        //     });
        // }

        public async Task<IActionResult> SearchStudent(string uuid)
        {
            var lineUser = await _context.LineRegistration
                            .FirstOrDefaultAsync(x => x.UserId == uuid);

            ViewBag.GrandLevelList = await (from g in _context.GradeLevelData
                                            where g.IsActive == true
                                            select new
                                            {
                                                Value = g.Id,
                                                Text = g.Name
                                            }).ToListAsync();
            ViewBag.SchoolYearList = await _context.SchoolYearData
                                        .Where(x => x.IsActive == true)
                                        .ToListAsync();
            return View(new LineDonateViewModel
            {
                LineUserId = lineUser?.LineUserId,
                Uuid = uuid
            });
        }


        [HttpPost]
        public async Task<IActionResult> SearchStudent(LineDonateViewModel model)
        {
            // โหลด GradeLevel และปีการศึกษาเสมอ
            ViewBag.GrandLevelList = await _context.GradeLevelData
                .Where(g => g.IsActive == true)
                .Select(g => new { Value = g.Id, Text = g.Name })
                .ToListAsync();

            ViewBag.SchoolYearList = await _context.SchoolYearData
                .Where(x => x.IsActive == true)
                .ToListAsync();

            if (model.GradeLevel_Id == null)
            {
                ModelState.AddModelError("", "กรุณาเลือกระดับชั้นปัจจุบัน");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.SearchText))
            {
                ModelState.AddModelError("", "กรุณากรอกข้อมูลค้นหา");
                return View(model);
            }

            // หา Student_Id ของนักเรียนที่อยู่ GradeLevel ที่เลือกและ IsCurrent == true
            var studentIds = await _context.StudentHistoryClassData
                .Where(h => h.Grade_Level_Id == model.GradeLevel_Id && h.IsCurrent == true)
                .Select(h => h.Student_Id) // string
                .ToListAsync();


            if (!studentIds.Any())
            {
                ModelState.AddModelError("", "ไม่พบนักเรียนในชั้นนี้");
                return View(model);
            }

            // โหลด StudentData ตาม studentIds
            var allStudents = await (from s in _context.StudentData
                                     join p in _context.PrefixData on s.Prefix_Id equals p.Id into prefixJoin
                                     from pj in prefixJoin.DefaultIfEmpty()
                                     where s.FirstName.Contains(model.SearchText) ||
                                           s.LastName.Contains(model.SearchText) ||
                                           s.NickName.Contains(model.SearchText) ||
                                           s.IDCard.Contains(model.SearchText)
                                     select new StudentSearchResultViewModel
                                     {
                                         IdGuid = s.Id,  // Guid
                                         Code = s.Code,
                                         Prefix = pj != null ? pj.Abbreviation : "",
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         NickName = s.NickName,
                                         IDCard = s.IDCard,
                                     }).ToListAsync();

            // filter ให้ตรงกับ studentIds (string) ใน C# หลัง query
            var students = allStudents
                .Where(s => studentIds.Contains(s.IdGuid.ToString()))
                .ToList();


            // โหลด StudentHistoryClassData ของนักเรียนเหล่านี้
            var studentHistories = await (from h in _context.StudentHistoryClassData
                                          join g in _context.GradeLevelData on h.Grade_Level_Id equals g.Id into gradeJoin
                                          from gj in gradeJoin.DefaultIfEmpty()
                                          join r in _context.RoomData on h.Room_Id equals r.Id into roomJoin
                                          from rj in roomJoin.DefaultIfEmpty()
                                          where studentIds.Contains(h.Student_Id) && h.IsCurrent == true
                                          select new
                                          {
                                              h.Student_Id,
                                              GradeLevel = gj != null ? gj.Name : "",
                                              Room = rj != null ? rj.Name : ""
                                          }).ToListAsync();


            // map ข้อมูล GradeLevel และ ClassRoom
            foreach (var student in students)
            {
                var history = studentHistories.FirstOrDefault(h => h.Student_Id == student.IdGuid.ToString());
                if (history != null)
                {
                    student.GradeLevel = history.GradeLevel;
                    student.Room = history.Room;
                }

                // แปลง Guid → string สำหรับ View
                student.Id = student.IdGuid.ToString();
            }

            ViewBag.StudentList = students;


            if (!students.Any())
            {
                ModelState.AddModelError("", "ไม่พบนักเรียนตามข้อมูลที่ค้นหา");
                return View(model);
            }

            ViewBag.StudentList = students;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoadYearTerm(string studentId)
        {
            try
            {
                var data = await (from history in _context.StudentHistoryClassData
                                  join setting in _context.DonateSettingData
                                        on new { g = history.Grade_Level_Id, t = history.Term_Id, y = history.School_Year_Id }
                                        equals new { g = setting.Grade_Level_Id, t = setting.Term_Id, y = setting.SchoolYear_Id }
                                  join year in _context.SchoolYearData on history.School_Year_Id equals year.Id
                                  join term in _context.TermData on history.Term_Id equals term.Id
                                  where history.Student_Id == studentId
                                  select new
                                  {
                                      SchoolYearId = year.Id,
                                      SchoolYearName = year.Name,
                                      TermId = term.Id,
                                      TermName = term.Name
                                  })
                                  .OrderByDescending(x => x.SchoolYearName)
                                  .ToListAsync();

                var result = data
                    .GroupBy(x => new { x.SchoolYearId, x.SchoolYearName })
                    .Select(g => new YearTermViewModel
                    {
                        SchoolYearId = g.Key.SchoolYearId,
                        SchoolYearName = g.Key.SchoolYearName,
                        Terms = g.Select(t => new TermItem
                        {
                            TermId = t.TermId,
                            TermName = t.TermName
                        }).ToList()
                    }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return BadRequest("โหลดข้อมูลล้มเหลว");
            }
        }
        [HttpGet]
        public async Task<IActionResult> OutstandingsByUuid(string uuid, int yearId, int termId)
        {
            try
            {
                var result = await (from setting in _context.DonateSettingData
                                    join historyClass in _context.StudentHistoryClassData
                                        on new { g = setting.Grade_Level_Id, t = setting.Term_Id, y = setting.SchoolYear_Id }
                                        equals new { g = historyClass.Grade_Level_Id, t = historyClass.Term_Id, y = historyClass.School_Year_Id }
                                    join term in _context.TermData on historyClass.Term_Id equals term.Id
                                    join gradelevel in _context.GradeLevelData on historyClass.Grade_Level_Id equals gradelevel.Id
                                    join schoolyear in _context.SchoolYearData on historyClass.School_Year_Id equals schoolyear.Id
                                    where historyClass.Student_Id == uuid
                                          && historyClass.School_Year_Id == yearId
                                          && historyClass.Term_Id == termId
                                    select new OutstandingDonationViewModel
                                    {
                                        StudentYear = schoolyear.Name,
                                        Term = term.Name,
                                        TotalAmount = setting.Donate ?? 0m,
                                        DonateAmount = (from donate in _context.DonateData
                                                        join donatedt in _context.DonateDetailsData on donate.Id equals donatedt.Donate_Id
                                                        where donate.Student_Id == uuid
                                                              && donatedt.SchoolYear_Id == yearId
                                                              && donatedt.Term_Id == termId
                                                        select donatedt.Donate ?? 0).Sum()
                                    }).ToListAsync();

                return PartialView("_OutstandingsByUuid", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Donate(string uuid, string studentId, int schoolYearId, int termId)
        {
            if (string.IsNullOrEmpty(uuid))
                return RedirectToAction("Index");

            var lineUser = await _context.LineRegistration
                .FirstOrDefaultAsync(x => x.UserId == uuid);

            if (lineUser == null)
                return RedirectToAction("Index");

            var requiredAmount = await _context.DonateSettingData
                .Where(x => x.SchoolYear_Id == schoolYearId && x.IsActive == true)
                .Select(x => x.Donate)
                .FirstOrDefaultAsync();

            var studentInfo = await _context.StudentData
                .Where(x => x.Id.ToString() == studentId).FirstOrDefaultAsync();

            var prefix = await _context.PrefixData.Where(x => x.Id == studentInfo.Prefix_Id).FirstOrDefaultAsync();

            var current = await _context.StudentHistoryClassData
                .Where(x => x.Student_Id == studentId && x.IsCurrent == true).FirstOrDefaultAsync();
            if (current is null)
                return NotFound();

            var history = await _context.StudentHistoryClassData
                .Where(x => x.Student_Id == studentId && x.School_Year_Id == schoolYearId && x.Term_Id == termId).FirstOrDefaultAsync();
            if (history is null)
                return NotFound();

            var gradelevel = await _context.GradeLevelData.Where(x => x.Id == current.Grade_Level_Id).FirstOrDefaultAsync();
            var room = await _context.RoomData.Where(x => x.Id == current.Room_Id).FirstOrDefaultAsync();
            var term = await _context.TermData.Where(x => x.Id == termId).FirstOrDefaultAsync();
            var schoolYear = await _context.SchoolYearData.Where(x => x.Id == schoolYearId).FirstOrDefaultAsync();
            var vm = new LineDonateViewModel
            {
                Uuid = uuid,
                LineUserId = lineUser.LineUserId,
                Student_Id = studentId,
                Prefix = prefix.Abbreviation,
                FirstName = studentInfo?.FirstName,
                LastName = studentInfo?.LastName,
                NiickName = studentInfo.NickName,
                IDCard = studentInfo.IDCard,
                Code = studentInfo.Code,
                GradeLevel_Id = gradelevel.Id,
                GradeLevel_Name = gradelevel.Name,
                Room_Name = room.Name,
                GradeLevel_History_Id = history.Grade_Level_Id,
                School_Year_Id = schoolYearId,
                School_Year = schoolYear.Name,
                Term_Id = termId,
                Term = term.Name,
                RequiredAmount = requiredAmount
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<JsonResult> GetDonateBalance(int schoolYearId, string? studentId, int term)
        {
            // ดึงยอดที่ต้องบริจาคจากตั้งค่า
            var requiredAmount = await _context.DonateSettingData
                .Where(x => x.SchoolYear_Id == schoolYearId && x.Term_Id == term && x.IsActive == true)
                .Select(x => x.Donate)
                .FirstOrDefaultAsync() ?? 0;

            // ถ้า studentId มีค่า ให้ filter ตาม student ด้วย
            decimal donatedAmount = 0;
            if (!string.IsNullOrEmpty(studentId))
            {
                donatedAmount = await (from d in _context.DonateData
                                       join dd in _context.DonateDetailsData on d.Id equals dd.Donate_Id
                                       where d.School_Year_Id == schoolYearId
                                       && dd.Term_Id == term
                                             && d.Student_Id == studentId
                                       select dd.Donate ?? 0)
                                      .SumAsync();
            }
            else
            {
                donatedAmount = await _context.DonateDetailsData
                    .Where(x => x.SchoolYear_Id == schoolYearId)
                    .SumAsync(x => x.Donate ?? 0);
            }

            var remain = requiredAmount - donatedAmount;

            return Json(new
            {
                requiredAmount,
                donatedAmount,
                remain
            });
        }


        [HttpPost]
        public async Task<IActionResult> Donate(LineDonateViewModel model, IFormFile? EvidenceFile, string action)
        {

            var datetimeNow = DateTime.Now;
            if (string.IsNullOrEmpty(model.Uuid))
                return RedirectToAction("Index");

            var lineUser = await _context.LineRegistration
                .FirstOrDefaultAsync(x => x.UserId == model.Uuid);

            if (string.IsNullOrEmpty(model.Uuid))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "ไม่พบผู้ใช้" });
                return RedirectToAction("Index");
            }

            if (lineUser == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "ไม่พบผู้ใช้ในระบบ" });
                return RedirectToAction("Index");
            }

            // โหลดปีการศึกษา
            ViewBag.schoolYearList = await _context.SchoolYearData
                .Where(x => x.IsActive == true)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToListAsync();

            // CASE 2: บริจาค
            if (action == "donate")
            {
                if (string.IsNullOrEmpty(model.Donate_Name))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "กรุณากรอกชื่อผู้บริจาค" });
                    }

                    ModelState.AddModelError("", "กรุณากรอกชื่อผู้บริจาค");
                    return View(model);
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

                    var physicalYearFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/uploads",
                        safeFolder,
                        monthFolderName
                    );

                    Directory.CreateDirectory(physicalYearFolder);

                    evidenceFileName = $"{Guid.NewGuid()}_{EvidenceFile.FileName}".Replace(" ", "_");

                    var physicalFilePath = Path.Combine(physicalYearFolder, evidenceFileName);
                    using var stream = new FileStream(physicalFilePath, FileMode.Create);
                    await EvidenceFile.CopyToAsync(stream);

                    evidenceFullPath = $"uploads/{safeFolder}/{monthFolderName}/{evidenceFileName}";
                }


                var channel = await _context.DonateChannelData.Where(x => x.Donate_channel == "LINE").FirstOrDefaultAsync();
                if (channel is null) NotFound();

                var donate = new DonateDataModel
                {
                    School_Year_Id = model.School_Year_Id,
                    Student_Id = model.Student_Id,
                    CreateBy = lineUser.LineUserId,
                    CreateDate = datetimeNow
                };
                _context.DonateData.Add(donate);
                await _context.SaveChangesAsync();

                var historyclass = await _context.StudentHistoryClassData.Where(x => x.School_Year_Id == donate.School_Year_Id).FirstOrDefaultAsync();

                var donateDetails = new DonateDetailsDataModel
                {
                    Donate_Id = donate.Id,
                    Donate = model.DonateAmount,
                    Donate_Name = model.Donate_Name,
                    DonateDate = model.DonateDate,
                    EvidenceImage = evidenceFileName,
                    Status = 1,
                    Channel_Id = channel.Id,
                    CreateBy = lineUser.LineUserId,
                    CreateDate = datetimeNow,
                    SchoolYear_Id = model.School_Year_Id ?? 0,
                    Term_Id = model.Term_Id ?? 0,
                    GradeLevel_Id = historyclass?.Grade_Level_Id ?? 0,
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

                                            where history.IsCurrent == true && student.Id.ToString() == model.Student_Id

                                            select new
                                            {
                                                FullName = prefix.Abbreviation + student.FirstName + " " + student.LastName,
                                                SchoolYearName = school.Name,
                                                GradeLevel = grade.Name + "/" + room.Name,
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
                message.AppendLine($" ");
                message.AppendLine($"ผู้ทำรายการ :");
                message.AppendLine($"{lineUser.DisplayName}");
                message.AppendLine($"วันเวลาที่ทำรายการ :");
                message.AppendLine($"{donateDetails.CreateDate?.ToString("dd/MMM/yyyy HH:mm")}");

                var sendLine = await _lineMessage
                    .SendMessageToGroup(groupId, message.ToString());

                // เก็บสถานะ (ไม่ให้กระทบ flow หลัก)
                donateDetails.SendLine = sendLine;
                donateDetails.SendLineDate = DateTime.Now;

                _context.DonateDetailsData.Update(donateDetails);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var redirectUrl = Url.Action("SearchStudent", new { uuid = model.Uuid, message = "donate" });
                    return Json(new { success = true, message = "บริจาคเรียบร้อยแล้ว!", redirect = redirectUrl });
                }

                return RedirectToAction("SearchStudent", new { uuid = model.Uuid, message = "donate" });
            }

            return View(model);
        }
    }
}