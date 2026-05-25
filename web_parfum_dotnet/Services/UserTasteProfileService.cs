using Microsoft.EntityFrameworkCore;
using WebParfum.Data;
using WebParfum.Models;

namespace WebParfum.Services;

public record TasteProfile(
    Dictionary<string, double> PositiveAccords,
    Dictionary<string, double> NegativeAccords,
    Dictionary<string, double> PositiveNotes,
    Dictionary<string, double> NegativeNotes,
    int SignalCount,
    int InteractedPerfumeCount
);

public record PerfumeRecommendation(
    Perfume Perfume,
    double Score,
    double ContentScore,
    double CollaborativeScore,
    List<string> MatchingAccords
);

public class UserTasteProfileService(AppDbContext db)
{
    // Sinyal ağırlıkları
    private const double LikeWeight = 2.0;
    private const double OwnedWeight = 2.5;
    private const double WishlistWeight = 1.5;
    private const double TriedWeight = 0.5;
    private const double DailyLogPerEntry = 0.3;
    private const double DailyLogMax = 3.0;

    // Hibrit ağırlıkları
    private const double ContentWeight = 0.7;
    private const double CollabWeight = 0.3;

    // NOTE: cache yok — küçük veri seti için her istek hesaplama yeterli.
    // Yavaşlama olursa IMemoryCache eklenebilir (key: $"taste:{userId}").

    public async Task<TasteProfile> ComputeAsync(int userId)
    {
        var perfumeScores = await CollectPerfumeScoresAsync(userId);

        var posAccords = new Dictionary<string, double>();
        var negAccords = new Dictionary<string, double>();
        var posNotes = new Dictionary<string, double>();
        var negNotes = new Dictionary<string, double>();

        foreach (var (perfume, score) in perfumeScores)
        {
            if (score == 0) continue;
            var accords = ExtractAccords(perfume);
            var notes = ExtractTopNotes(perfume);

            var accordWeight = score; // her akor full payı alır
            var noteWeight = score * 0.6; // notalar biraz daha hafif

            foreach (var a in accords)
                AddTo(score > 0 ? posAccords : negAccords, a, Math.Abs(accordWeight));
            foreach (var n in notes)
                AddTo(score > 0 ? posNotes : negNotes, n, Math.Abs(noteWeight));
        }

        return new TasteProfile(
            PositiveAccords: posAccords,
            NegativeAccords: negAccords,
            PositiveNotes: posNotes,
            NegativeNotes: negNotes,
            SignalCount: perfumeScores.Count(p => p.Score != 0),
            InteractedPerfumeCount: perfumeScores.Count
        );
    }

    public async Task<List<PerfumeRecommendation>> RecommendAsync(int userId, int take = 20)
    {
        var profile = await ComputeAsync(userId);
        if (profile.SignalCount == 0) return new();

        // Etkileşim kurulan parfümler — önerilerden hariç tutulacak
        var interactedIds = await GetInteractedPerfumeIdsAsync(userId);
        var interactedSet = interactedIds.ToHashSet();

        // İçerik tabanlı: pozitif akor/nota vektörü ile cosine
        var userVec = BuildUserVector(profile);
        var userNeg = profile.NegativeAccords;

        // İşbirlikçi: benzer kullanıcıların yüksek puanları
        var collabScores = await ComputeCollaborativeScoresAsync(userId, interactedSet);

        var candidates = await db.Perfumes
            .Where(p => !interactedSet.Contains(p.Id))
            .ToListAsync();

        var recos = new List<PerfumeRecommendation>();
        foreach (var p in candidates)
        {
            var pVec = BuildPerfumeVector(p);
            if (pVec.Count == 0) continue;

            var content = CosineSimilarity(userVec, pVec);
            if (content <= 0 && !collabScores.ContainsKey(p.Id)) continue;

            // Negatif akor cezası
            var penalty = ComputeNegativePenalty(p, userNeg);
            content = Math.Max(0, content - penalty);

            var collab = collabScores.TryGetValue(p.Id, out var c) ? c : 0;
            var final = ContentWeight * content + CollabWeight * collab;
            if (final <= 0) continue;

            var matching = ExtractAccords(p)
                .Where(a => profile.PositiveAccords.ContainsKey(a))
                .Take(3)
                .ToList();

            recos.Add(new PerfumeRecommendation(p, final, content, collab, matching));
        }

        return recos
            .OrderByDescending(r => r.Score)
            .Take(take)
            .ToList();
    }

    // --- private helpers ---

    private async Task<List<(Perfume Perfume, double Score)>> CollectPerfumeScoresAsync(int userId)
    {
        var likes = await db.Likes.Where(l => l.UserId == userId).Include(l => l.Perfume).ToListAsync();
        var reviews = await db.Reviews.Where(r => r.UserId == userId).Include(r => r.Perfume).ToListAsync();
        var collections = await db.Collections.Where(c => c.UserId == userId).Include(c => c.Perfume).ToListAsync();
        var logs = await db.DailyLogs.Where(d => d.UserId == userId).Include(d => d.Perfume).ToListAsync();

        var map = new Dictionary<int, (Perfume p, double s)>();

        void Add(Perfume p, double delta)
        {
            if (map.TryGetValue(p.Id, out var cur))
                map[p.Id] = (cur.p, cur.s + delta);
            else
                map[p.Id] = (p, delta);
        }

        foreach (var l in likes) Add(l.Perfume, LikeWeight);

        foreach (var r in reviews)
        {
            var w = r.Rating switch
            {
                5 => 3.0,
                4 => 1.5,
                3 => 0.0,
                2 => -1.5,
                1 => -3.0,
                _ => 0.0
            };
            if (w != 0) Add(r.Perfume, w);
        }

        foreach (var c in collections)
        {
            var w = c.Status?.ToLower() switch
            {
                "owned" => OwnedWeight,
                "wishlist" => WishlistWeight,
                "tried" => TriedWeight,
                _ => 0.0
            };
            if (w != 0) Add(c.Perfume, w);
        }

        // DailyLog: kaç gün log'ladıysa o kadar artı, max DailyLogMax
        var logGroups = logs.GroupBy(d => d.PerfumeId);
        foreach (var g in logGroups)
        {
            var perfume = g.First().Perfume;
            var w = Math.Min(DailyLogMax, g.Count() * DailyLogPerEntry);
            Add(perfume, w);
        }

        return map.Values.Select(v => (v.p, v.s)).ToList();
    }

    private async Task<HashSet<int>> GetInteractedPerfumeIdsAsync(int userId)
    {
        var ids = new HashSet<int>();
        ids.UnionWith(await db.Likes.Where(l => l.UserId == userId).Select(l => l.PerfumeId).ToListAsync());
        ids.UnionWith(await db.Reviews.Where(r => r.UserId == userId).Select(r => r.PerfumeId).ToListAsync());
        ids.UnionWith(await db.Collections.Where(c => c.UserId == userId).Select(c => c.PerfumeId).ToListAsync());
        ids.UnionWith(await db.DailyLogs.Where(d => d.UserId == userId).Select(d => d.PerfumeId).ToListAsync());
        return ids;
    }

    private async Task<Dictionary<int, double>> ComputeCollaborativeScoresAsync(int userId, HashSet<int> myInteractedIds)
    {
        if (myInteractedIds.Count == 0) return new();

        // Benzer kullanıcı: aynı parfümlerle pozitif etkileşim (like veya review>=4)
        var myPositiveIds = await db.Likes.Where(l => l.UserId == userId).Select(l => l.PerfumeId).ToListAsync();
        myPositiveIds.AddRange(await db.Reviews
            .Where(r => r.UserId == userId && r.Rating >= 4)
            .Select(r => r.PerfumeId).ToListAsync());
        var mySet = myPositiveIds.ToHashSet();
        if (mySet.Count == 0) return new();

        // Aynı parfümleri beğenen kullanıcılar
        var otherUsers = await db.Likes
            .Where(l => mySet.Contains(l.PerfumeId) && l.UserId != userId)
            .GroupBy(l => l.UserId)
            .Select(g => new { UserId = g.Key, Shared = g.Count() })
            .Where(x => x.Shared >= 2) // minimum 2 ortak
            .OrderByDescending(x => x.Shared)
            .Take(20)
            .ToListAsync();

        if (otherUsers.Count == 0) return new();

        var similarIds = otherUsers.Select(o => o.UserId).ToList();
        var weightById = otherUsers.ToDictionary(o => o.UserId, o => (double)o.Shared / mySet.Count);

        // Bu kullanıcıların yüksek değer verdiği ama benim etkileşmediğim parfümler
        var theirLikes = await db.Likes
            .Where(l => similarIds.Contains(l.UserId) && !myInteractedIds.Contains(l.PerfumeId))
            .ToListAsync();

        var scores = new Dictionary<int, double>();
        foreach (var l in theirLikes)
        {
            var w = weightById[l.UserId];
            scores[l.PerfumeId] = scores.GetValueOrDefault(l.PerfumeId) + w;
        }

        // Normalize 0-1
        if (scores.Count > 0)
        {
            var max = scores.Values.Max();
            if (max > 0)
                foreach (var k in scores.Keys.ToList())
                    scores[k] /= max;
        }

        return scores;
    }

    private static Dictionary<string, double> BuildUserVector(TasteProfile profile)
    {
        var vec = new Dictionary<string, double>();
        foreach (var (k, v) in profile.PositiveAccords) vec[k] = v;
        foreach (var (k, v) in profile.PositiveNotes)
            vec[k] = vec.GetValueOrDefault(k) + v * 0.5;
        return vec;
    }

    private static Dictionary<string, double> BuildPerfumeVector(Perfume p)
    {
        var vec = new Dictionary<string, double>();
        foreach (var a in ExtractAccords(p)) vec[a] = vec.GetValueOrDefault(a) + 1.0;
        foreach (var n in ExtractTopNotes(p)) vec[n] = vec.GetValueOrDefault(n) + 0.5;
        return vec;
    }

    private static double CosineSimilarity(Dictionary<string, double> a, Dictionary<string, double> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;
        double dot = 0;
        foreach (var (k, v) in a)
            if (b.TryGetValue(k, out var bv)) dot += v * bv;
        if (dot == 0) return 0;
        var magA = Math.Sqrt(a.Values.Sum(v => v * v));
        var magB = Math.Sqrt(b.Values.Sum(v => v * v));
        if (magA == 0 || magB == 0) return 0;
        return dot / (magA * magB);
    }

    private static double ComputeNegativePenalty(Perfume p, Dictionary<string, double> negativeAccords)
    {
        if (negativeAccords.Count == 0) return 0;
        var maxNeg = negativeAccords.Values.Max();
        if (maxNeg == 0) return 0;
        double penalty = 0;
        foreach (var a in ExtractAccords(p))
            if (negativeAccords.TryGetValue(a, out var w))
                penalty += (w / maxNeg) * 0.15;
        return Math.Min(penalty, 0.5);
    }

    private static IEnumerable<string> ExtractAccords(Perfume p)
    {
        var raw = new[] { p.Accord1, p.Accord2, p.Accord3, p.Accord4, p.Accord5 };
        return raw
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim().ToLowerInvariant())
            .Where(s => s.Length > 0);
    }

    private static IEnumerable<string> ExtractTopNotes(Perfume p)
    {
        if (string.IsNullOrWhiteSpace(p.TopNotes)) return Array.Empty<string>();
        return p.TopNotes
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => s.Length > 0)
            .Distinct();
    }

    private static void AddTo(Dictionary<string, double> dict, string key, double value)
    {
        dict[key] = dict.GetValueOrDefault(key) + value;
    }
}
