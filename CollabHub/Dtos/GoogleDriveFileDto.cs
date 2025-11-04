namespace CollabHub.Dtos;

public record GoogleDriveFileDto(
    string Id,
    string Name,
    string MimeType,
    long? Size,
    DateTime? ModifiedTime,
    string ViewUrl
);
