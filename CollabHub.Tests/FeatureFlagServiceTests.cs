using System.Collections.Generic;
using System.Security.Claims;
using CollabHub.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CollabHub.Tests
{
    public class FeatureFlagServiceTests
    {
        private IFeatureFlagService CreateService()
        {
            // емуляція appsettings.json
            var data = new Dictionary<string, string?>
            {
                ["FeatureFlags:Plans:Free:PremiumTodo"] = "false",
                ["FeatureFlags:Plans:Premium:PremiumTodo"] = "true"
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data!)
                .Build();

            return new FeatureFlagService(config);
        }

        private ClaimsPrincipal CreateUser(string? plan = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Test User")
            };

            if (!string.IsNullOrWhiteSpace(plan))
            {
                claims.Add(new Claim("plan", plan));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void FreePlan_ShouldHavePremiumTodoDisabled()
        {
            var service = CreateService();
            var user = CreateUser("Free");

            var enabled = service.IsEnabled("PremiumTodo", user);

            Assert.False(enabled);
        }

        [Fact]
        public void PremiumPlan_ShouldHavePremiumTodoEnabled()
        {
            var service = CreateService();
            var user = CreateUser("Premium");

            var enabled = service.IsEnabled("PremiumTodo", user);

            Assert.True(enabled);
        }

        [Fact]
        public void NoPlanClaim_DefaultsToFree()
        {
            var service = CreateService();
            var user = CreateUser(plan: null); // без claim "plan"

            var enabled = service.IsEnabled("PremiumTodo", user);

            // Free для PremiumTodo -> false
            Assert.False(enabled);
        }
    }
}
