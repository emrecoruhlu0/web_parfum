using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Services;

namespace WebParfum.Filters;

/// <summary>
/// Layout sidebar'ındaki bildirim badge'i için her authenticated request'te
/// ViewData["UnreadCount"] değerini set eder.
/// </summary>
public class UnreadCountFilter(
    AppDbContext db,
    ICurrentUserService currentUser,
    UserTasteProfileService tasteService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (currentUser.IsAuthenticated && currentUser.UserId is int userId
            && context.Controller is Controller mvcController)
        {
            var count = await db.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);

            mvcController.ViewData["UnreadCount"] = count;
            mvcController.ViewData["SillageColor"] = await tasteService.GetSillageColorAsync(userId);
        }

        await next();
    }
}
