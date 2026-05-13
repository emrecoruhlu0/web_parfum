using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;

namespace WebParfum.Controllers;

[Authorize]
public class FollowsController(
    AppDbContext db,
    ICurrentUserService currentUser,
    INotificationService notif) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string username)
    {
        var followerId = currentUser.UserId!.Value;

        var target = await db.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));
        if (target == null) return NotFound();
        if (target.Id == followerId)
        {
            TempData["Error"] = "Kendini takip edemezsin.";
            return RedirectToAction("View", "Profile", new { username });
        }

        var existing = await db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == target.Id);

        if (existing != null)
        {
            db.Follows.Remove(existing);
            await db.SaveChangesAsync();
        }
        else
        {
            db.Follows.Add(new Follow
            {
                FollowerId = followerId,
                FollowingId = target.Id,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();

            // Bildirim — fail-soft
            try { await notif.CreateAsync(target.Id, followerId, "follow"); }
            catch { /* ignore */ }
        }

        return RedirectToAction("View", "Profile", new { username });
    }
}
