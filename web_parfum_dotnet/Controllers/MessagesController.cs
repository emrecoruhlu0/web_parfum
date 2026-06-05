using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Hubs;
using WebParfum.Models;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

[Authorize]
public class MessagesController(
    AppDbContext db,
    ICurrentUserService currentUser,
    IHubContext<ChatHub> chatHub) : Controller
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
        // AJAX (fetch) isteği mi? JS varsa JSON, yoksa klasik redirect döndürürüz.
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (string.IsNullOrWhiteSpace(body))
        {
            if (isAjax) return BadRequest(new { error = "Boş mesaj gönderilemez." });
            TempData["Error"] = "Boş mesaj gönderilemez.";
            return RedirectToAction(nameof(Conversation), new { username });
        }

        var recipient = await db.Users
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));
        if (recipient == null) return NotFound();
        if (recipient.Id == userId)
        {
            if (isAjax) return BadRequest(new { error = "Kendine mesaj gönderemezsin." });
            TempData["Error"] = "Kendine mesaj gönderemezsin.";
            return RedirectToAction(nameof(Index));
        }

        var message = new Message
        {
            SenderId = userId,
            RecipientId = recipient.Id,
            Body = body.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };
        db.Messages.Add(message);
        await db.SaveChangesAsync();

        // Anlık iletim: alıcının ve gönderenin (diğer açık sekmeleri için) gruplarına push et.
        var senderUsername = currentUser.Username;
        var payload = new
        {
            id = message.Id,
            senderId = message.SenderId,
            senderUsername,
            recipientId = message.RecipientId,
            body = message.Body,
            createdAt = message.CreatedAt,
        };
        await chatHub.Clients
            .Groups(ChatHub.GroupFor(recipient.Id), ChatHub.GroupFor(userId))
            .SendAsync("ReceiveMessage", payload);

        if (isAjax) return Ok(payload);
        return RedirectToAction(nameof(Conversation), new { username = recipient.Username });
    }
}
