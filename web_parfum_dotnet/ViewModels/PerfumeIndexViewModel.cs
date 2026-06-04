using WebParfum.Models;

namespace WebParfum.ViewModels;

public class PerfumeIndexViewModel
{
    // Filters — çoklu seçim destekli (?Brand=a&Brand=b&Accord=oud&Accord=woody)
    public string? Search { get; set; }
    public List<string> Brand { get; set; } = [];
    public List<string> Gender { get; set; } = [];
    public List<string> Accord { get; set; } = [];
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

    public bool HasAnyFilter =>
        !string.IsNullOrWhiteSpace(Search) ||
        Brand.Count > 0 || Gender.Count > 0 || Accord.Count > 0;
}

public record FacetItem(string Value, int Count);
