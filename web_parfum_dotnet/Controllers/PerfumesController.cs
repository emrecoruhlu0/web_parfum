using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Helpers;
using WebParfum.Models;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

public class PerfumesController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    public async Task<IActionResult> Index(PerfumeIndexViewModel filter)
    {
        ViewData["ActivePage"] = "Discover";

        var query = db.Perfumes.AsQueryable().ApplySearch(filter.Search);

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand == filter.Brand);

        if (!string.IsNullOrWhiteSpace(filter.Gender))
            query = query.Where(p => p.Gender == filter.Gender);

        if (!string.IsNullOrWhiteSpace(filter.Accord))
        {
            foreach (var a in filter.Accord
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()))
            {
                var accord = a;
                query = query.Where(p =>
                    p.Accord1 == accord || p.Accord2 == accord ||
                    p.Accord3 == accord || p.Accord4 == accord ||
                    p.Accord5 == accord);
            }
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

        // Meta facets — chip filtreler her sayfada görünmeli
        {
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

            // Accord aggregation: 5 kolonu C# tarafında union
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
        }

        // AJAX (Keşfet sayfasında akıcı arama): sadece grid partial'ı dön — focus korunur.
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_PerfumeGrid", filter);

        return View(filter);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Discover";

        var perfume = await db.Perfumes.FirstOrDefaultAsync(p => p.Id == id);
        if (perfume == null) return NotFound();

        var reviews = await db.Reviews
            .Include(r => r.User)
            .Where(r => r.PerfumeId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        var likeCount = await db.Likes.CountAsync(l => l.PerfumeId == id);
        var collectionCount = await db.Collections.CountAsync(c => c.PerfumeId == id);

        var vm = new PerfumeDetailViewModel
        {
            Perfume = perfume,
            Reviews = reviews,
            LikeCount = likeCount,
            CollectionCount = collectionCount,
        };

        if (currentUser.IsAuthenticated && currentUser.UserId is int userId)
        {
            vm.IsLiked = await db.Likes.AnyAsync(l => l.PerfumeId == id && l.UserId == userId);
            var statuses = await db.Collections
                .Where(c => c.PerfumeId == id && c.UserId == userId)
                .Select(c => c.Status)
                .ToListAsync();
            vm.UserCollectionStatuses = new HashSet<string>(statuses, StringComparer.OrdinalIgnoreCase);
            vm.UserReview = await db.Reviews
                .FirstOrDefaultAsync(r => r.PerfumeId == id && r.UserId == userId);
        }

        return View(vm);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Discover";
        return View(new PerfumeCreateViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PerfumeCreateViewModel model)
    {
        ViewData["ActivePage"] = "Discover";
        if (!ModelState.IsValid) return View(model);

        var perfume = new Perfume
        {
            Name = model.Name.Trim(),
            Brand = model.Brand.Trim(),
            Country = model.Country?.Trim(),
            Gender = model.Gender?.Trim(),
            Year = model.Year,
            TopNotes = model.TopNotes?.Trim(),
            MiddleNotes = model.MiddleNotes?.Trim(),
            BaseNotes = model.BaseNotes?.Trim(),
            Accord1 = model.Accord1?.Trim(),
            Accord2 = model.Accord2?.Trim(),
            Accord3 = model.Accord3?.Trim(),
            Accord4 = model.Accord4?.Trim(),
            Accord5 = model.Accord5?.Trim(),
            ImageUrl = model.ImageUrl,
            CreatedAt = DateTime.UtcNow,
        };

        db.Perfumes.Add(perfume);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = perfume.Id });
    }

    // Autocomplete endpoint — kullanıldığı yerler: DailyLog modal, Community post,
    // Feed quick share. pg_trgm + unaccent destekli SearchKey üzerinden tipo toleranslı,
    // kelime sırasından bağımsız arama. Notes/Accord'ları da kapsar; Name+Brand öncelikli sıralanır.
    [HttpGet]
    public async Task<IActionResult> Search(string? q, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(Array.Empty<object>());

        var normalized = SearchNormalizer.Normalize(q);
        if (normalized.Length == 0) return Json(Array.Empty<object>());

        limit = Math.Clamp(limit, 1, 25);

        var prefix = normalized + "%";
        var results = await db.Perfumes
            .AsQueryable()
            .ApplySearch(q)
            .Select(p => new
            {
                p.Id, p.Name, p.Brand, p.ImageUrl, p.RatingCount,
                ExactName  = p.SearchKeyName == normalized,
                PrefixName = EF.Functions.ILike(p.SearchKeyName!, prefix),
                WSimName   = EF.Functions.TrigramsWordSimilarity(normalized, p.SearchKeyName!),
                WSimAll    = EF.Functions.TrigramsWordSimilarity(normalized, p.SearchKey!),
            })
            .OrderByDescending(x => x.ExactName)
            .ThenByDescending(x => x.PrefixName)
            .ThenByDescending(x => x.WSimName)
            .ThenByDescending(x => x.WSimAll)
            .ThenByDescending(x => x.RatingCount ?? 0)
            .Take(limit)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                brand = x.Brand,
                imageUrl = x.ImageUrl,
            })
            .ToListAsync();

        return Json(results);
    }
}
