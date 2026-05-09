using Microsoft.AspNetCore.Mvc;
using ResipWeb.Areas.Admin.Repository;

namespace ResipWeb.Controllers
{
    public class DevController : Controller
    {
        private readonly IEmailSender _emailSender;

        public DevController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpGet("/dev/test-email")]
        public async Task<IActionResult> TestEmail()
        {
            await _emailSender.SendAsync(
                "hotro.resip@gmail.com", 
                "TEST EMAIL RESIPWEB",
                "<h2>🎉 Gửi mail thành công!</h2><p>SMTP Gmail đã hoạt động.</p>"
            );

            return Ok("Sent email successfully!");
        }
    }
}
