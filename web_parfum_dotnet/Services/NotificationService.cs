using WebParfum.Data;
using WebParfum.Models;

namespace WebParfum.Services;

public interface INotificationService
{
    Task CreateAsync(int recipientId, int actorId, string type,
        int? perfumeId = null, int? communityId = null, bool saveImmediately = true);
}

public class NotificationService(AppDbContext db) : INotificationService
{
    public async Task CreateAsync(int recipientId, int actorId, string type,
        int? perfumeId = null, int? communityId = null, bool saveImmediately = true)
    {
        if (recipientId == actorId) return;

        db.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            ActorId = actorId,
            Type = type,
            PerfumeId = perfumeId,
            CommunityId = communityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        if (saveImmediately)
            await db.SaveChangesAsync();
    }
}
