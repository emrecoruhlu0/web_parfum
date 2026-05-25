using Microsoft.EntityFrameworkCore;
using WebParfum.Models;

namespace WebParfum.Helpers;

// Search ve Index action'larının paylaştığı arama mantığı.
// pg_trgm + unaccent destekli SearchKey kolonu üzerinden çalışır.
// Her token için (ILIKE substring) OR (word_similarity > 0.4); tokenlar AND'lenir.
// word_similarity, token'ı uzun SearchKey içindeki en yakın "kelime" ile karşılaştırır
// — kısa query'lerde standart similarity'den çok daha doğru.
public static class PerfumeSearchQuery
{
    public const double WordSimilarityThreshold = 0.4;

    public static IQueryable<Perfume> ApplySearch(this IQueryable<Perfume> source, string? raw)
    {
        var tokens = SearchNormalizer.Tokenize(raw);
        if (tokens.Length == 0) return source;

        var query = source;
        foreach (var t in tokens)
        {
            var like = $"%{t}%";
            var token = t;
            query = query.Where(p =>
                EF.Functions.ILike(p.SearchKey!, like) ||
                EF.Functions.TrigramsWordSimilarity(token, p.SearchKey!) > WordSimilarityThreshold);
        }
        return query;
    }
}
