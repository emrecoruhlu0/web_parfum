using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;

namespace WebParfum.Controllers;

[Authorize]
public class FeedController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Feed";

        var recentPerfumes = await db.Perfumes
            .OrderByDescending(p => p.RatingCount)
            .Take(20)
            .ToListAsync();

        return View(recentPerfumes);
    }
}
