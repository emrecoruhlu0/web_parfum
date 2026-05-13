using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

[Authorize]
public class CollectionController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "owned", "wishlist", "tried" };

    public async Task<IActionResult> Index(string? status)
    {
        ViewData["ActivePage"] = "Collection";
        var userId = currentUser.UserId!.Value;

        var tab = (status ?? "owned").ToLower();
        if (!ValidStatuses.Contains(tab)) tab = "owned";

        var all = db.Collections.Where(c => c.UserId == userId);

        var counts = await all
            .GroupBy(c => c.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var vm = new CollectionIndexViewModel
        {
            Tab = tab,
            OwnedCount = counts.FirstOrDefault(c => c.Key == "owned")?.Count ?? 0,
            WishlistCount = counts.FirstOrDefault(c => c.Key == "wishlist")?.Count ?? 0,
            TriedCount = counts.FirstOrDefault(c => c.Key == "tried")?.Count ?? 0,
            Items = await all
                .Where(c => c.Status == tab)
                .Include(c => c.Perfume)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync(),
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int perfumeId, string status, int? bottleLevel)
    {
        var userId = currentUser.UserId!.Value;
        status = (status ?? "owned").ToLower();
        if (!ValidStatuses.Contains(status))
        {
            TempData["Error"] = "Geçersiz durum.";
            return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
        }

        if (!await db.Perfumes.AnyAsync(p => p.Id == perfumeId))
            return NotFound();

        var existing = await db.Collections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PerfumeId == perfumeId);

        if (existing != null)
        {
            existing.Status = status;
            if (bottleLevel.HasValue) existing.BottleLevel = Math.Clamp(bottleLevel.Value, 0, 100);
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.Collections.Add(new Collection
            {
                UserId = userId,
                PerfumeId = perfumeId,
                Status = status,
                BottleLevel = Math.Clamp(bottleLevel ?? 100, 0, 100),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { status });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBottleLevel(int perfumeId, int bottleLevel)
    {
        var userId = currentUser.UserId!.Value;
        var item = await db.Collections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PerfumeId == perfumeId);
        if (item == null) return NotFound();

        item.BottleLevel = Math.Clamp(bottleLevel, 0, 100);
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { status = item.Status });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int perfumeId)
    {
        var userId = currentUser.UserId!.Value;
        var item = await db.Collections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PerfumeId == perfumeId);
        if (item == null) return NotFound();

        var status = item.Status;
        db.Collections.Remove(item);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { status });
    }
}
