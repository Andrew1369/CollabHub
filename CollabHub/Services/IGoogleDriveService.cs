using CollabHub.Dtos;

namespace CollabHub.Services;

public interface IGoogleDriveService
{
    Task<IReadOnlyList<GoogleDriveFileDto>> GetFilesFromPublicFolderAsync(
        CancellationToken cancellationToken = default);
}
