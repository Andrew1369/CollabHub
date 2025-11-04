using CollabHub.Data;
using CollabHub.Dtos;
using CollabHub.Models;
using CollabHub.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollabHub.Controllers.Api;

[ApiController]
[Route("api/v1/events")]
public class EventsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public EventsApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<EventDto>>> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? skip = null,
    [FromQuery] int? limit = null,
    [FromQuery] string? q = null,
    [FromQuery] int? venueId = null,
    [FromQuery] DateTime? dateFrom = null,
    [FromQuery] DateTime? dateTo = null,
    [FromQuery] string? sortBy = "StartsAt",
    [FromQuery] string? sortDir = "asc")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(limit ?? pageSize, 1, 100);

        var queryable = _db.Events.AsQueryable();

        if (venueId.HasValue)
            queryable = queryable.Where(e => e.VenueId == venueId.Value);
        if (dateFrom.HasValue)
            queryable = queryable.Where(e => e.StartsAt >= dateFrom.Value);
        if (dateTo.HasValue)
            queryable = queryable.Where(e => e.EndsAt <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            queryable = queryable.Where(e =>
                e.Title.Contains(needle) ||
                (e.Description != null && e.Description.Contains(needle)));
        }

        var total = await queryable.CountAsync();
        queryable = queryable.OrderByDynamic(sortBy, sortDir);

        var effectiveSkip = skip ?? ((page - 1) * pageSize);

        var items = await queryable
            .Skip(effectiveSkip)
            .Take(pageSize)
            .Select(e => new EventDto(
                e.Id, e.VenueId, e.Title, e.Description,
                e.StartsAt, e.EndsAt, e.Capacity, e.ImageUrl))
            .ToListAsync();

        var hasNext = effectiveSkip + items.Count < total;

        string? nextLink = null;
        if (hasNext)
        {
            var nextQ = new Dictionary<string, string?>
            {
                [skip.HasValue || limit.HasValue ? "skip" : "page"] =
                    (skip.HasValue ? (effectiveSkip + pageSize).ToString() : (page + 1).ToString()),
                [skip.HasValue || limit.HasValue ? "limit" : "pageSize"] = pageSize.ToString(),
                ["venueId"] = venueId?.ToString(),
                ["dateFrom"] = dateFrom?.ToString("o"),
                ["dateTo"] = dateTo?.ToString("o"),
                ["q"] = q,
                ["sortBy"] = sortBy,
                ["sortDir"] = sortDir
            };
            nextLink = PagingHelpers.BuildNextLink(Request, nextQ);
        }

        var reportedPage = skip.HasValue ? (effectiveSkip / pageSize) + 1 : page;

        return Ok(new PagedResult<EventDto>(items, total, reportedPage, pageSize, hasNext, nextLink));
    }



    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var e = await _db.Events.FindAsync(id);
        return e is null ? NotFound()
            : new EventDto(e.Id, e.VenueId, e.Title, e.Description, e.StartsAt, e.EndsAt, e.Capacity, e.ImageUrl);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create(CreateEventDto dto)
    {
        if (!await _db.Venues.AnyAsync(v => v.Id == dto.VenueId))
            return BadRequest($"Venue {dto.VenueId} not found");

        var e = new Event
        {
            VenueId = dto.VenueId,
            Title = dto.Title,
            Description = dto.Description,
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt,
            Capacity = dto.Capacity,
            ImageUrl = dto.ImageUrl
        };
        _db.Events.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = e.Id },
            new EventDto(e.Id, e.VenueId, e.Title, e.Description, e.StartsAt, e.EndsAt, e.Capacity, e.ImageUrl));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEventDto dto)
    {
        var e = await _db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (!await _db.Venues.AnyAsync(v => v.Id == dto.VenueId))
            return BadRequest($"Venue {dto.VenueId} not found");

        e.VenueId = dto.VenueId; e.Title = dto.Title; e.Description = dto.Description;
        e.StartsAt = dto.StartsAt; e.EndsAt = dto.EndsAt; e.Capacity = dto.Capacity; e.ImageUrl = dto.ImageUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e is null) return NotFound();
        _db.Events.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
