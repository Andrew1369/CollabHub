namespace CollabHub.Dtos;

public record OrganizationDto(int Id, string Name, string? Description);
public record CreateOrganizationDto(string Name, string? Description);
public record UpdateOrganizationDto(string Name, string? Description);
