using System.ComponentModel.DataAnnotations;
using WebParfum.Models;

namespace WebParfum.ViewModels;

public class CommunityListItem
{
    public Community Community { get; set; } = null!;
    public int MemberCount { get; set; }
    public bool IsMember { get; set; }
}

public class CommunityIndexViewModel
{
    public List<CommunityListItem> Communities { get; set; } = [];
}

public class CommunityDetailViewModel
{
    public Community Community { get; set; } = null!;
    public List<CommunityMember> Members { get; set; } = [];
    public List<CommunityPost> Posts { get; set; } = [];
    public int MemberCount { get; set; }
    public bool IsMember { get; set; }
    public bool IsOwner { get; set; }
}

public class CommunityCreateViewModel
{
    [Required, MaxLength(100), Display(Name = "Topluluk Adı")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500), Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Url, Display(Name = "Resim URL")]
    public string? ImageUrl { get; set; }
}
