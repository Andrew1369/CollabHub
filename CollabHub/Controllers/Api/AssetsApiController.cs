using CollabHub.Data;
using CollabHub.Dtos;
using CollabHub.Models;
using CollabHub.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;

namespace CollabHub.Controllers.Api;

[ApiController]
[Route("api/v1/assets")]
public class AssetsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly TelemetryClient _telemetry;
    public AssetsApiController(ApplicationDbContext db, IWebHostEnvironment env, TelemetryClient telemetry)
    {
        _db = db; _env = env; _telemetry = telemetry;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AssetDto>>> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? skip = null,
    [FromQuery] int? limit = null,
    [FromQuery] string? q = null,
    [FromQuery] int? eventId = null,
    [FromQuery] string? sortBy = "UploadedAt",
    [FromQuery] string? sortDir = "desc")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(limit ?? pageSize, 1, 100);

        var queryable = _db.Assets.AsQueryable();

        if (eventId.HasValue)
            queryable = queryable.Where(a => a.EventId == eventId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim();
            queryable = queryable.Where(a =>
                (a.OriginalFileName != null && a.OriginalFileName.Contains(needle)) ||
                (a.ContentType != null && a.ContentType.Contains(needle)) ||
                (a.FilePath != null && a.FilePath.Contains(needle)));
        }

        var total = await queryable.CountAsync();
        queryable = queryable.OrderByDynamic(sortBy, sortDir);

        var effectiveSkip = skip ?? ((page - 1) * pageSize);

        var items = await queryable
            .Skip(effectiveSkip)
            .Take(pageSize)
            .Select(a => new AssetDto(
                a.Id, a.EventId, a.OriginalFileName, a.ContentType, a.FilePath!, a.UploadedAt))
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
                ["eventId"] = eventId?.ToString(),
                ["q"] = q,
                ["sortBy"] = sortBy,
                ["sortDir"] = sortDir
            };
            nextLink = PagingHelpers.BuildNextLink(Request, nextQ);
        }

        var reportedPage = skip.HasValue ? (effectiveSkip / pageSize) + 1 : page;

        return Ok(new PagedResult<AssetDto>(items, total, reportedPage, pageSize, hasNext, nextLink));
    }



    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetDto>> GetById(int id)
    {
        var a = await _db.Assets.FindAsync(id);
        return a is null ? NotFound()
            : new AssetDto(a.Id, a.EventId, a.OriginalFileName, a.ContentType, a.FilePath!, a.UploadedAt);
    }

    // POST /api/v1/assets  (multipart/form-data: fields: eventId, file)
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AssetDto>> Create([FromForm] int eventId, IFormFile file)
    {
        if (!await _db.Events.AnyAsync(e => e.Id == eventId))
            return BadRequest($"Event {eventId} not found");
        if (file is null || file.Length == 0)
            return BadRequest("File is required");

        var uploadsRoot = Environment.GetEnvironmentVariable("UPLOADS_PATH")
            ?? Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var safeName = Path.GetFileName(file.FileName);
        var unique = $"{Guid.NewGuid():N}_{safeName}";
        var full = Path.Combine(uploadsRoot, unique);
        using (var s = System.IO.File.Create(full)) { await file.CopyToAsync(s); }

        var a = new Asset
        {
            EventId = eventId,
            OriginalFileName = safeName,
            ContentType = file.ContentType,
            FilePath = $"/uploads/{unique}",
            UploadedAt = DateTime.UtcNow
        };
        _db.Assets.Add(a);
        await _db.SaveChangesAsync();

        var dto = new AssetDto(a.Id, a.EventId, a.OriginalFileName, a.ContentType, a.FilePath!, a.UploadedAt);

        _telemetry.TrackEvent("AssetUploaded", new Dictionary<string, string>
        {
            ["EventId"] = eventId.ToString(),
            ["FileName"] = safeName,
            ["ContentType"] = file.ContentType ?? ""
        });
        return CreatedAtAction(nameof(GetById), new { id = a.Id }, dto);
    }

    // PUT /api/v1/assets/{id} (можна замінити файл: multipart, поля не обов’язкові)
    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] int eventId, IFormFile? file)
    {
        var a = await _db.Assets.FindAsync(id);
        if (a is null) return NotFound();

        if (!await _db.Events.AnyAsync(e => e.Id == eventId))
            return BadRequest($"Event {eventId} not found");

        a.EventId = eventId;

        if (file != null && file.Length > 0)
        {
            // заміна файлу
            var uploadsRoot = Environment.GetEnvironmentVariable("UPLOADS_PATH")
                ?? Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var safeName = Path.GetFileName(file.FileName);
            var unique = $"{Guid.NewGuid():N}_{safeName}";
            var full = Path.Combine(uploadsRoot, unique);
            using (var s = System.IO.File.Create(full)) { await file.CopyToAsync(s); }

            // видалити старий файл (опційно)
            if (!string.IsNullOrEmpty(a.FilePath))
            {
                var rel = a.FilePath!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var oldFull = Path.Combine(_env.WebRootPath, rel);
                try { if (System.IO.File.Exists(oldFull)) System.IO.File.Delete(oldFull); } catch { }
            }

            a.OriginalFileName = safeName;
            a.ContentType = file.ContentType;
            a.FilePath = $"/uploads/{unique}";
            a.UploadedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Assets.FindAsync(id);
        if (a is null) return NotFound();
        _db.Assets.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
