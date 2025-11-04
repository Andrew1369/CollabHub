namespace CollabHub.Dtos;

public record AssetDto(int Id, int EventId, string? OriginalFileName, string? ContentType,
                       string? FilePath, DateTime UploadedAt);
public record CreateAssetDto(int EventId); // файл приймемо окремо (multipart)
public record UpdateAssetDto(int EventId); // файл можемо замінити (multipart)
