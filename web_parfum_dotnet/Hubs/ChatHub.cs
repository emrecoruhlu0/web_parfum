using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebParfum.Services;

namespace WebParfum.Hubs;

// Her giriş yapmış kullanıcı kendi UserId'sine ait bir gruba eklenir.
// Mesaj gönderilince alıcının grubuna anlık olarak push edilir.
[Authorize]
public class ChatHub(ICurrentUserService currentUser) : Hub
{
    // UserId bazlı grup adı — istemciye özel mesaj göndermek için kullanılır.
    public static string GroupFor(int userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = currentUser.UserId;
        if (userId.HasValue)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId.Value));

        await base.OnConnectedAsync();
    }
}
