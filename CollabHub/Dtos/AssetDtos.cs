namespace CollabHub.Dtos;

public record AssetDto(int Id, int EventId, string? OriginalFileName, string? ContentType,
                       string? FilePath, DateTime UploadedAt);
public record CreateAssetDto(int EventId);
public record UpdateAssetDto(int EventId);
