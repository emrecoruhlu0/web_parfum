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
    public async Task<IActionResult> Index(PerfumeIndexViewModel filter)
    {
        ViewData["PublicCatalog"] = true;
        ViewData["Title"] = "Katalog";
        ViewData["ContentWidth"] = "wide";

        filter.Brand = filter.Brand.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        filter.Gender = filter.Gender.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        filter.Accord = filter.Accord.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

        var query = db.Perfumes.AsQueryable().ApplySearch(filter.Search);

        if (filter.Brand.Count > 0)
            query = query.Where(p => filter.Brand.Contains(p.Brand));

        if (filter.Gender.Count > 0)
            query = query.Where(p => p.Gender != null && filter.Gender.Contains(p.Gender));

        if (filter.Accord.Count > 0)
        {
            var accords = filter.Accord;
            query = query.Where(p =>
                (p.Accord1 != null && accords.Contains(p.Accord1)) ||
                (p.Accord2 != null && accords.Contains(p.Accord2)) ||
                (p.Accord3 != null && accords.Contains(p.Accord3)) ||
                (p.Accord4 != null && accords.Contains(p.Accord4)) ||
                (p.Accord5 != null && accords.Contains(p.Accord5)));
        }

        if (filter.Year.HasValue)
            query = query.Where(p => p.Year == filter.Year);

        query = filter.Sort switch
        {
            "newest" => query.OrderByDescending(p => p.Year ?? 0).ThenByDescending(p => p.Id),
            "rating" => query.OrderByDescending(p => p.RatingValue ?? 0).ThenByDescending(p => p.RatingCount ?? 0),
            _ => query.OrderByDescending(p => p.RatingCount ?? 0).ThenByDescending(p => p.RatingValue ?? 0),
        };

        filter.TotalCount = await query.CountAsync();
        filter.Page = Math.Max(1, filter.Page);

        filter.Perfumes = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var brandRows = await db.Perfumes
            .GroupBy(p => p.Brand)
            .Select(g => new { Value = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        filter.Brands = brandRows.Select(b => new FacetItem(b.Value, b.Count)).ToList();

        var genderRows = await db.Perfumes
            .Where(p => p.Gender != null)
            .GroupBy(p => p.Gender!)
            .Select(g => new { Value = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        filter.Genders = genderRows.Select(g => new FacetItem(g.Value, g.Count)).ToList();

        var accordRows = await db.Perfumes
            .Select(p => new { p.Accord1, p.Accord2, p.Accord3, p.Accord4, p.Accord5 })
            .ToListAsync();

        filter.Accords = accordRows
            .SelectMany(r => new[] { r.Accord1, r.Accord2, r.Accord3, r.Accord4, r.Accord5 })
            .Where(a => !string.IsNullOrEmpty(a))
            .GroupBy(a => a!)
            .Select(g => new FacetItem(g.Key, g.Count()))
            .OrderByDescending(f => f.Count)
            .ToList();

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_KatalogResults", filter);

        return View(filter);
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
