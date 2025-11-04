using System.Text;
using CollabHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class SitemapController : Controller
{
    private readonly ApplicationDbContext _db;
    public SitemapController(ApplicationDbContext db) => _db = db;

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        sb.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");

        void Add(string path, DateTime? lastmod = null)
        {
            sb.AppendLine("<url>");
            sb.AppendLine($"<loc>{baseUrl}{path}</loc>");
            if (lastmod.HasValue) sb.AppendLine($"<lastmod>{lastmod.Value:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("</url>");
        }

        Add("/");
        Add("/Events");
        Add("/Venues");
        Add("/Organizations");

        var events = await _db.Events
            .AsNoTracking()
            .Select(e => new { e.Id, e.StartsAt })
            .ToListAsync();

        foreach (var e in events)
            Add($"/Events/Details/{e.Id}", e.StartsAt);

        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}
