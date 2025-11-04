using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MapController : Controller
{
    [HttpGet("/Map")]
    public IActionResult Index() => View();
}
