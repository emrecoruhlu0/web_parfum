using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

public class ProfileController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    private static readonly HashSet<string> ValidSections = new(StringComparer.OrdinalIgnoreCase)
        { "collection", "logs", "reviews" };
    private static readonly HashSet<string> ValidCollectionStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "owned", "wishlist", "tried" };

    // /Profile → kendi profilime redirect
    public IActionResult Index()
    {
        if (!currentUser.IsAuthenticated)
            return RedirectToAction("Login", "Auth");

        return RedirectToAction(nameof(View), new { username = currentUser.Username });
    }

    // /Profile/Search?q=…  → kullanıcı arama sayfası
    [HttpGet]
    public async Task<IActionResult> Search(string? q)
    {
        ViewData["ActivePage"] = "Profile";

        var vm = new UserSearchViewModel { Query = q ?? "" };
        var meId = currentUser.UserId;

        if (!string.IsNullOrWhiteSpace(q) && q.Trim().Length >= 1)
        {
            var s = q.Trim();
            var users = await db.Users
                .Where(u => EF.Functions.ILike(u.Username, $"%{s}%"))
                .OrderBy(u => u.Username)
                .Take(50)
                .Select(u => new UserSearchResult
                {
                    Id = u.Id,
                    Username = u.Username,
                    Bio = u.Bio,
                    IsSelf = meId.HasValue && u.Id == meId.Value,
                })
                .ToListAsync();

            if (meId.HasValue)
            {
                var followingIds = await db.Follows
                    .Where(f => f.FollowerId == meId.Value)
                    .Select(f => f.FollowingId)
                    .ToListAsync();
                foreach (var u in users) u.IsFollowing = followingIds.Contains(u.Id);
            }

            vm.Results = users;
        }

        return View(vm);
    }

    // /Profile/View/{username}
    [HttpGet("Profile/View/{username}")]
    public async Task<IActionResult> View(string username, string? section, string? collectionStatus)
    {
        ViewData["ActivePage"] = "Profile";

        if (string.IsNullOrWhiteSpace(username)) return NotFound();

        var user = await db.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));
        if (user == null) return NotFound();

        var tab = (section ?? "collection").ToLower();
        if (!ValidSections.Contains(tab)) tab = "collection";

        var vm = new ProfileViewModel
        {
            User = user,
            IsOwnProfile = currentUser.UserId == user.Id,
            Section = tab,
            FollowersCount = await db.Follows.CountAsync(f => f.FollowingId == user.Id),
            FollowingCount = await db.Follows.CountAsync(f => f.FollowerId == user.Id),
            ReviewsCount = await db.Reviews.CountAsync(r => r.UserId == user.Id),
            CollectionCount = await db.Collections.CountAsync(c => c.UserId == user.Id),
            DailyLogCount = await db.DailyLogs.CountAsync(d => d.UserId == user.Id),
        };

        if (currentUser.IsAuthenticated && currentUser.UserId != user.Id)
        {
            vm.IsFollowing = await db.Follows
                .AnyAsync(f => f.FollowerId == currentUser.UserId && f.FollowingId == user.Id);
        }

        // Section-specific data
        if (tab == "collection")
        {
            var colStatus = (collectionStatus ?? "owned").ToLower();
            if (!ValidCollectionStatuses.Contains(colStatus)) colStatus = "owned";
            vm.CollectionStatus = colStatus;

            var counts = await db.Collections
                .Where(c => c.UserId == user.Id)
                .GroupBy(c => c.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            vm.OwnedCount = counts.FirstOrDefault(c => c.Key == "owned")?.Count ?? 0;
            vm.WishlistCount = counts.FirstOrDefault(c => c.Key == "wishlist")?.Count ?? 0;
            vm.TriedCount = counts.FirstOrDefault(c => c.Key == "tried")?.Count ?? 0;

            vm.CollectionItems = await db.Collections
                .Where(c => c.UserId == user.Id && c.Status == colStatus)
                .Include(c => c.Perfume)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(50)
                .ToListAsync();
        }
        else if (tab == "logs")
        {
            vm.RecentLogs = await db.DailyLogs
                .Where(d => d.UserId == user.Id)
                .Include(d => d.Perfume)
                .OrderByDescending(d => d.Date)
                .Take(30)
                .ToListAsync();
        }
        else if (tab == "reviews")
        {
            vm.RecentReviews = await db.Reviews
                .Where(r => r.UserId == user.Id)
                .Include(r => r.Perfume)
                .OrderByDescending(r => r.CreatedAt)
                .Take(30)
                .ToListAsync();
        }

        // Aggregation: kullanıcının DailyLog'larındaki parfümlerin top notes ve accord'ları
        var logPerfumes = await db.DailyLogs
            .Where(d => d.UserId == user.Id)
            .Include(d => d.Perfume)
            .Select(d => new
            {
                d.Perfume.TopNotes,
                d.Perfume.Accord1,
                d.Perfume.Accord2,
                d.Perfume.Accord3,
                d.Perfume.Accord4,
                d.Perfume.Accord5,
            })
            .ToListAsync();

        vm.TopNotes = logPerfumes
            .SelectMany(p => (p.TopNotes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(n => n.Trim().ToLower())
            .Where(n => n.Length > 0)
            .GroupBy(n => n)
            .Select(g => new NoteFreq(g.Key, g.Count()))
            .OrderByDescending(n => n.Count)
            .Take(10)
            .ToList();

        vm.TopAccords = logPerfumes
            .SelectMany(p => new[] { p.Accord1, p.Accord2, p.Accord3, p.Accord4, p.Accord5 })
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a!.Trim().ToLower())
            .GroupBy(a => a)
            .Select(g => new NoteFreq(g.Key, g.Count()))
            .OrderByDescending(n => n.Count)
            .Take(10)
            .ToList();

        return base.View(vm);
    }
}
