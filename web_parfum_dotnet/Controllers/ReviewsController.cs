using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;

namespace WebParfum.Controllers;

[Authorize]
public class ReviewsController(
    AppDbContext db,
    ICurrentUserService currentUser,
    INotificationService notif) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int perfumeId, int rating, string? body)
    {
        var userId = currentUser.UserId!.Value;
        if (rating is < 1 or > 5)
        {
            TempData["Error"] = "Puan 1 ile 5 arasında olmalı.";
            return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
        }

        if (!await db.Perfumes.AnyAsync(p => p.Id == perfumeId)) return NotFound();

        if (await db.Reviews.AnyAsync(r => r.UserId == userId && r.PerfumeId == perfumeId))
        {
            TempData["Error"] = "Bu parfüm için zaten yorum yazmışsın.";
            return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
        }

        using var tx = await db.Database.BeginTransactionAsync();

        var review = new Review
        {
            UserId = userId,
            PerfumeId = perfumeId,
            Rating = rating,
            Body = string.IsNullOrWhiteSpace(body) ? null : body.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        // Perfume rating'ini yeniden hesapla
        var stats = await db.Reviews
            .Where(r => r.PerfumeId == perfumeId)
            .GroupBy(r => r.PerfumeId)
            .Select(g => new { Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync();

        var perfume = await db.Perfumes.FirstAsync(p => p.Id == perfumeId);
        if (stats != null)
        {
            perfume.RatingValue = Math.Round(stats.Avg, 2);
            perfume.RatingCount = stats.Count;
        }

        // Notification side-effect: bu parfümü "owned" olarak işaretlemiş son 10 kullanıcıya bildirim
        var ownerIds = await db.Collections
            .Where(c => c.PerfumeId == perfumeId
                     && c.Status == "owned"
                     && c.UserId != userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(10)
            .Select(c => c.UserId)
            .ToListAsync();

        foreach (var ownerId in ownerIds)
        {
            await notif.CreateAsync(ownerId, userId, "review", perfumeId, saveImmediately: false);
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = currentUser.UserId!.Value;
        var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return NotFound();
        if (review.UserId != userId) return Forbid();

        var perfumeId = review.PerfumeId;

        using var tx = await db.Database.BeginTransactionAsync();
        db.Reviews.Remove(review);
        await db.SaveChangesAsync();

        // Rating yeniden hesapla
        var stats = await db.Reviews
            .Where(r => r.PerfumeId == perfumeId)
            .GroupBy(r => r.PerfumeId)
            .Select(g => new { Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync();

        var perfume = await db.Perfumes.FirstAsync(p => p.Id == perfumeId);
        if (stats != null)
        {
            perfume.RatingValue = Math.Round(stats.Avg, 2);
            perfume.RatingCount = stats.Count;
        }
        else
        {
            perfume.RatingValue = null;
            perfume.RatingCount = 0;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
    }
}
