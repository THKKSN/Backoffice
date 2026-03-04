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
    public class SchoolYearController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchoolYearController> _logger;

        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public SchoolYearController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<SchoolYearController> logger,

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

                // ดึงข้อมูลปีการศึกษา
                var schoolYearData = await _context.SchoolYearData.ToListAsync();

                // Map เข้า ViewModel
                var model = schoolYearData.Select(x => new SchoolYearViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        public async Task<IActionResult> ViewTerm(int id)
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

                var schoolYear = await _context.SchoolYearData.Where(x => x.Id == id).FirstOrDefaultAsync();

                // id = SchoolYearId
                var terms = await _context.TermData
                .Where(x => x.SchoolYear_Id == id)
                .OrderBy(x => x.Name)
                .Select(x => new TermViewModel
                {
                    TermId = x.Id,
                    Term = x.Name,
                    IsActive = x.IsActive
                })
                .ToListAsync();


                var model = new SchoolYearTermViewModel
                {
                    SchoolYearId = id,
                    SchoolYear = schoolYear.Name,
                    Terms = terms,
                };

                return PartialView("ViewTerm", model);
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
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user?.Id == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }

                var schoolYear = await _context.SchoolYearData
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (schoolYear == null)
                    return NotFound();

                var terms = await _context.TermData
                    .Where(x => x.SchoolYear_Id == id)
                    .ToListAsync();

                var model = new SchoolYearTermViewModel
                {
                    SchoolYearId = schoolYear.Id,
                    SchoolYear = schoolYear.Name,
                    Terms = terms.Select(t => new TermViewModel
                    {
                        TermId = t.Id,
                        Term = t.Name,
                        IsActive = t.IsActive,
                        SchoolYearId = t.SchoolYear_Id
                    }).ToList()
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchoolYear(SchoolYearTermViewModel model)
        {
            try
            {
                var schoolYear = await _context.SchoolYearData
                    .FirstOrDefaultAsync(x => x.Id == model.SchoolYearId);

                if (schoolYear == null)
                    return NotFound();

                schoolYear.Name = model.SchoolYear; // อัปเดตชื่ออย่างเดียว

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
        public async Task<IActionResult> ToggleTermStatus(int id, bool active)
        {
            var term = await _context.TermData.FindAsync(id);

            if (term == null)
                return NotFound();

            term.IsActive = active;
            await _context.SaveChangesAsync();

            return Ok();
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
                var model = new SchoolYearViewModel();


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
        public async Task<IActionResult> Create(SchoolYearViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }

                if (ModelState.IsValid)
                {
                    var newSchoolYear = new SchoolYearDataModel
                    {
                        Name = model.Name,
                        IsActive = true,
                        Status = 1,
                        CreateBy = user.Id,
                        CreateDate = DateTime.Now
                    };

                    _context.SchoolYearData.Add(newSchoolYear);
                    await _context.SaveChangesAsync();

                    var terms = new[]
                    {
                        new TermDataModel
                        {
                            Name = "1",
                            SchoolYear_Id = newSchoolYear.Id,
                            IsActive = false,
                            CreateBy = user.Id,
                            CreateDate = DateTime.Now
                        },
                        new TermDataModel
                        {
                            Name = "2",
                            SchoolYear_Id = newSchoolYear.Id,
                            IsActive = false,
                            CreateBy = user.Id,
                            CreateDate = DateTime.Now
                        }
                    };

                    _context.TermData.AddRange(terms);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }
    }
}
