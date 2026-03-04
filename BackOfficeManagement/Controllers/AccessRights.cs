using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Diagnostics;
using System.Globalization;

namespace BackOfficeManagement.Controllers
{
    [AllowAnonymous]
    public class AccessRights : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LanguageService _lang;

        public AccessRights(
            ApplicationDbContext context,
            LanguageService lang
            )
        {
            _context = context;
            _lang = lang;
        }

        [HttpPost]
        [Route("AccessRights/FindLineUser")]
        public async Task<IActionResult> FindLineUser([FromBody] LineProfile model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var lineUser = await _context.LineRegistration
                .FirstOrDefaultAsync(x => x.LineUserId == model.LineUserId);

            if (lineUser is null)
            {
                var uuid = Guid.NewGuid().ToString();

                _context.LineRegistration.Add(new LineRegistrationModel
                {
                    UserId = uuid,
                    LineUserId = model.LineUserId,
                    DisplayName = model.DisplayName,
                    PictureUrl = model.PictureUrl,
                    RegisteredDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return Ok(new { message = "donate", uuid });
            }

            bool updated = false;
            if (lineUser.DisplayName != model.DisplayName)
            {
                lineUser.DisplayName = model.DisplayName;
                updated = true;
            }
            if (lineUser.PictureUrl != model.PictureUrl)
            {
                lineUser.PictureUrl = model.PictureUrl;
                updated = true;
            }
            if (updated) await _context.SaveChangesAsync();

            return Ok(new { message = "donate", uuid = lineUser.UserId });
        }


        [HttpPost]
        [Route("Residential/AccessRights/SetLanguage")]
        public IActionResult SetLanguage([FromBody] LangRequest data)
        {
            var result = _lang.ChangeLang(HttpContext.Response, data.languageCode);

            // ตรวจจับ base path จาก environment ปัจจุบัน (จะได้ beserve อัตโนมัติถ้ามี)
            var basePath = HttpContext.Request.PathBase.HasValue
                ? HttpContext.Request.PathBase.Value
                : string.Empty;

            var redirectPath = $"{basePath}{data.returnUrl}";

            return Ok(new
            {
                message = "success",
                redirect = redirectPath
            });
        }

    }
}
