using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace CollabHub.Utils;

public static class PagingHelpers
{
    public static string BuildNextLink(HttpRequest req, IDictionary<string, string?> q)
    {
        // Базовий абсолютний URL
        var baseUrl = $"{req.Scheme}://{req.Host}{req.Path}";
        return QueryHelpers.AddQueryString(baseUrl, q!);
    }
}
