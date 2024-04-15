using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace EhriMemoMap.Controllers
{
    [Route("[controller]/[action]")]
    public class CultureController : Controller
    {
        public IActionResult Set(string culture, string redirectUri)
        {
            if (culture != null)
            {
                HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(culture, culture)));
            }

            //var returnUrl = HttpContext.Request.Headers["Referer"].ToString().Replace((HttpContext.Request.Scheme + "://" + HttpContext.Request.Host), "");

            return LocalRedirect(redirectUri);
        }
    }
}