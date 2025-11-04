using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CollabHub.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "ToDo-List";
            ViewData["MetaDescription"] = "Особистий список задач організатора подій у CollabHub.";
            return View();
        }
    }
}
