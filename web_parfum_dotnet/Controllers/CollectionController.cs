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
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PerfumeId == perfumeId && c.Status == status);

        if (existing != null)
        {
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

    // Aynı parfümün bir status'ünü açıp kapatmak için — Details sayfasındaki
    // checkbox tipi toggle formları bunu çağırır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int perfumeId, string status, string? returnUrl)
    {
        var userId = currentUser.UserId!.Value;
        status = (status ?? "").ToLower();
        if (!ValidStatuses.Contains(status))
        {
            TempData["Error"] = "Geçersiz durum.";
            return RedirectBack(returnUrl, perfumeId);
        }

        if (!await db.Perfumes.AnyAsync(p => p.Id == perfumeId))
            return NotFound();

        var existing = await db.Collections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PerfumeId == perfumeId && c.Status == status);

        if (existing != null)
        {
            db.Collections.Remove(existing);
        }
        else
        {
            db.Collections.Add(new Collection
            {
                UserId = userId,
                PerfumeId = perfumeId,
                Status = status,
                BottleLevel = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();
        return RedirectBack(returnUrl, perfumeId);
    }

    private IActionResult RedirectBack(string? returnUrl, int perfumeId)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBottleLevel(int perfumeId, int bottleLevel)
    {
        var userId = currentUser.UserId!.Value;
        // Bottle level kavramı sadece "owned" için anlamlı.
        var item = await db.Collections
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PerfumeId == perfumeId && c.Status == "owned");
        if (item == null) return NotFound();

        item.BottleLevel = Math.Clamp(bottleLevel, 0, 100);
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { status = item.Status });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int perfumeId, string? status)
    {
        var userId = currentUser.UserId!.Value;
        var query = db.Collections.Where(c => c.UserId == userId && c.PerfumeId == perfumeId);
        if (!string.IsNullOrEmpty(status))
        {
            var s = status.ToLower();
            query = query.Where(c => c.Status == s);
        }

        var items = await query.ToListAsync();
        if (items.Count == 0) return NotFound();

        var returnStatus = items[0].Status;
        db.Collections.RemoveRange(items);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { status = returnStatus });
    }
}
