namespace CollabHub.Models.ViewModels;

public class LandingVm
{
    public int Organizations { get; set; }
    public int Venues { get; set; }
    public int Events { get; set; }
    public int Assets { get; set; }

    public List<OrgCard> Orgs { get; set; } = new();
    public List<VenueCard> VenuesList { get; set; } = new();
    public List<EventCard> Upcoming { get; set; } = new();
    public List<AssetCard> LatestAssets { get; set; } = new();

    public record OrgCard(int Id, string Name, string? Description);
    public record VenueCard(int Id, string Name, string? Address, string OrgName);
    public record EventCard(int Id, string Title, DateTime StartsAt, string? ImageUrl, string VenueName);
    public record AssetCard(int Id, string? OriginalFileName, string? FilePath, DateTime UploadedAt, int EventId);
}
