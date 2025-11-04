using System.Globalization;
using System.Text.Json;
using CollabHub.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CollabHub.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly string _apiKey;
    private readonly string _publicFolderId;

    public GoogleDriveService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleDriveService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _apiKey = configuration["GoogleDrive:ApiKey"] ?? string.Empty;
        _publicFolderId = configuration["GoogleDrive:PublicFolderId"] ?? string.Empty;
    }

    public async Task<IReadOnlyList<GoogleDriveFileDto>> GetFilesFromPublicFolderAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_publicFolderId))
        {
            _logger.LogWarning("Google Drive configuration missing (ApiKey/PublicFolderId).");
            return Array.Empty<GoogleDriveFileDto>();
        }

        var allFiles = new List<GoogleDriveFileDto>();
        string? nextPageToken = null;

        do
        {
            var query = $"'{_publicFolderId}' in parents and trashed = false";
            var fields = "nextPageToken,files(id,name,mimeType,modifiedTime,size)";

            var url =
                $"https://www.googleapis.com/drive/v3/files" +
                $"?key={_apiKey}" +
                $"&q={Uri.EscapeDataString(query)}" +
                $"&fields={Uri.EscapeDataString(fields)}" +
                $"&pageSize=1000";

            if (!string.IsNullOrEmpty(nextPageToken))
            {
                url += $"&pageToken={Uri.EscapeDataString(nextPageToken)}";
            }

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Drive API returned {StatusCode}", response.StatusCode);
                break;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var data = await JsonSerializer.DeserializeAsync<DriveListResponse>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken);

            if (data?.Files == null || data.Files.Count == 0)
            {
                break;
            }

            foreach (var f in data.Files)
            {
                var modified = ParseDateTime(f.ModifiedTime);
                var viewUrl = $"https://drive.google.com/file/d/{f.Id}/view?usp=sharing";

                long? size = null;
                if (!string.IsNullOrWhiteSpace(f.Size) && long.TryParse(f.Size, out var parsed))
                {
                    size = parsed;
                }

                allFiles.Add(new GoogleDriveFileDto(
                    f.Id ?? string.Empty,
                    f.Name ?? "(no name)",
                    f.MimeType ?? "application/octet-stream",
                    size,
                    modified,
                    viewUrl
                ));
            }

            nextPageToken = data.NextPageToken;
        }
        while (!string.IsNullOrEmpty(nextPageToken));

        return allFiles;
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var dt))
        {
            return dt;
        }

        return null;
    }

    // Внутрішні моделі для JSON
    private class DriveListResponse
    {
        public List<DriveFile> Files { get; set; } = new();
        public string? NextPageToken { get; set; }
    }

    private class DriveFile
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? MimeType { get; set; }
        public string? ModifiedTime { get; set; }
        public string? Size { get; set; }
    }
}
