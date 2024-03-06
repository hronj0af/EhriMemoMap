using EhriMemoMap.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace EhriMemoMap.Controllers
{
    [Route("[controller]/[action]")]
    public class WMSProxyController : Controller
    {
        private readonly HttpClient _client;

        public WMSProxyController(HttpClient client) 
        {
            _client = client;
        }

        public async Task<IActionResult> Get()
        {
            var url = "https://geodata.ehri-project.eu/geoserver/ehri/wms?" + HttpContext.Request.QueryString.Value;
            var imageFileName = url.GetHashCode() + ".png";
            var imageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache/" + imageFileName);

            Response.Headers.Add("Cache-Control", "public,max-age=1000000");

            if (System.IO.File.Exists(imageFilePath))
                return File(System.IO.File.ReadAllBytes(imageFilePath), "image/png", imageFileName);

            var result = await _client.GetByteArrayAsync(url);
            System.IO.File.WriteAllBytes(imageFilePath, result);
            return File(result, "image/png", imageFileName);
        }
    }
}