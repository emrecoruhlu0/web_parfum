using System.Globalization;
using System.Text;
using WebParfum.Models;

namespace WebParfum.Data;

public static class DbInitializer
{
    private const int BatchSize = 500;

    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Perfumes.Any()) return;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var csvPath = ResolveCsvPath();
        if (csvPath == null)
        {
            Console.WriteLine("[Seed] fra_cleaned.csv bulunamadı, parfüm seed atlanıyor.");
            return;
        }

        Console.WriteLine($"[Seed] CSV okunuyor: {csvPath}");

        var encoding = Encoding.GetEncoding("ISO-8859-1");
        using var reader = new StreamReader(csvPath, encoding);

        var headerLine = reader.ReadLine();
        if (headerLine == null) return;

        var headers = headerLine.Split(';');
        var idx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
            idx[headers[i].Trim()] = i;

        var batch = new List<Perfume>(BatchSize);
        int total = 0, skipped = 0;

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var cols = line.Split(';');
            if (cols.Length < headers.Length) { skipped++; continue; }

            var name = Clean(Get(cols, idx, "Perfume"));
            var brand = Clean(Get(cols, idx, "Brand"));
            if (name == null || brand == null) { skipped++; continue; }

            batch.Add(new Perfume
            {
                Name = name,
                Brand = brand,
                Country = Clean(Get(cols, idx, "Country")),
                Gender = Clean(Get(cols, idx, "Gender")),
                Year = ParseYear(Get(cols, idx, "Year")),
                RatingValue = ParseRating(Get(cols, idx, "Rating Value")),
                RatingCount = ParseInt(Get(cols, idx, "Rating Count")),
                TopNotes = Clean(Get(cols, idx, "Top")),
                MiddleNotes = Clean(Get(cols, idx, "Middle")),
                BaseNotes = Clean(Get(cols, idx, "Base")),
                Accord1 = Clean(Get(cols, idx, "mainaccord1")),
                Accord2 = Clean(Get(cols, idx, "mainaccord2")),
                Accord3 = Clean(Get(cols, idx, "mainaccord3")),
                Accord4 = Clean(Get(cols, idx, "mainaccord4")),
                Accord5 = Clean(Get(cols, idx, "mainaccord5")),
                CreatedAt = DateTime.UtcNow,
            });

            if (batch.Count >= BatchSize)
            {
                db.Perfumes.AddRange(batch);
                db.SaveChanges();
                total += batch.Count;
                batch.Clear();
                if (total % 5000 == 0)
                    Console.WriteLine($"[Seed] {total} parfüm eklendi...");
            }
        }

        if (batch.Count > 0)
        {
            db.Perfumes.AddRange(batch);
            db.SaveChanges();
            total += batch.Count;
        }

        Console.WriteLine($"[Seed] Tamamlandı: {total} parfüm eklendi, {skipped} satır atlandı.");
    }

    private static string? ResolveCsvPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Data", "fra_cleaned.csv"),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "fra_cleaned.csv"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "archive", "fra_cleaned.csv"),
            Path.Combine(Directory.GetCurrentDirectory(), "archive", "fra_cleaned.csv"),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static string Get(string[] cols, Dictionary<string, int> idx, string key)
        => idx.TryGetValue(key, out var i) && i < cols.Length ? cols[i] : "";

    private static string? Clean(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var trimmed = val.Trim();
        if (trimmed.Equals("unknown", StringComparison.OrdinalIgnoreCase)) return null;
        return trimmed;
    }

    private static int? ParseYear(string? val)
    {
        if (!int.TryParse(val, out var n)) return null;
        return n is > 1800 and < 2100 ? n : null;
    }

    private static int? ParseInt(string? val)
        => int.TryParse(val, out var n) ? n : null;

    private static double? ParseRating(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var normalized = val.Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
    }
}
