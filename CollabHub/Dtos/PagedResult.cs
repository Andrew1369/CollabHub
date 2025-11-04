namespace CollabHub.Dtos;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize,
    bool HasNext,
    string? NextLink
);
