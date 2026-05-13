using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;

namespace WebParfum.Controllers;

[Authorize]
public class LikesController(
    AppDbContext db,
    ICurrentUserService currentUser,
    INotificationService notif) : Controller
{
    /// <summary>
    /// AJAX endpoint — JSON döner. Form POST'la da çalışır (Accept header kontrolü).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int perfumeId)
    {
        var userId = currentUser.UserId!.Value;
        if (!await db.Perfumes.AnyAsync(p => p.Id == perfumeId)) return NotFound();

        var existing = await db.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PerfumeId == perfumeId);

        bool liked;
        if (existing != null)
        {
            db.Likes.Remove(existing);
            liked = false;
        }
        else
        {
            db.Likes.Add(new Like
            {
                UserId = userId,
                PerfumeId = perfumeId,
                CreatedAt = DateTime.UtcNow,
            });
            liked = true;
        }
        await db.SaveChangesAsync();

        // Notification: sadece yeni like'ta, parfümü ekleyen kullanıcı kendisi değilse
        // (Bu projede parfümlerin sahibi yok — seed verisi. Skip.)

        var count = await db.Likes.CountAsync(l => l.PerfumeId == perfumeId);

        // AJAX isteğiyse JSON, değilse redirect
        if (Request.Headers.Accept.ToString().Contains("application/json")
            || Request.Headers["X-Requested-With"] == "fetch")
        {
            return Json(new { liked, count });
        }

        return RedirectToAction("Details", "Perfumes", new { id = perfumeId });
    }

    /// <summary>
    /// Kullanıcının beğendiği parfümlerin listesi.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Likes";
        var userId = currentUser.UserId!.Value;

        var likes = await db.Likes
            .Where(l => l.UserId == userId)
            .Include(l => l.Perfume)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => l.Perfume)
            .ToListAsync();

        return View(likes);
    }
}
