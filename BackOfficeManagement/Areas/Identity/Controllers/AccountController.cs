using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using BackOfficeLibrary;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;

namespace BackOfficeManagement.Areas.Identity.Controllers
{
    [Area("Identity")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly LanguageService _lang;
        private readonly string _path;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger,
            LanguageService lang,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _lang = lang;
            _path = configuration["Path"] ?? string.Empty;
        }

        // ==============================
        // LOGIN (GET)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;

            return View(new LoginViewModel());
        }

        // ==============================
        // LOGIN (POST)
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key} = {Request.Form[key]}");
            }
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return View(model);

            // หา user จาก email หรือ username
            ApplicationUser? user;

            if (model.UserName.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(model.UserName);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.UserName);
            }

            if (user == null)
            {
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password!, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                var url = _path + "/Home";

                return LocalRedirect(url);
            }


            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (!string.IsNullOrEmpty(_path))
                    return LocalRedirect(_path + "/Home");

                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }

        // ==============================
        // LOGOUT
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        // ==============================
        // ACCESS DENIED
        // ==============================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login");

            return View();
        }

        // ==============================
        // LANGUAGE
        // ==============================
        [HttpGet]
        public IActionResult Language(string languageCode, string? returnUrl)
        {
            _lang.ChangeLang(HttpContext.Response, languageCode);

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Login");
        }
    }
}