using System.Security.Claims;

namespace CollabHub.Services
{
    public interface IFeatureFlagService
    {
        bool IsEnabled(string featureName, ClaimsPrincipal? user);
    }
}
