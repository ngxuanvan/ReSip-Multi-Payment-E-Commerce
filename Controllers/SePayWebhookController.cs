using Microsoft.AspNetCore.Mvc;
using ResipWeb.Models.Payments.SePay;
using ResipWeb.Services.SePay;

namespace ResipWeb.Controllers
{
    [ApiController]
    public class SePayWebhookController : ControllerBase
    {
        private readonly ISePayWebhookService _webhook;

        public SePayWebhookController(ISePayWebhookService webhook)
        {
            _webhook = webhook;
        }

        [HttpPost("/hooks/sepay-payment")]
        public async Task<IActionResult> Receive([FromBody] SePayWebhookDto payload)
        {
            var auth = Request.Headers["Authorization"].ToString(); // "Apikey resip"
            await _webhook.HandleAsync(payload, auth);

            // SePay chỉ cần 2xx là OK
            return Ok(new { success = true });
        }
    }
}
