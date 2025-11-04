using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CollabHub.Data;
using CollabHub.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using System.IO;
using System.Text;
using CollabHub.Utils;
using Microsoft.AspNetCore.Authorization;

namespace CollabHub.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TelemetryClient _telemetry;

        public EventsController(ApplicationDbContext context, TelemetryClient telemetry)
        {
            _context = context;
            _telemetry = telemetry;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var query = _context.Events.Include(e => e.Venue);
            return View(await query.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Assets)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            // кастомний івент
            _telemetry.TrackEvent("EventDetailsViewed", new Dictionary<string, string>
            {
                ["EventId"] = ev.Id.ToString(),
                ["Title"] = ev.Title ?? "",
                ["VenueId"] = ev.VenueId.ToString()
            });

            return View(ev);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VenueId,Title,Description,StartsAt,EndsAt,Capacity,ImageUrl")] Event model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", model.VenueId);
            return View(model);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Events == null) return NotFound();

            var model = await _context.Events.FindAsync(id);
            if (model == null) return NotFound();
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", model.VenueId);
            return View(model);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,VenueId,Title,Description,StartsAt,EndsAt,Capacity,ImageUrl")] Event model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", model.VenueId);
            return View(model);
        }

        // GET: Events/DownloadIcs/5
        public async Task<IActionResult> DownloadIcs(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            var ics = CalendarLinkHelpers.BuildIcsContent(ev);
            var bytes = Encoding.UTF8.GetBytes(ics);

            var safeTitle = SanitizeFileName(ev.Title);
            var fileName = $"event-{ev.Id}-{safeTitle}.ics";

            return File(bytes, "text/calendar", fileName);
        }

        private static string SanitizeFileName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "event";
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null) return NotFound();

            var model = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null) return Problem("Entity set 'ApplicationDbContext.Events' is null.");

            var model = await _context.Events.FindAsync(id);
            if (model != null) _context.Events.Remove(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id) =>
            (_context.Events?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
