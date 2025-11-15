using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CollabHub.Services;

namespace CollabHub.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly IFeatureFlagService _features;

        public TodoController(IFeatureFlagService features)
        {
            _features = features;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Premium()
        {
            if (!_features.IsEnabled("PremiumTodo", User))
            {
                // немає доступу до feature -> 403 або редірект на AccessDenied
                return Forbid();
            }

            return View();
        }
    }
}
