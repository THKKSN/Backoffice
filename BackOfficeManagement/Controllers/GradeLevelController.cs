using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class GradeLevelController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GradeLevelController> _logger;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public GradeLevelController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<GradeLevelController> logger,

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

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }
                var GradeLevelData = await _context.GradeLevelData.ToListAsync();

                var model = GradeLevelData.Select(x => new GradeLevelViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    NameEng = x.NameEng,
                    IsActive = x.IsActive,
                    CreateBy = x.CreateBy,
                    CreateDate = x.CreateDate,
                    UpdateBy = x.UpdateBy,
                    UpdateDate = x.UpdateDate
                }).ToList();


                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetGradeLevelTable()
        {
            var gradeLevels = await _context.GradeLevelData.ToListAsync();
            var rooms = await _context.RoomData.ToListAsync();

            var result = gradeLevels.Select(g => new
            {
                id = g.Id,
                name = g.Name,
                nameEng = g.NameEng,
                roomCount = rooms.Count(r => r.Grade_Level_Id == g.Id)
            });

            return Json(new { data = result });
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }

                var gradeLevel = await _context.GradeLevelData
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (gradeLevel == null)
                    return View("NotFound");

                var rooms = await _context.RoomData
                    .Where(x => x.Grade_Level_Id == id)
                    .Select(r => new RoomDataModel
                    {
                        Name = r.Name,
                        IsActive = r.IsActive ?? false
                    })
                    .ToListAsync();

                var model = new GradeLevelViewModel
                {
                    Id = gradeLevel.Id,
                    Name = gradeLevel.Name,
                    NameEng = gradeLevel.NameEng,
                    Rooms = rooms.Select(r => new RoomViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        IsActive = r.IsActive,
                    }).ToList()
                };

                return PartialView(model);
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
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }
                var model = new GradeLevelViewModel();


                return PartialView("Create", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeLevelViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var gradeLevel = new GradeLevelDataModel
                {
                    Name = model.Name,
                    NameEng = model.NameEng,
                    IsActive = true,
                    CreateBy = user.Id,
                    CreateDate = DateTime.Now
                };

                _context.GradeLevelData.Add(gradeLevel);
                await _context.SaveChangesAsync();

                var rooms = new List<RoomDataModel>();
                for (int i = 1; i <= model.Room_Count; i++)
                {
                    rooms.Add(new RoomDataModel
                    {
                        Grade_Level_Id = gradeLevel.Id,
                        Name = $"{i}",
                        NameEng = $"{i}",
                        IsActive = true,
                        CreateBy = user.Id,
                        CreateDate = DateTime.Now
                    });
                }

                _context.RoomData.AddRange(rooms);
                await _context.SaveChangesAsync();

                return Json(new { status = 200, message = "GradeLevel and rooms created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Json(new { status = 500, message = ex.Message });
            }
        }

        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }

                var gradeLevel = await _context.GradeLevelData
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (gradeLevel == null)
                    return View("NotFound");

                var rooms = await _context.RoomData
                    .Where(x => x.Grade_Level_Id == id)
                    .Select(r => new RoomViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Code = r.Code,
                        IsActive = r.IsActive ?? true
                    })
                    .ToListAsync();

                var model = new GradeLevelViewModel
                {
                    Id = gradeLevel.Id,
                    Name = gradeLevel.Name,
                    NameEng = gradeLevel.NameEng,
                    Rooms = rooms
                };

                return PartialView("Update", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(GradeLevelViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var grade = await _context.GradeLevelData.FindAsync(model.Id);
            if (grade == null) return View("NotFound");

            grade.Name = model.Name;
            grade.NameEng = model.NameEng;

            // update room name + code
            foreach (var rm in model.Rooms)
            {
                if (rm.Id > 0)
                {
                    var room = await _context.RoomData.FindAsync(rm.Id);
                    if (room != null)
                    {
                        room.Name = rm.Name;
                        room.Code = rm.Code;
                    }
                }
                else
                {
                    // add new room
                    _context.RoomData.Add(new RoomDataModel
                    {
                        Name = rm.Name,
                        Code = rm.Code,
                        Grade_Level_Id = model.Id,
                        IsActive = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRoomStatus(int id, bool isActive)
        {
            try
            {
                var room = await _context.RoomData.FindAsync(id);
                if (room == null)
                    return Json(new { success = false, message = "ไม่พบข้อมูลห้องเรียน" });

                room.IsActive = isActive;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "บันทึกสำเร็จ", isActive = room.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleRoomStatus error for id {Id}", id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
            }
        }
    }
}
