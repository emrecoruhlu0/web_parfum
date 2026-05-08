using WebParfum.Models;

namespace WebParfum.ViewModels;

public class PerfumeIndexViewModel
{
    // Filters
    public string? Search { get; set; }
    public string? Brand { get; set; }
    public string? Gender { get; set; }
    public string? Accord { get; set; }
    public int? Year { get; set; }
    public string Sort { get; set; } = "popular"; // popular | newest | rating

    // Paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    // Results
    public List<Perfume> Perfumes { get; set; } = [];

    // Meta (sidebar facets)
    public List<FacetItem> Brands { get; set; } = [];
    public List<FacetItem> Accords { get; set; } = [];
    public List<FacetItem> Genders { get; set; } = [];
}

public record FacetItem(string Value, int Count);
