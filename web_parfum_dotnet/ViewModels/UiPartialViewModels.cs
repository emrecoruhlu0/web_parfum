namespace WebParfum.ViewModels;

public class PerfumeImageViewModel
{
    public string? ImageUrl { get; set; }
    public required string Name { get; set; }
    public string? Brand { get; set; }
    /// <summary>card | thumb-sm | thumb-md | detail</summary>
    public string Variant { get; set; } = "card";
    public string? CssClass { get; set; }
    public string? Alt { get; set; }
}

public class EmptyStateViewModel
{
    public string Icon { get; set; } = "bi-inbox";
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? ActionText { get; set; }
    public string? ActionController { get; set; }
    public string? ActionName { get; set; }
}

public class PageHeaderViewModel
{
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
}
