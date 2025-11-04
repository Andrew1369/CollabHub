using CollabHub.Data;
using CollabHub.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Події, локації, файли";
        ViewData["MetaDescription"] = "Лендінг CollabHub: огляд організацій, локацій, майбутніх подій і файлів.";
        ViewBag.Canonical = $"{Request.Scheme}://{Request.Host}{Request.Path}";

        var now = DateTime.UtcNow;

        var vm = new LandingVm
        {
            Organizations = await _db.Organizations.CountAsync(),
            Venues = await _db.Venues.CountAsync(),
            Events = await _db.Events.CountAsync(),
            Assets = await _db.Assets.CountAsync(),
            Orgs = await _db.Organizations
                .OrderBy(o => o.Name).Take(6)
                .Select(o => new LandingVm.OrgCard(o.Id, o.Name, o.Description))
                .ToListAsync(),
            VenuesList = await _db.Venues
                .OrderBy(v => v.Name).Take(6)
                .Select(v => new LandingVm.VenueCard(
                    v.Id, v.Name, v.Address!, v.Organization != null ? v.Organization.Name : ("Org #" + v.OrganizationId)))
                .ToListAsync(),
            Upcoming = await _db.Events
                .Where(e => e.StartsAt >= now)
                .OrderBy(e => e.StartsAt).Take(6)
                .Select(e => new LandingVm.EventCard(
                    e.Id, e.Title, e.StartsAt, e.ImageUrl, e.Venue != null ? e.Venue.Name : ("Venue #" + e.VenueId)))
                .ToListAsync(),
            LatestAssets = await _db.Assets
                .OrderByDescending(a => a.UploadedAt).Take(6)
                .Select(a => new LandingVm.AssetCard(a.Id, a.OriginalFileName, a.FilePath, a.UploadedAt, a.EventId))
                .ToListAsync()
        };

        return View(vm);
    }
}
