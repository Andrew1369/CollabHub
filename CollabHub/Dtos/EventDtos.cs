namespace CollabHub.Dtos;

public record EventDto(int Id, int VenueId, string Title, string? Description,
                       DateTime StartsAt, DateTime EndsAt, int? Capacity, string? ImageUrl);
public record CreateEventDto(int VenueId, string Title, string? Description,
                             DateTime StartsAt, DateTime EndsAt, int? Capacity, string? ImageUrl);
public record UpdateEventDto(int VenueId, string Title, string? Description,
                             DateTime StartsAt, DateTime EndsAt, int? Capacity, string? ImageUrl);
