using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WebParfum.Models;

namespace WebParfum.Data;

public static class DbInitializer
{
    private const int BatchSize = 500;

    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        var needsImageUpdate = db.Perfumes.Any() && !db.Perfumes.Any(p => p.ImageUrl != null);
        if (needsImageUpdate)
        {
            BackfillImageUrls(db);
            return;
        }

        if (db.Perfumes.Any())
        {
            SeedMockData(db);
            return;
        }

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
                ImageUrl = ExtractImageUrl(Get(cols, idx, "url")),
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

        SeedMockData(db);
    }

    private static void SeedMockData(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var perfumes = db.Perfumes.Take(30).ToList();
        if (perfumes.Count == 0) return;

        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var now = DateTime.UtcNow;

        var users = new List<User>
        {
            new() { Username = "emre", Email = "emre@example.com", PasswordHash = hash, Bio = "Parfüm dünyasını keşfeden meraklı bir ruh.", CreatedAt = now },
            new() { Username = "ayse", Email = "ayse@example.com", PasswordHash = hash, Bio = "Niş kokuların izinde bir koleksiyoncu.", CreatedAt = now },
            new() { Username = "mehmet", Email = "mehmet@example.com", PasswordHash = hash, Bio = "Fresh ve sporty kokular benim için vazgeçilmez.", CreatedAt = now },
            new() { Username = "zeynep", Email = "zeynep@example.com", PasswordHash = hash, Bio = "Oriental ve oud konusunda bir uzman.", CreatedAt = now },
            new() { Username = "can", Email = "can@example.com", PasswordHash = hash, Bio = "Az ama öz. Minimalist koku anlayışı.", CreatedAt = now },
            new() { Username = "lale", Email = "lale@example.com", PasswordHash = hash, Bio = "Çiçeksi ve feminen kokuların hayranı.", CreatedAt = now },
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var u = users; // kısa alias
        var p = perfumes;

        // Collections
        var collections = new List<Collection>
        {
            new() { UserId = u[0].Id, PerfumeId = p[0].Id, Status = "owned",    BottleLevel = 80,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[1].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[2].Id, Status = "tried",    BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[3].Id, Status = "owned",    BottleLevel = 45,  CreatedAt = now, UpdatedAt = now },

            new() { UserId = u[1].Id, PerfumeId = p[4].Id, Status = "owned",    BottleLevel = 60,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[5].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[6].Id, Status = "tried",    BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[0].Id, Status = "owned",    BottleLevel = 30,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[7].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },

            new() { UserId = u[2].Id, PerfumeId = p[8].Id,  Status = "owned",    BottleLevel = 100, CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[9].Id,  Status = "tried",    BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[10].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },

            new() { UserId = u[3].Id, PerfumeId = p[11].Id, Status = "owned",    BottleLevel = 55,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[12].Id, Status = "owned",    BottleLevel = 20,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[13].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[14].Id, Status = "tried",    BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },

            new() { UserId = u[4].Id, PerfumeId = p[15].Id, Status = "owned",    BottleLevel = 70,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[16].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[17].Id, Status = "tried",    BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },

            new() { UserId = u[5].Id, PerfumeId = p[18].Id, Status = "owned",    BottleLevel = 90,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[19].Id, Status = "owned",    BottleLevel = 15,  CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[20].Id, Status = "wishlist", BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[1].Id,  Status = "tried",    BottleLevel = 0,   CreatedAt = now, UpdatedAt = now },
        };
        db.Collections.AddRange(collections);
        db.SaveChanges();

        // Reviews
        var reviews = new List<Review>
        {
            new() { UserId = u[0].Id, PerfumeId = p[0].Id, Rating = 5, Body = "Muhteşem bir koku! İlk spreyden itibaren büyülendim.", CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[4].Id, Rating = 4, Body = "Uzun süre kalıcı, özellikle sonbahar için ideal.", CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[0].Id, Rating = 4, Body = "Klasik bir seçim, her ortama uygun.", CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[5].Id, Rating = 5, Body = "Koleksiyonumun favorisi. Başka koku istemiyorum.", CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[8].Id, Rating = 3, Body = "Güzel ama beklentimi tam karşılamadı.", CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[8].Id, Rating = 5, Body = "Spor sonrası mükemmel, ferahlık veriyor.", CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[9].Id, Rating = 4, Body = "Hafif ve enerjik, her gün kullanılabilir.", CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[11].Id, Rating = 5, Body = "Oud bazlı en iyi koku bu. Derin ve gizemli.", CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[12].Id, Rating = 5, Body = "Oriental notalar mükemmel dengelenmiş.", CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[2].Id, Rating = 3, Body = "Fena değil ama çok sıradan buldum.", CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[15].Id, Rating = 5, Body = "Sade ve şık. Minimalizmin koku hali.", CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[16].Id, Rating = 4, Body = "Temiz ve taze, ofis için biçilmiş kaftan.", CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[18].Id, Rating = 5, Body = "Gül ve yasemin dengesi harika. Çok feminen.", CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[19].Id, Rating = 4, Body = "Çiçekli ama ağır değil, bahar için mükemmel.", CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[15].Id, Rating = 4, Body = "Kalıcılığı düşük ama koku gerçekten güzel.", CreatedAt = now },
        };
        db.Reviews.AddRange(reviews);
        db.SaveChanges();

        // DailyLogs
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyLogs = new List<DailyLog>
        {
            new() { UserId = u[0].Id, PerfumeId = p[0].Id,  Date = today,                Sprays = 3, Note = "İş toplantısı için.", CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[1].Id,  Date = today.AddDays(-1),    Sprays = 2, Note = null, CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[3].Id,  Date = today.AddDays(-2),    Sprays = 4, Note = "Akşam yemeği.", CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[2].Id,  Date = today.AddDays(-4),    Sprays = 2, Note = null, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[5].Id,  Date = today,                Sprays = 5, Note = "Özel bir gün!", CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[4].Id,  Date = today.AddDays(-1),    Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[7].Id,  Date = today.AddDays(-3),    Sprays = 2, Note = "Alışveriş merkezi.", CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[8].Id,  Date = today,                Sprays = 4, Note = "Spor öncesi.", CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[9].Id,  Date = today.AddDays(-2),    Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[11].Id, Date = today,                Sprays = 2, Note = "Akşam çıkışı.", CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[12].Id, Date = today.AddDays(-1),    Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[13].Id, Date = today.AddDays(-3),    Sprays = 2, Note = "Toplantı.", CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[15].Id, Date = today,                Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[16].Id, Date = today.AddDays(-2),    Sprays = 2, Note = "Günlük.", CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[18].Id, Date = today,                Sprays = 4, Note = "Doğum günü partisi!", CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[19].Id, Date = today.AddDays(-1),    Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[20].Id, Date = today.AddDays(-3),    Sprays = 2, Note = "Sabah rutini.", CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[4].Id,  Date = today.AddDays(-5),    Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[6].Id,  Date = today.AddDays(-5),    Sprays = 2, Note = null, CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[10].Id, Date = today.AddDays(-6),    Sprays = 1, Note = "Hafif bir gün.", CreatedAt = now },
        };
        db.DailyLogs.AddRange(dailyLogs);
        db.SaveChanges();

        // Likes
        var likes = new List<Like>
        {
            new() { UserId = u[0].Id, PerfumeId = p[0].Id,  CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[4].Id,  CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[8].Id,  CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[11].Id, CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[15].Id, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[0].Id,  CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[5].Id,  CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[6].Id,  CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[12].Id, CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[8].Id,  CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[9].Id,  CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[10].Id, CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[1].Id,  CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[11].Id, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[12].Id, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[13].Id, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[14].Id, CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[15].Id, CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[16].Id, CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[3].Id,  CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[18].Id, CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[19].Id, CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[20].Id, CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[7].Id,  CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[18].Id, CreatedAt = now },
        };
        db.Likes.AddRange(likes);
        db.SaveChanges();

        // Follows
        var follows = new List<Follow>
        {
            new() { FollowerId = u[0].Id, FollowingId = u[1].Id, CreatedAt = now },
            new() { FollowerId = u[0].Id, FollowingId = u[3].Id, CreatedAt = now },
            new() { FollowerId = u[1].Id, FollowingId = u[0].Id, CreatedAt = now },
            new() { FollowerId = u[1].Id, FollowingId = u[5].Id, CreatedAt = now },
            new() { FollowerId = u[2].Id, FollowingId = u[4].Id, CreatedAt = now },
            new() { FollowerId = u[4].Id, FollowingId = u[3].Id, CreatedAt = now },
        };
        db.Follows.AddRange(follows);
        db.SaveChanges();

        // Notifications
        var notifications = new List<Notification>
        {
            new() { RecipientId = u[1].Id, ActorId = u[0].Id, Type = "follow",  IsRead = false, CreatedAt = now },
            new() { RecipientId = u[3].Id, ActorId = u[0].Id, Type = "follow",  IsRead = true,  CreatedAt = now },
            new() { RecipientId = u[0].Id, ActorId = u[1].Id, Type = "follow",  IsRead = false, CreatedAt = now },
            new() { RecipientId = u[5].Id, ActorId = u[1].Id, Type = "follow",  IsRead = true,  CreatedAt = now },
            new() { RecipientId = u[4].Id, ActorId = u[2].Id, Type = "follow",  IsRead = false, CreatedAt = now },
            new() { RecipientId = u[0].Id, ActorId = u[1].Id, Type = "like",    PerfumeId = p[0].Id, IsRead = false, CreatedAt = now },
            new() { RecipientId = u[0].Id, ActorId = u[2].Id, Type = "like",    PerfumeId = p[8].Id, IsRead = true,  CreatedAt = now },
            new() { RecipientId = u[0].Id, ActorId = u[3].Id, Type = "review",  PerfumeId = p[2].Id, IsRead = false, CreatedAt = now },
            new() { RecipientId = u[1].Id, ActorId = u[0].Id, Type = "review",  PerfumeId = p[0].Id, IsRead = true,  CreatedAt = now },
            new() { RecipientId = u[3].Id, ActorId = u[4].Id, Type = "follow",  IsRead = false, CreatedAt = now },
        };
        db.Notifications.AddRange(notifications);
        db.SaveChanges();

        // Communities
        var oudCommunity = new Community
        {
            Name = "Oud Severler",
            Description = "Oriental ve oud bazlı kokuları tartışıyoruz. Doğu'nun zengin koku dünyasına hoş geldiniz.",
            OwnerId = u[3].Id,
            CreatedAt = now,
        };
        var freshCommunity = new Community
        {
            Name = "Fresh & Citrus",
            Description = "Ferah, narenciye ve aquatik kokuların tutkunları burada. Yaz enerjisini yıl boyu yaşayalım.",
            OwnerId = u[4].Id,
            CreatedAt = now,
        };
        db.Communities.AddRange(oudCommunity, freshCommunity);
        db.SaveChanges();

        // CommunityMembers
        var members = new List<CommunityMember>
        {
            new() { CommunityId = oudCommunity.Id,   UserId = u[3].Id, Role = "admin",  JoinedAt = now },
            new() { CommunityId = oudCommunity.Id,   UserId = u[0].Id, Role = "member", JoinedAt = now },
            new() { CommunityId = oudCommunity.Id,   UserId = u[1].Id, Role = "member", JoinedAt = now },
            new() { CommunityId = oudCommunity.Id,   UserId = u[5].Id, Role = "member", JoinedAt = now },
            new() { CommunityId = freshCommunity.Id, UserId = u[4].Id, Role = "admin",  JoinedAt = now },
            new() { CommunityId = freshCommunity.Id, UserId = u[2].Id, Role = "member", JoinedAt = now },
            new() { CommunityId = freshCommunity.Id, UserId = u[0].Id, Role = "member", JoinedAt = now },
        };
        db.CommunityMembers.AddRange(members);
        db.SaveChanges();

        // CommunityPosts
        var posts = new List<CommunityPost>
        {
            new() { CommunityId = oudCommunity.Id,   UserId = u[3].Id, PerfumeId = p[11].Id, Body = "Bu oud kokusu gerçekten eşsiz. Gece çıkışları için birebir tavsiye ederim.", CreatedAt = now },
            new() { CommunityId = oudCommunity.Id,   UserId = u[0].Id, PerfumeId = null,      Body = "Oud kokularını ilk keşfettiğimde çok yabancı gelmişti ama şimdi favorilerimden.", CreatedAt = now },
            new() { CommunityId = oudCommunity.Id,   UserId = u[1].Id, PerfumeId = p[12].Id, Body = "Oriental notaları seven herkese bu parfümü denemesini şiddetle tavsiye ederim!", CreatedAt = now },
            new() { CommunityId = freshCommunity.Id, UserId = u[4].Id, PerfumeId = p[15].Id, Body = "Sade ve temiz bir koku. Her mevsim kullanılabilir, özellikle sabah için harika.", CreatedAt = now },
            new() { CommunityId = freshCommunity.Id, UserId = u[2].Id, PerfumeId = p[8].Id,  Body = "Spor yaparken bu kokunun enerjisi inanılmaz. Kesinlikle deneyin.", CreatedAt = now },
            new() { CommunityId = freshCommunity.Id, UserId = u[0].Id, PerfumeId = null,      Body = "Yaz geldi, narenciye kokularının zamanı! Önerileriniz neler?", CreatedAt = now },
        };
        db.CommunityPosts.AddRange(posts);
        db.SaveChanges();

        // Messages
        var messages = new List<Message>
        {
            new() { SenderId = u[0].Id, RecipientId = u[1].Id, Body = "Merhaba! Koleksiyonunu gördüm, çok etkileyici.", IsRead = true,  CreatedAt = now.AddMinutes(-60) },
            new() { SenderId = u[1].Id, RecipientId = u[0].Id, Body = "Teşekkürler! Sen de güzel parfümler seçmişsin.", IsRead = true,  CreatedAt = now.AddMinutes(-55) },
            new() { SenderId = u[0].Id, RecipientId = u[1].Id, Body = "O oud kokuyu sana da öneririm, mutlaka dene.", IsRead = false, CreatedAt = now.AddMinutes(-50) },

            new() { SenderId = u[2].Id, RecipientId = u[3].Id, Body = "Oud Severler topluluğuna ben de katılabilir miyim?", IsRead = true,  CreatedAt = now.AddMinutes(-120) },
            new() { SenderId = u[3].Id, RecipientId = u[2].Id, Body = "Tabii ki! Seni bekliyoruz.", IsRead = true,  CreatedAt = now.AddMinutes(-115) },

            new() { SenderId = u[4].Id, RecipientId = u[5].Id, Body = "Fresh & Citrus topluluğuna göz attın mı?", IsRead = true,  CreatedAt = now.AddMinutes(-200) },
            new() { SenderId = u[5].Id, RecipientId = u[4].Id, Body = "Evet, çok güzel içerikler var! Ben çiçeksiyi tercih etsem de.", IsRead = true,  CreatedAt = now.AddMinutes(-190) },
            new() { SenderId = u[4].Id, RecipientId = u[5].Id, Body = "Anladım, bir gün belki birlikte koku mağazasına gidebiliriz.", IsRead = false, CreatedAt = now.AddMinutes(-180) },
        };
        db.Messages.AddRange(messages);
        db.SaveChanges();

        Console.WriteLine("[Seed] Mock veriler eklendi: 6 kullanıcı, koleksiyonlar, yorumlar, günlük loglar, beğeniler, takipler, bildirimler, 2 topluluk, mesajlar.");
    }

    private static void BackfillImageUrls(AppDbContext db)
    {
        var csvPath = ResolveCsvPath();
        if (csvPath == null) return;

        Console.WriteLine("[Seed] ImageUrl backfill başlıyor...");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("ISO-8859-1");
        using var reader = new StreamReader(csvPath, encoding);

        var headerLine = reader.ReadLine();
        if (headerLine == null) return;

        var headers = headerLine.Split(';');
        var idx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
            idx[headers[i].Trim()] = i;

        // name -> imageUrl mapping
        var urlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var cols = line.Split(';');
            if (cols.Length < headers.Length) continue;
            var name = Clean(Get(cols, idx, "Perfume"));
            var imgUrl = ExtractImageUrl(Get(cols, idx, "url"));
            if (name != null && imgUrl != null)
                urlMap.TryAdd(name, imgUrl);
        }

        var perfumes = db.Perfumes.Where(p => p.ImageUrl == null).ToList();
        int updated = 0;
        foreach (var p in perfumes)
        {
            if (urlMap.TryGetValue(p.Name, out var img))
            {
                p.ImageUrl = img;
                updated++;
            }
        }

        if (updated > 0)
        {
            db.SaveChanges();
            Console.WriteLine($"[Seed] {updated} parfümün ImageUrl'si güncellendi.");
        }
    }

    private static string? ExtractImageUrl(string? fraganticaUrl)
    {
        if (string.IsNullOrWhiteSpace(fraganticaUrl)) return null;
        var m = Regex.Match(fraganticaUrl, @"-(\d+)\.html$");
        if (!m.Success) return null;
        return $"https://fimgs.net/mdimg/perfume/375x500.{m.Groups[1].Value}.jpg";
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
