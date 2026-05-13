using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;
using WebParfum.Services;
using WebParfum.ViewModels;

namespace WebParfum.Controllers;

[Authorize]
public class CommunitiesController(AppDbContext db, ICurrentUserService currentUser) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Communities";
        var userId = currentUser.UserId!.Value;

        var communities = await db.Communities
            .Include(c => c.Owner)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var memberCounts = await db.CommunityMembers
            .GroupBy(m => m.CommunityId)
            .Select(g => new { CommunityId = g.Key, Count = g.Count() })
            .ToListAsync();

        var myMemberships = await db.CommunityMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.CommunityId)
            .ToListAsync();

        var vm = new CommunityIndexViewModel
        {
            Communities = communities.Select(c => new CommunityListItem
            {
                Community = c,
                MemberCount = memberCounts.FirstOrDefault(x => x.CommunityId == c.Id)?.Count ?? 0,
                IsMember = myMemberships.Contains(c.Id),
            }).ToList(),
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Communities";
        var userId = currentUser.UserId!.Value;

        var community = await db.Communities
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (community == null) return NotFound();

        var members = await db.CommunityMembers
            .Include(m => m.User)
            .Where(m => m.CommunityId == id)
            .OrderBy(m => m.JoinedAt)
            .Take(50)
            .ToListAsync();

        var posts = await db.CommunityPosts
            .Include(p => p.User)
            .Include(p => p.Perfume)
            .Where(p => p.CommunityId == id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .ToListAsync();

        var vm = new CommunityDetailViewModel
        {
            Community = community,
            Members = members,
            Posts = posts,
            MemberCount = await db.CommunityMembers.CountAsync(m => m.CommunityId == id),
            IsMember = members.Any(m => m.UserId == userId)
                       || await db.CommunityMembers.AnyAsync(m => m.CommunityId == id && m.UserId == userId),
            IsOwner = community.OwnerId == userId,
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Communities";
        return View(new CommunityCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CommunityCreateViewModel model)
    {
        ViewData["ActivePage"] = "Communities";
        if (!ModelState.IsValid) return View(model);

        var userId = currentUser.UserId!.Value;
        var name = model.Name.Trim();

        if (await db.Communities.AnyAsync(c => EF.Functions.ILike(c.Name, name)))
        {
            ModelState.AddModelError(nameof(model.Name), "Bu adda bir topluluk zaten var.");
            return View(model);
        }

        using var tx = await db.Database.BeginTransactionAsync();

        var community = new Community
        {
            Name = name,
            Description = model.Description?.Trim(),
            Image = model.ImageUrl,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow,
        };
        db.Communities.Add(community);
        await db.SaveChangesAsync();

        // Owner ilk üye olarak otomatik eklenir (admin rolüyle)
        db.CommunityMembers.Add(new CommunityMember
        {
            CommunityId = community.Id,
            UserId = userId,
            Role = "admin",
            JoinedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction(nameof(Details), new { id = community.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinToggle(int id)
    {
        var userId = currentUser.UserId!.Value;

        var community = await db.Communities.FirstOrDefaultAsync(c => c.Id == id);
        if (community == null) return NotFound();

        if (community.OwnerId == userId)
        {
            TempData["Error"] = "Owner topluluktan ayrılamaz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var existing = await db.CommunityMembers
            .FirstOrDefaultAsync(m => m.CommunityId == id && m.UserId == userId);

        if (existing != null)
        {
            db.CommunityMembers.Remove(existing);
        }
        else
        {
            db.CommunityMembers.Add(new CommunityMember
            {
                CommunityId = id,
                UserId = userId,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
            });
        }
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(int id, string body, int? perfumeId)
    {
        var userId = currentUser.UserId!.Value;

        if (string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "Boş gönderi olmaz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var isMember = await db.CommunityMembers
            .AnyAsync(m => m.CommunityId == id && m.UserId == userId);
        if (!isMember) return Forbid();

        if (perfumeId.HasValue && !await db.Perfumes.AnyAsync(p => p.Id == perfumeId.Value))
        {
            perfumeId = null;
        }

        db.CommunityPosts.Add(new CommunityPost
        {
            CommunityId = id,
            UserId = userId,
            Body = body.Trim(),
            PerfumeId = perfumeId,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int communityId, int postId)
    {
        var userId = currentUser.UserId!.Value;

        var post = await db.CommunityPosts
            .Include(p => p.Community)
            .FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) return NotFound();

        // post sahibi veya community sahibi silebilir
        if (post.UserId != userId && post.Community.OwnerId != userId)
            return Forbid();

        db.CommunityPosts.Remove(post);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = communityId });
    }
}
