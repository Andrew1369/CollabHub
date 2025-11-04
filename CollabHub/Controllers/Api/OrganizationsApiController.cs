using CollabHub.Data;
using CollabHub.Dtos;
using CollabHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollabHub.Utils;

namespace CollabHub.Controllers.Api;

[ApiController]
[Route("api/v1/organizations")]
public class OrganizationsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public OrganizationsApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrganizationDto>>> GetAll(
    // Варіант A: сторінки
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    // Варіант B: офсет
    [FromQuery] int? skip = null,
    [FromQuery] int? limit = null,
    // Пошук/сортування
    [FromQuery] string? q = null,
    [FromQuery] string? sortBy = "Name",
    [FromQuery] string? sortDir = "asc")
    {
        // Нормалізація параметрів
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Якщо клієнт подав limit — вважаємо, що він хоче режим limit/skip
        if (limit.HasValue) pageSize = Math.Clamp(limit.Value, 1, 100);

        var queryable = _db.Organizations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            queryable = queryable.Where(o =>
                o.Name.Contains(needle) ||
                (o.Description != null && o.Description.Contains(needle)));
        }

        var total = await queryable.CountAsync();

        // Сортування (динамічне)
        queryable = queryable.OrderByDynamic(sortBy, sortDir);

        // Обчислюємо ефективний офсет
        var effectiveSkip = skip ?? ((page - 1) * pageSize);

        var items = await queryable
            .Skip(effectiveSkip)
            .Take(pageSize)
            .Select(o => new OrganizationDto(o.Id, o.Name, o.Description))
            .ToListAsync();

        var hasNext = effectiveSkip + items.Count < total;

        // Формуємо nextLink (зберігаємо всі поточні фільтри + пересуваємося вперед)
        string? nextLink = null;
        if (hasNext)
        {
            // Якщо користувач прийшов у режимі skip/limit — продовжуємо ним
            if (skip.HasValue || limit.HasValue)
            {
                var nextQ = new Dictionary<string, string?>
                {
                    ["skip"] = (effectiveSkip + pageSize).ToString(),
                    ["limit"] = pageSize.ToString(),
                    ["q"] = q,
                    ["sortBy"] = sortBy,
                    ["sortDir"] = sortDir
                };
                nextLink = PagingHelpers.BuildNextLink(Request, nextQ);
            }
            else
            {
                var nextPage = page + 1;
                var nextQ = new Dictionary<string, string?>
                {
                    ["page"] = nextPage.ToString(),
                    ["pageSize"] = pageSize.ToString(),
                    ["q"] = q,
                    ["sortBy"] = sortBy,
                    ["sortDir"] = sortDir
                };
                nextLink = PagingHelpers.BuildNextLink(Request, nextQ);
            }
        }

        // Для сумісності все одно повертаємо page/pageSize (навіть якщо прийшли як skip/limit)
        var reportedPage = skip.HasValue && pageSize > 0 ? (skip.Value / pageSize) + 1 : page;

        return Ok(new PagedResult<OrganizationDto>(
            Items: items,
            Total: total,
            Page: reportedPage,
            PageSize: pageSize,
            HasNext: hasNext,
            NextLink: nextLink
        ));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrganizationDto>> GetById(int id)
    {
        var o = await _db.Organizations.FindAsync(id);
        return o is null ? NotFound() : new OrganizationDto(o.Id, o.Name, o.Description);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> Create(CreateOrganizationDto dto)
    {
        var o = new Organization { Name = dto.Name, Description = dto.Description };
        _db.Organizations.Add(o);
        await _db.SaveChangesAsync();
        var result = new OrganizationDto(o.Id, o.Name, o.Description);
        return CreatedAtAction(nameof(GetById), new { id = o.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateOrganizationDto dto)
    {
        var o = await _db.Organizations.FindAsync(id);
        if (o is null) return NotFound();
        o.Name = dto.Name;
        o.Description = dto.Description;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var o = await _db.Organizations.FindAsync(id);
        if (o is null) return NotFound();
        _db.Organizations.Remove(o);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
