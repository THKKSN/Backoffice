using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BackOfficeManagement.Services
{
    public class LanguageService
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LanguageService(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public string Get(string key)
        {
            return _localizer[key].Value;
        }

        public bool ChangeLang(HttpResponse response, string culture)
        {
            response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture)),
                new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddDays(30) });

            return true;
        }

        public string CurrentLang(HttpRequest request)
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }
}
