using Microsoft.AspNetCore.Mvc;

namespace ResipWeb.Controllers
{
    public class LienHeController : Controller
    {
        // URL: /lien-he3222222
        [HttpGet("/lien-he")]
        public IActionResult LienHe()
        {
            return View("LienHe");
        }

        [HttpPost("/lien-he")]
        public IActionResult GuiLienHe(string ten, string email, string dienthoai, string noidung)
        {
            // TODO: Lưu database hoặc gửi email sau này
            ViewBag.ThongBao = "Cảm ơn bạn! Thông tin đã được gửi thành công.";
            return View("LienHe");
        }
    }
}
