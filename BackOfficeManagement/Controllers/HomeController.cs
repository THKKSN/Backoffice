using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BackOfficeManagement.Data;
using BackOfficeManagement.Models;
using System.Data;
using System.Diagnostics;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            ILogger<HomeController> logger
            )
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            bool isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    if (roles.Contains("Accounting"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (roles.Contains("Manager"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (roles.Contains("Administrator"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (roles.Contains("SysAdministrator"))
                    {
                        _logger.LogInformation("User role SysAdministrator redirect to Dashboard");
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
