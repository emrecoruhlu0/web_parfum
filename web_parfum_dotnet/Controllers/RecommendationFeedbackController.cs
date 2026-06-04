using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;

namespace WebParfum.Controllers;

[Authorize]
public class RecommendationFeedbackController(
    AppDbContext db,
    ICurrentUserService currentUser) : Controller
{
    /// <summary>
    /// Öneri kartında "beğendim/beğenmedim" sinyali. AJAX (JSON) ve form POST'u destekler.
    /// Aynı butona tekrar basmak geri bildirimi geri alır (toggle).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int perfumeId, bool liked)
    {
        var userId = currentUser.UserId!.Value;
        if (!await db.Perfumes.AnyAsync(p => p.Id == perfumeId)) return NotFound();

        var existing = await db.RecommendationFeedbacks
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PerfumeId == perfumeId);

        // null = geri bildirim yok, true = beğendim, false = beğenmedim
        bool? state;
        if (existing == null)
        {
            db.RecommendationFeedbacks.Add(new RecommendationFeedback
            {
                UserId = userId,
                PerfumeId = perfumeId,
                Liked = liked,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
            state = liked;
        }
        else if (existing.Liked == liked)
        {
            // Aynı butona tekrar basıldı → geri bildirimi kaldır
            db.RecommendationFeedbacks.Remove(existing);
            state = null;
        }
        else
        {
            // Karşıt butona basıldı → güncelle
            existing.Liked = liked;
            existing.UpdatedAt = DateTime.UtcNow;
            state = liked;
        }

        await db.SaveChangesAsync();

        if (Request.Headers.Accept.ToString().Contains("application/json")
            || Request.Headers["X-Requested-With"] == "fetch")
        {
            return Json(new { state });
        }

        return RedirectToAction("Index", "Feed");
    }
}
