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

    // /Profile → kendi profilime redirect
    public IActionResult Index()
    {
        if (!currentUser.IsAuthenticated)
            return RedirectToAction("Login", "Auth");

        return RedirectToAction(nameof(View), new { username = currentUser.Username });
    }

    // /Profile/View/{username}
    [HttpGet("Profile/View/{username}")]
    public async Task<IActionResult> View(string username, string? section)
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
            vm.CollectionItems = await db.Collections
                .Where(c => c.UserId == user.Id)
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
