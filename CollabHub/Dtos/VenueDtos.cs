namespace CollabHub.Dtos;

public record VenueDto(int Id, string Name, string? Address, int OrganizationId);
public record CreateVenueDto(string Name, string? Address, int OrganizationId);
public record UpdateVenueDto(string Name, string? Address, int OrganizationId);
