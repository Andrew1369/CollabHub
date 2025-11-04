using System.Security.Claims;
using CollabHub.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CollabHub.Tests
{
    public class AccountControllerTests
    {
        [Fact]
        public void Login_ReturnsChallenge_WithGoogleScheme()
        {
            // arrange
            var controller = new AccountController();

            // act
            var result = controller.Login(returnUrl: "/") as ChallengeResult;

            // assert
            Assert.NotNull(result);
            Assert.Contains(result!.AuthenticationSchemes, s => s == GoogleDefaults.AuthenticationScheme);

            // RedirectUri має бути або "/", або будь-який інший переданий returnUrl
            Assert.NotNull(result.Properties);
            Assert.Equal("/", result.Properties!.RedirectUri);
        }

        [Fact]
        public void Plans_SetsCurrentPlan_FromUserClaims()
        {
            // arrange
            var controller = new AccountController();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim("plan", "Premium")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            // act
            var result = controller.Plans() as ViewResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal("Premium", result!.ViewData["CurrentPlan"]);
        }

        [Fact]
        public void Plans_DefaultsToFree_IfNoPlanClaim()
        {
            // arrange
            var controller = new AccountController();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User")
                // без plan
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            // act
            var result = controller.Plans() as ViewResult;

            // assert
            Assert.NotNull(result);
            Assert.Equal("Free", result!.ViewData["CurrentPlan"]);
        }
    }
}
