using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Helpers;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

/// <summary>
/// Giriş gerektirmeyen salt okunur parfüm kataloğu: arama ve inceleme.
/// </summary>
[AllowAnonymous]
public class KatalogController(AppDbContext db, UserTasteProfileService tasteProfile) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? sort, int page = 1)
    {
        ViewData["PublicCatalog"] = true;
        ViewData["Title"] = "Katalog";
        ViewData["ContentWidth"] = "wide";

        const int pageSize = 24;
        page = Math.Max(1, page);
        sort = sort is "newest" or "rating" or "popular" ? sort : "popular";

        var query = db.Perfumes.AsQueryable().ApplySearch(search);
        query = sort switch
        {
            "newest" => query.OrderByDescending(p => p.Year ?? 0).ThenByDescending(p => p.Id),
            "rating" => query.OrderByDescending(p => p.RatingValue ?? 0).ThenByDescending(p => p.RatingCount ?? 0),
            _ => query.OrderByDescending(p => p.RatingCount ?? 0).ThenByDescending(p => p.RatingValue ?? 0),
        };

        var total = await query.CountAsync();
        var perfumes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vm = new PerfumeIndexViewModel
        {
            Search = search,
            Sort = sort,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Perfumes = perfumes,
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["PublicCatalog"] = true;
        ViewData["ContentWidth"] = "wide";

        var perfume = await db.Perfumes.FirstOrDefaultAsync(p => p.Id == id);
        if (perfume == null) return NotFound();

        ViewData["Title"] = perfume.Name;

        var reviews = await db.Reviews
            .Include(r => r.User)
            .Where(r => r.PerfumeId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        var vm = new PerfumeDetailViewModel
        {
            Perfume = perfume,
            Reviews = reviews,
            LikeCount = await db.Likes.CountAsync(l => l.PerfumeId == id),
            CollectionCount = await db.Collections.CountAsync(c => c.PerfumeId == id),
            SimilarPerfumes = await tasteProfile.FindSimilarAsync(id, take: 6),
        };

        return View(vm);
    }
}
