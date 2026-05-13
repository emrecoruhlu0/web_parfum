using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Services;

namespace WebParfum.Controllers;

[Authorize]
public class NotificationsController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        ViewData["ActivePage"] = "Notifications";
        var userId = currentUser.UserId!.Value;
        page = Math.Max(1, page);
        const int pageSize = 30;

        var notifications = await db.Notifications
            .Where(n => n.RecipientId == userId)
            .Include(n => n.Actor)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.HasMore = notifications.Count == pageSize;

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = currentUser.UserId!.Value;
        var notif = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        if (notif == null) return NotFound();
        if (notif.RecipientId != userId) return Forbid();

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = currentUser.UserId!.Value;
        var unread = await db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
