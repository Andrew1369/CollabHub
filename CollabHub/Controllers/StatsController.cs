using Microsoft.AspNetCore.Mvc;

namespace CollabHub.Controllers
{
    public class StatsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Statistics";
            ViewData["MetaDescription"] = "Статистика подій CollabHub за локаціями.";
            return View();
        }
    }
}
