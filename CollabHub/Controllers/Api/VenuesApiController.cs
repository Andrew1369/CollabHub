using CollabHub.Data;
using CollabHub.Dtos;
using CollabHub.Models;
using CollabHub.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollabHub.Controllers.Api;

[ApiController]
[Route("api/v1/venues")]
public class VenuesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public VenuesApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<VenueDto>>> GetAll(
    // A) сторінки
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    // B) офсет
    [FromQuery] int? skip = null,
    [FromQuery] int? limit = null,
    // фільтри/пошук/сортування
    [FromQuery] int? organizationId = null,
    [FromQuery] string? q = null,
    [FromQuery] string? sortBy = "Name",
    [FromQuery] string? sortDir = "asc")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(limit ?? pageSize, 1, 100);

        var queryable = _db.Venues.AsQueryable();

        if (organizationId.HasValue)
            queryable = queryable.Where(v => v.OrganizationId == organizationId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            queryable = queryable.Where(v =>
                v.Name.Contains(needle) ||
                (v.Address != null && v.Address.Contains(needle)));
        }

        var total = await queryable.CountAsync();
        queryable = queryable.OrderByDynamic(sortBy, sortDir);

        var effectiveSkip = skip ?? ((page - 1) * pageSize);

        var items = await queryable
            .Skip(effectiveSkip)
            .Take(pageSize)
            .Select(v => new VenueDto(v.Id, v.Name, v.Address, v.OrganizationId))
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
                ["organizationId"] = organizationId?.ToString(),
                ["q"] = q,
                ["sortBy"] = sortBy,
                ["sortDir"] = sortDir
            };
            nextLink = PagingHelpers.BuildNextLink(Request, nextQ);
        }

        var reportedPage = skip.HasValue ? (effectiveSkip / pageSize) + 1 : page;

        return Ok(new PagedResult<VenueDto>(items, total, reportedPage, pageSize, hasNext, nextLink));
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<VenueDto>> GetById(int id)
    {
        var v = await _db.Venues.FindAsync(id);
        return v is null ? NotFound() : new VenueDto(v.Id, v.Name, v.Address, v.OrganizationId);
    }

    [HttpPost]
    public async Task<ActionResult<VenueDto>> Create(CreateVenueDto dto)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == dto.OrganizationId))
            return BadRequest($"Organization {dto.OrganizationId} not found");

        var v = new Venue { Name = dto.Name, Address = dto.Address, OrganizationId = dto.OrganizationId };
        _db.Venues.Add(v);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = v.Id },
            new VenueDto(v.Id, v.Name, v.Address, v.OrganizationId));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateVenueDto dto)
    {
        var v = await _db.Venues.FindAsync(id);
        if (v is null) return NotFound();

        if (!await _db.Organizations.AnyAsync(o => o.Id == dto.OrganizationId))
            return BadRequest($"Organization {dto.OrganizationId} not found");

        v.Name = dto.Name; v.Address = dto.Address; v.OrganizationId = dto.OrganizationId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.Venues.FindAsync(id);
        if (v is null) return NotFound();
        _db.Venues.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
