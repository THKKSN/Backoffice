using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Diagnostics;
using System.Globalization;
using BackOfficeManagement.Interface;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeacherController> _logger;
        private readonly IFileService _fileService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<TeacherController> logger,
            IStringLocalizer<SharedResource> localizer,
            LanguageService lang,
            IFileService fileService
        )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _localizer = localizer;
            _lang = lang;
            _fileService = fileService;
        }

        // GET: TeacherController
        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTeacherListDataTable([FromBody] DataTableReq input)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null || user.Id is null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var result = await (from t in _context.TeacherData
                                join prefix in _context.PrefixData on t.Prefix_Id equals prefix.Id
                                where t.IsActive == true
                                select new TeacherViewModel
                                {
                                    Id = t.Id,
                                    Prefix_Id = t.Prefix_Id,
                                    FirstName = t.FirstName,
                                    LastName = t.LastName,
                                    FullName = $"{prefix.Name}{t.FirstName} {t.LastName}",
                                    NickName = t.NickName,
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

            _datalist = _datalist.OrderBy(x => x.Id).ToList();


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

        private async Task LoadDropdownsAsync()
        {
            this.currentLang = this._lang.CurrentLang(HttpContext.Request);

            var prefix = await _context.PrefixData.ToListAsync();

            ViewBag.PrefixList = prefix
                .Where(x => x.Abbreviation != "ด.ช." && x.Abbreviation != "ด.ญ.")
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                }).ToList();
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user is null || user.Id is null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                await LoadDropdownsAsync();
                return PartialView();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        public async Task<ResponsesModel> Create(TeacherDTOModel form)
        {
            ResponsesModel response = new ResponsesModel();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                if (user is null || user.Id is null)
                {
                    response.status = false;
                    response.message = "User cannot be found!";
                    return response;
                }

                var entity = new TeacherDataModel
                {
                    Prefix_Id = form.Prefix_Id,
                    FirstName = form.FirstName,
                    LastName = form.FirstName,
                    NickName = form.NickName,
                    CreateBy = user.Id,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true
                };
                _context.TeacherData.Add(entity);
                await _context.SaveChangesAsync();
                response.status = true;
                response.message = "Success";
                return response;


            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Error" + ex.ToString();
                return response;
            }
        }

        public async Task<IActionResult> Update(int Id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user is null || user.Id is null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var _data = await (from t in _context.TeacherData
                                   where t.Id == Id
                                   select new TeacherDTOModel
                                   {
                                       Id = t.Id,
                                       Prefix_Id = t.Prefix_Id,
                                       FirstName = t.FirstName,
                                       LastName = t.LastName,
                                       NickName = t.NickName
                                   }).FirstOrDefaultAsync();

                await LoadDropdownsAsync();
                return PartialView(_data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        public async Task<ResponsesModel> Update(TeacherDTOModel form)
        {
            ResponsesModel response = new ResponsesModel();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                if (user is null || user.Id is null)
                {
                    response.status = false;
                    response.message = "User cannot be found!";
                    return response;
                }

                var _teacher = await _context.TeacherData.Where(t => t.Id == form.Id).FirstOrDefaultAsync();

                if (_teacher != null)
                {
                    _teacher.Prefix_Id = form.Prefix_Id;
                    _teacher.FirstName = form.FirstName;
                    _teacher.LastName = form.LastName;
                    _teacher.NickName = form.NickName;
                    _teacher.UpdateBy = user.Id;
                    _teacher.UpdateDate = DateTime.UtcNow;

                    _context.TeacherData.Update(_teacher);
                    await _context.SaveChangesAsync();
                    response.status = true;
                    response.message = "Success";
                    return response;
                }
                else
                {
                    response.status = false;
                    response.message = "ไม่พบข้อมูลคุณครู";
                    return response;
                }

            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Error" + ex.ToString();
                return response;
            }
        }


        public async Task<IActionResult> Detail(int Id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user is null || user.Id is null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var _data = await (from t in _context.TeacherData
                                   join prefix in _context.PrefixData on t.Prefix_Id equals prefix.Id
                                   where t.Id == Id
                                   select new TeacherViewDetailModel
                                   {
                                       Prefix = prefix.Name,
                                       FirstName = t.FirstName,
                                       LastName = t.LastName,
                                       FullName = $"{prefix.Name}{t.FirstName} {t.LastName}",
                                       NickName = t.NickName
                                   }).FirstOrDefaultAsync();

                await LoadDropdownsAsync();
                return PartialView(_data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpPost]
        public async Task<ResponsesModel> Delete(int Id)
        {
            ResponsesModel response = new ResponsesModel();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user is null || user.Id is null)
                {
                    response.status = false;
                    response.message = "User cannot be found!";
                    return response;
                }


                var teacher = await _context.TeacherData
                    .FirstOrDefaultAsync(x => x.Id == Id);

                if (teacher is null)
                {
                    response.status = false;
                    response.message = "teacher cannot be found!";
                    return response;
                }


                teacher.UpdateDate = DateTime.UtcNow;
                teacher.UpdateBy = user.Id;
                teacher.IsActive = false;
                _context.TeacherData.Update(teacher);
                await _context.SaveChangesAsync();

                response.status = true;
                response.message = "Success";
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Error" + ex.ToString();
                return response;
            }
        }

    }
}
