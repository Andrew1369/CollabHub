using Microsoft.AspNetCore.Mvc;

public class MapController : Controller
{
    [HttpGet("/Map")]
    public IActionResult Index() => View();
}
