using Azure.Core;
using CollabHub.Data;
using Microsoft.AspNetCore.Mvc;
using System;

using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CollabHub.Controllers.Api;

[ApiController]
[Route("api/v1")]
public class GeoController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public GeoController(ApplicationDbContext db) => _db = db;

    // GET /api/v1/venues/geo
    [HttpGet("venues/geo")]
    public async Task<IActionResult> VenuesGeo()
    {
        var features = await _db.Venues
            .Where(v => v.Latitude.HasValue && v.Longitude.HasValue)
            .Select(v => new {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { v.Longitude!.Value, v.Latitude!.Value } },
                properties = new
                {
                    id = v.Id,
                    name = v.Name,
                    address = v.Address,
                    organizationId = v.OrganizationId,
                    detailsUrl = Url.Action("Details", "Venues", new { id = v.Id }, Request.Scheme)
                }
            })
            .ToListAsync();
        return Ok(new { type = "FeatureCollection", features });
    }

    // GET /api/v1/events/geo?upcoming=true
    [HttpGet("events/geo")]
    public async Task<IActionResult> EventsGeo([FromQuery] bool upcoming = true)
    {
        var now = DateTime.UtcNow;

        var query = _db.Events
            .Include(e => e.Venue)
            .AsQueryable();

        if (upcoming)
            query = query.Where(e => e.StartsAt >= now);

        var features = await query
            .Where(e => e.Venue != null && e.Venue.Latitude.HasValue && e.Venue.Longitude.HasValue)
            .OrderBy(e => e.StartsAt)
            .Take(500) // безпека від великих вибірок
            .Select(e => new {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { e.Venue!.Longitude!.Value, e.Venue!.Latitude!.Value } },
                properties = new
                {
                    id = e.Id,
                    title = e.Title,
                    startsAt = e.StartsAt,
                    venueName = e.Venue!.Name,
                    imageUrl = e.ImageUrl,
                    detailsUrl = Url.Action("Details", "Events", new { id = e.Id }, Request.Scheme)
                }
            })
            .ToListAsync();

        var fc = new { type = "FeatureCollection", features };
        return Ok(fc);
    }
}
