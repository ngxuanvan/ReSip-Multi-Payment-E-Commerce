using Microsoft.AspNetCore.Mvc;

public class GioiThieuController : Controller
{
    [HttpGet("/gioi-thieu")]
    public IActionResult GioiThieu()
    {
        return View("GioiThieu");
    }
}
