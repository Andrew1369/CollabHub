using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CollabHub.Data;
using CollabHub.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CollabHub.Controllers
{
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AssetsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Assets
        public async Task<IActionResult> Index()
        {
            var assets = _context.Assets.Include(a => a.Event);
            return View(await assets.ToListAsync());
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _context.Assets
                .Include(a => a.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title");
            return View();
        }

        // POST: Assets/Create  (приймаємо файл)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId")] Asset asset, IFormFile? file)
        {
            if (asset.EventId == 0)
                ModelState.AddModelError(nameof(Asset.EventId), "Event is required");

            if (file == null || file.Length == 0)
                ModelState.AddModelError(nameof(Asset.FilePath), "Please choose a file");

            if (!ModelState.IsValid)
            {
                ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", asset.EventId);
                return View(asset);
            }

            var uploadsRoot = Environment.GetEnvironmentVariable("UPLOADS_PATH") ?? Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var safeName = Path.GetFileName(file!.FileName);
            var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
            var fullPath = Path.Combine(uploadsRoot, uniqueName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            asset.FilePath = $"/uploads/{uniqueName}";
            asset.OriginalFileName = safeName;
            asset.ContentType = file.ContentType;
            asset.UploadedAt = DateTime.UtcNow;

            _context.Add(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", asset.EventId);
            return View(asset);
        }

        // POST: Assets/Edit/5  (дозволимо заміну файлу)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventId")] Asset input, IFormFile? file)
        {
            if (id != input.Id) return NotFound();

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();

            if (input.EventId == 0)
                ModelState.AddModelError(nameof(Asset.EventId), "Event is required");

            if (!ModelState.IsValid)
            {
                ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", input.EventId);
                // повертаємо поточний asset у в'юшку (а не input), щоб показати існуючий файл
                asset.EventId = input.EventId;
                return View(asset);
            }

            asset.EventId = input.EventId;

            if (file != null && file.Length > 0)
            {
                // видалимо старий файл (опційно)
                if (!string.IsNullOrEmpty(asset.FilePath))
                {
                    var old = asset.FilePath.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar);
                    var oldFull = Path.Combine(_env.WebRootPath, old);
                    if (System.IO.File.Exists(oldFull))
                        System.IO.File.Delete(oldFull);
                }

                var uploadsRoot = Environment.GetEnvironmentVariable("UPLOADS_PATH") ?? Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsRoot);

                var safeName = Path.GetFileName(file.FileName);
                var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
                var fullPath = Path.Combine(uploadsRoot, uniqueName);

                using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }

                asset.FilePath = $"/uploads/{uniqueName}";
                asset.OriginalFileName = safeName;
                asset.ContentType = file.ContentType;
                asset.UploadedAt = DateTime.UtcNow;
            }

            _context.Update(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var asset = await _context.Assets
                .Include(a => a.Event)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                if (!string.IsNullOrEmpty(asset.FilePath))
                {
                    var rel = asset.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var full = Path.Combine(_env.WebRootPath, rel);
                    if (System.IO.File.Exists(full))
                        System.IO.File.Delete(full);
                }

                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
