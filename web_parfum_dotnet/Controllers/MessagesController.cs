using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

[Authorize]
public class MessagesController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Messages";
        var userId = currentUser.UserId!.Value;

        // Tüm mesajları çek (ben sender veya recipient olduğum), karşı tarafa göre grupla
        var msgs = await db.Messages
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var conversations = msgs
            .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Select(g =>
            {
                var last = g.First(); // OrderByDescending uygulanmıştı
                var other = last.SenderId == userId ? last.Recipient : last.Sender;
                var unread = g.Count(m => m.RecipientId == userId && !m.IsRead);
                return new ConversationSummary
                {
                    OtherUser = other,
                    LastMessage = last,
                    UnreadCount = unread,
                };
            })
            .OrderByDescending(c => c.LastMessage.CreatedAt)
            .ToList();

        return View(new MessagesIndexViewModel { Conversations = conversations });
    }

    [HttpGet("Messages/Conversation/{username}")]
    public async Task<IActionResult> Conversation(string username)
    {
        ViewData["ActivePage"] = "Messages";
        var userId = currentUser.UserId!.Value;

        var other = await db.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));
        if (other == null) return NotFound();
        if (other.Id == userId) return RedirectToAction(nameof(Index));

        var messages = await db.Messages
            .Where(m =>
                (m.SenderId == userId && m.RecipientId == other.Id) ||
                (m.SenderId == other.Id && m.RecipientId == userId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // Karşı taraftan gelen okunmamışları okundu işaretle
        var unread = messages.Where(m => m.RecipientId == userId && !m.IsRead).ToList();
        if (unread.Count > 0)
        {
            foreach (var m in unread) m.IsRead = true;
            await db.SaveChangesAsync();
        }

        return View(new ConversationViewModel
        {
            OtherUser = other,
            Messages = messages,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string username, string body)
    {
        var userId = currentUser.UserId!.Value;

        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Boş mesaj gönderilemez.";
            return RedirectToAction(nameof(Conversation), new { username });
        }

        var recipient = await db.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));
        if (recipient == null) return NotFound();
        if (recipient.Id == userId)
        {
            TempData["Error"] = "Kendine mesaj gönderemezsin.";
            return RedirectToAction(nameof(Index));
        }

        db.Messages.Add(new Message
        {
            SenderId = userId,
            RecipientId = recipient.Id,
            Body = body.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Conversation), new { username = recipient.Username });
    }
}
