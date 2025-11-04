using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace CollabHub.Services
{
    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly IConfiguration _configuration;

        public FeatureFlagService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsEnabled(string featureName, ClaimsPrincipal? user)
        {
            // план за замовчуванням
            var plan = user?.FindFirst("plan")?.Value ?? "Free";

            // шлях типу FeatureFlags:Plans:Premium:PremiumTodo
            var path = $"FeatureFlags:Plans:{plan}:{featureName}";

            // якщо в конфігу немає — вважаємо, що false
            return _configuration.GetValue<bool>(path);
        }
    }
}
