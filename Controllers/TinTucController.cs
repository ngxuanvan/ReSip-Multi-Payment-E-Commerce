using Microsoft.AspNetCore.Mvc;

namespace ResipWeb.Controllers
{
    public class TinTucController : Controller
    {
        [HttpGet("/tin-tuc")]
        public IActionResult TinTuc()
        {
            return View("TinTuc");
        }
    }
}
