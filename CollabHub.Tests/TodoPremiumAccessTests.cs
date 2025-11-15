using System.Collections.Generic;
using System.Security.Claims;
using CollabHub.Controllers;
using CollabHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CollabHub.Tests
{
    public class TodoPremiumAccessTests
    {
        private IFeatureFlagService CreateFeatureService(bool freePremiumTodo, bool premiumPremiumTodo)
        {
            var data = new Dictionary<string, string?>
            {
                ["FeatureFlags:Plans:Free:PremiumTodo"] = freePremiumTodo.ToString(),
                ["FeatureFlags:Plans:Premium:PremiumTodo"] = premiumPremiumTodo.ToString()
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data!)
                .Build();

            return new FeatureFlagService(config);
        }

        private TodoController CreateController(IFeatureFlagService features, ClaimsPrincipal user)
        {
            var controller = new TodoController(features);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
            return controller;
        }

        private ClaimsPrincipal CreateUser(string plan)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim("plan", plan)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void PremiumAction_ForFreePlan_ShouldReturnForbid()
        {
            // Free -> PremiumTodo:false, Premium -> PremiumTodo:true
            var features = CreateFeatureService(freePremiumTodo: false, premiumPremiumTodo: true);
            var user = CreateUser("Free");
            var controller = CreateController(features, user);

            var result = controller.Premium();

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void PremiumAction_ForPremiumPlan_ShouldReturnView()
        {
            var features = CreateFeatureService(freePremiumTodo: false, premiumPremiumTodo: true);
            var user = CreateUser("Premium");
            var controller = CreateController(features, user);

            var result = controller.Premium();

            Assert.IsType<ViewResult>(result);
        }
    }
}
