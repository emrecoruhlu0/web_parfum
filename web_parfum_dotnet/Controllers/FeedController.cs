using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

[Authorize]
public class FeedController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Feed";
        var userId = currentUser.UserId!.Value;

        var followingIds = await db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var vm = new FeedViewModel
        {
            HasFollows = followingIds.Count > 0,
        };

        if (followingIds.Count > 0)
        {
            var reviews = await db.Reviews
                .Where(r => followingIds.Contains(r.UserId))
                .Include(r => r.User)
                .Include(r => r.Perfume)
                .OrderByDescending(r => r.CreatedAt)
                .Take(30)
                .ToListAsync();

            var logs = await db.DailyLogs
                .Where(d => followingIds.Contains(d.UserId))
                .Include(d => d.User)
                .Include(d => d.Perfume)
                .OrderByDescending(d => d.CreatedAt)
                .Take(30)
                .ToListAsync();

            vm.Items = reviews.Select(r => new FeedItem
            {
                Type = "review",
                CreatedAt = r.CreatedAt,
                Actor = r.User,
                Perfume = r.Perfume,
                Rating = r.Rating,
                Body = r.Body,
            }).Concat(logs.Select(d => new FeedItem
            {
                Type = "log",
                CreatedAt = d.CreatedAt,
                Actor = d.User,
                Perfume = d.Perfume,
                LogDate = d.Date,
                Sprays = d.Sprays,
                Note = d.Note,
            }))
            .OrderByDescending(i => i.CreatedAt)
            .Take(30)
            .ToList();
        }

        // Fallback: hiç follow yoksa veya feed boşsa, en çok rated parfümleri göster
        if (vm.Items.Count == 0)
        {
            vm.FallbackPopular = await db.Perfumes
                .OrderByDescending(p => p.RatingCount ?? 0)
                .ThenByDescending(p => p.RatingValue ?? 0)
                .Take(20)
                .ToListAsync();
        }

        return View(vm);
    }
}
