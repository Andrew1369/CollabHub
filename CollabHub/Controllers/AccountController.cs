using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CollabHub.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // куди повертатись після логіну
            var redirectUrl = returnUrl ?? Url.Action("Index", "Home")!;
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            // Челенджим Google OAuth
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult AccessDenied()
        {
            return View(); 
        }

        [Authorize]
        [HttpGet]
        public IActionResult Plans()
        {
            var plan = User.FindFirst("plan")?.Value ?? "Free";
            ViewData["CurrentPlan"] = plan;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPlan(string plan)
        {
            if (string.IsNullOrWhiteSpace(plan))
            {
                return RedirectToAction("Plans");
            }

            // нормалізуємо
            plan = plan.Equals("Premium", StringComparison.OrdinalIgnoreCase)
                ? "Premium"
                : "Free";

            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = authResult.Principal as ClaimsPrincipal;
            if (principal == null)
            {
                return RedirectToAction("Login");
            }

            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return RedirectToAction("Login");
            }

            // видаляємо старий план
            var existingPlanClaim = identity.FindFirst("plan");
            if (existingPlanClaim != null)
            {
                identity.RemoveClaim(existingPlanClaim);
            }

            // додаємо новий
            identity.AddClaim(new Claim("plan", plan));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authResult.Properties);

            return RedirectToAction("Plans");
        }
    }
}
