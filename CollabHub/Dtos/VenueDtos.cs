namespace CollabHub.Dtos;

public record VenueDto(int Id, string Name, string? Address, int OrganizationId, double? Latitude, double? Longitude);
public record CreateVenueDto(string Name, string? Address, int OrganizationId, double? Latitude, double? Longitude);
public record UpdateVenueDto(string Name, string? Address, int OrganizationId, double? Latitude, double? Longitude);
