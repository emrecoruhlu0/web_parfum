namespace WebParfum.ViewModels;

public class UserSearchViewModel
{
    public string Query { get; set; } = "";
    public List<UserSearchResult> Results { get; set; } = [];
}

public class UserSearchResult
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string? Bio { get; set; }
    public bool IsSelf { get; set; }
    public bool IsFollowing { get; set; }
}
