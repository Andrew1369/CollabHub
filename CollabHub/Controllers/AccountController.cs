using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

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
            return View(); // можна зробити простий view з текстом "Доступ заборонено"
        }
    }
}
