using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

[Authorize]
public class DailyLogController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    public async Task<IActionResult> Index(int? year, int? month)
    {
        ViewData["ActivePage"] = "DailyLog";
        var userId = currentUser.UserId!.Value;

        var today = DateTime.Today;
        var y = year ?? today.Year;
        var m = month ?? today.Month;
        if (m is < 1 or > 12) m = today.Month;

        var firstDay = new DateOnly(y, m, 1);
        var daysInMonth = DateTime.DaysInMonth(y, m);
        var lastDay = new DateOnly(y, m, daysInMonth);

        var logs = await db.DailyLogs
            .Where(d => d.UserId == userId && d.Date >= firstDay && d.Date <= lastDay)
            .Include(d => d.Perfume)
            .OrderBy(d => d.Date)
            .ToListAsync();

        // ISO: Monday=1 .. Sunday=7
        var firstDow = (int)firstDay.DayOfWeek;
        if (firstDow == 0) firstDow = 7; // Sunday → 7

        var vm = new DailyLogIndexViewModel
        {
            Year = y,
            Month = m,
            DaysInMonth = daysInMonth,
            FirstDayOfWeek = firstDow,
            TotalLogsThisMonth = logs.Count,
            LogsByDay = logs.GroupBy(l => l.Date.Day).ToDictionary(g => g.Key, g => g.ToList()),
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateOnly date, int perfumeId, int sprays, string? note)
    {
        var userId = currentUser.UserId!.Value;

        if (!await db.Perfumes.AnyAsync(p => p.Id == perfumeId))
        {
            TempData["Error"] = "Parfüm bulunamadı.";
            return RedirectToAction(nameof(Index), new { year = date.Year, month = date.Month });
        }

        db.DailyLogs.Add(new DailyLog
        {
            UserId = userId,
            PerfumeId = perfumeId,
            Date = date,
            Sprays = Math.Max(0, sprays),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = date.Year, month = date.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? year, int? month)
    {
        var userId = currentUser.UserId!.Value;
        var log = await db.DailyLogs.FirstOrDefaultAsync(d => d.Id == id);
        if (log == null) return NotFound();
        if (log.UserId != userId) return Forbid();

        var y = year ?? log.Date.Year;
        var m = month ?? log.Date.Month;

        db.DailyLogs.Remove(log);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = y, month = m });
    }
}
