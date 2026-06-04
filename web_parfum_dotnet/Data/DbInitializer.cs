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

    // ---------------------------------------------------------------------
    // Ek mock veri — SEED_MOCK=true ile tetiklenir (Program.cs).
    // İşaret: tüm mock kullanıcıların email'i "@mock.local" ile biter.
    // Idempotent: mock kullanıcılar zaten varsa hiçbir şey yapmaz.
    // ---------------------------------------------------------------------
    private const string MockEmailSuffix = "@mock.local";

    public static void SeedAdditionalMockData(AppDbContext db)
    {
        if (db.Users.Any(u => u.Email.EndsWith(MockEmailSuffix)))
        {
            Console.WriteLine("[MockSeed] Mock kullanıcılar zaten mevcut, ekleme atlanıyor.");
            return;
        }

        // Parfümlere ihtiyacımız var; CSV seed edilmemişse anlamlı veri üretemeyiz.
        var perfumes = db.Perfumes.OrderBy(p => p.Id).Take(60).ToList();
        if (perfumes.Count < 30)
        {
            Console.WriteLine("[MockSeed] Yeterli parfüm yok, mock veri atlanıyor.");
            return;
        }

        var p = perfumes;
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var now = DateTime.UtcNow;

        // --- 10 yeni kullanıcı (her birinin belirgin bir koku karakteri var) ---
        var users = new List<User>
        {
            new() { Username = "deniz",   Email = "deniz@mock.local",   PasswordHash = hash, Bio = "Aquatik ve marine kokuların peşinde bir deniz tutkunu.", CreatedAt = now.AddDays(-40) },
            new() { Username = "selin",   Email = "selin@mock.local",   PasswordHash = hash, Bio = "Gurmand ve tatlı kokular benim zaafım.", CreatedAt = now.AddDays(-38) },
            new() { Username = "kerem",   Email = "kerem@mock.local",   PasswordHash = hash, Bio = "Odunsu ve deri notalar, klasik bir erkek.", CreatedAt = now.AddDays(-35) },
            new() { Username = "ece",     Email = "ece@mock.local",     PasswordHash = hash, Bio = "Pudramsı ve iris bazlı zarafet arıyorum.", CreatedAt = now.AddDays(-33) },
            new() { Username = "burak",   Email = "burak@mock.local",   PasswordHash = hash, Bio = "Baharatlı ve füme kokuların hayranıyım.", CreatedAt = now.AddDays(-30) },
            new() { Username = "irem",    Email = "irem@mock.local",    PasswordHash = hash, Bio = "Beyaz çiçekler ve yaseminle büyülenirim.", CreatedAt = now.AddDays(-28) },
            new() { Username = "tolga",   Email = "tolga@mock.local",   PasswordHash = hash, Bio = "Fougère ve aromatik kokular, sade ama güçlü.", CreatedAt = now.AddDays(-25) },
            new() { Username = "pelin",   Email = "pelin@mock.local",   PasswordHash = hash, Bio = "Meyvemsi ve neşeli kokular her günümü renklendiriyor.", CreatedAt = now.AddDays(-22) },
            new() { Username = "onur",    Email = "onur@mock.local",    PasswordHash = hash, Bio = "Tütün, vanilya ve sıcak kokular akşamların efendisi.", CreatedAt = now.AddDays(-20) },
            new() { Username = "melis",   Email = "melis@mock.local",   PasswordHash = hash, Bio = "Yeşil, çimensi ve toprak kokuları doğayı hatırlatıyor.", CreatedAt = now.AddDays(-18) },
        };
        db.Users.AddRange(users);
        db.SaveChanges();
        var u = users;

        // --- Collections ---
        // Her kullanıcı 3-5 parfüm: bazısı owned (şişe seviyesi ile), wishlist, tried.
        var collections = new List<Collection>
        {
            // deniz (0) — aquatik
            new() { UserId = u[0].Id, PerfumeId = p[21].Id, Status = "owned",    BottleLevel = 75, CreatedAt = now.AddDays(-39), UpdatedAt = now.AddDays(-39) },
            new() { UserId = u[0].Id, PerfumeId = p[22].Id, Status = "owned",    BottleLevel = 40, CreatedAt = now.AddDays(-38), UpdatedAt = now.AddDays(-10) },
            new() { UserId = u[0].Id, PerfumeId = p[23].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-30) },
            new() { UserId = u[0].Id, PerfumeId = p[24].Id, Status = "tried",    BottleLevel = 0,  CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-15) },

            // selin (1) — gurmand
            new() { UserId = u[1].Id, PerfumeId = p[25].Id, Status = "owned",    BottleLevel = 90, CreatedAt = now.AddDays(-37), UpdatedAt = now.AddDays(-37) },
            new() { UserId = u[1].Id, PerfumeId = p[26].Id, Status = "owned",    BottleLevel = 55, CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-8) },
            new() { UserId = u[1].Id, PerfumeId = p[27].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-20) },

            // kerem (2) — odunsu/deri
            new() { UserId = u[2].Id, PerfumeId = p[28].Id, Status = "owned",    BottleLevel = 65, CreatedAt = now.AddDays(-34), UpdatedAt = now.AddDays(-34) },
            new() { UserId = u[2].Id, PerfumeId = p[29].Id, Status = "owned",    BottleLevel = 30, CreatedAt = now.AddDays(-33), UpdatedAt = now.AddDays(-5) },
            new() { UserId = u[2].Id, PerfumeId = p[30].Id, Status = "tried",    BottleLevel = 0,  CreatedAt = now.AddDays(-12), UpdatedAt = now.AddDays(-12) },
            new() { UserId = u[2].Id, PerfumeId = p[31].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-11), UpdatedAt = now.AddDays(-11) },

            // ece (3) — pudramsı/iris
            new() { UserId = u[3].Id, PerfumeId = p[32].Id, Status = "owned",    BottleLevel = 85, CreatedAt = now.AddDays(-32), UpdatedAt = now.AddDays(-32) },
            new() { UserId = u[3].Id, PerfumeId = p[33].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-18), UpdatedAt = now.AddDays(-18) },
            new() { UserId = u[3].Id, PerfumeId = p[34].Id, Status = "tried",    BottleLevel = 0,  CreatedAt = now.AddDays(-9),  UpdatedAt = now.AddDays(-9) },

            // burak (4) — baharatlı/füme
            new() { UserId = u[4].Id, PerfumeId = p[35].Id, Status = "owned",    BottleLevel = 50, CreatedAt = now.AddDays(-29), UpdatedAt = now.AddDays(-29) },
            new() { UserId = u[4].Id, PerfumeId = p[36].Id, Status = "owned",    BottleLevel = 20, CreatedAt = now.AddDays(-27), UpdatedAt = now.AddDays(-3) },
            new() { UserId = u[4].Id, PerfumeId = p[37].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-14), UpdatedAt = now.AddDays(-14) },

            // irem (5) — beyaz çiçek
            new() { UserId = u[5].Id, PerfumeId = p[38].Id, Status = "owned",    BottleLevel = 70, CreatedAt = now.AddDays(-26), UpdatedAt = now.AddDays(-26) },
            new() { UserId = u[5].Id, PerfumeId = p[39].Id, Status = "tried",    BottleLevel = 0,  CreatedAt = now.AddDays(-13), UpdatedAt = now.AddDays(-13) },
            new() { UserId = u[5].Id, PerfumeId = p[40].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-10) },

            // tolga (6) — fougère
            new() { UserId = u[6].Id, PerfumeId = p[41].Id, Status = "owned",    BottleLevel = 95, CreatedAt = now.AddDays(-24), UpdatedAt = now.AddDays(-24) },
            new() { UserId = u[6].Id, PerfumeId = p[42].Id, Status = "owned",    BottleLevel = 60, CreatedAt = now.AddDays(-22), UpdatedAt = now.AddDays(-4) },

            // pelin (7) — meyvemsi
            new() { UserId = u[7].Id, PerfumeId = p[43].Id, Status = "owned",    BottleLevel = 45, CreatedAt = now.AddDays(-21), UpdatedAt = now.AddDays(-21) },
            new() { UserId = u[7].Id, PerfumeId = p[44].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-16), UpdatedAt = now.AddDays(-16) },
            new() { UserId = u[7].Id, PerfumeId = p[45].Id, Status = "tried",    BottleLevel = 0,  CreatedAt = now.AddDays(-7),  UpdatedAt = now.AddDays(-7) },

            // onur (8) — tütün/vanilya
            new() { UserId = u[8].Id, PerfumeId = p[46].Id, Status = "owned",    BottleLevel = 80, CreatedAt = now.AddDays(-19), UpdatedAt = now.AddDays(-19) },
            new() { UserId = u[8].Id, PerfumeId = p[47].Id, Status = "owned",    BottleLevel = 35, CreatedAt = now.AddDays(-17), UpdatedAt = now.AddDays(-2) },
            new() { UserId = u[8].Id, PerfumeId = p[48].Id, Status = "wishlist", BottleLevel = 0,  CreatedAt = now.AddDays(-6),  UpdatedAt = now.AddDays(-6) },

            // melis (9) — yeşil/toprak
            new() { UserId = u[9].Id, PerfumeId = p[49].Id, Status = "owned",    BottleLevel = 55, CreatedAt = now.AddDays(-17), UpdatedAt = now.AddDays(-17) },
            new() { UserId = u[9].Id, PerfumeId = p[50].Id, Status = "tried",    BottleLevel = 0,  CreatedAt = now.AddDays(-8),  UpdatedAt = now.AddDays(-8) },
        };
        db.Collections.AddRange(collections);
        db.SaveChanges();

        // --- Reviews (User+Perfume unique; her çift bir kez) ---
        var reviews = new List<Review>
        {
            new() { UserId = u[0].Id, PerfumeId = p[21].Id, Rating = 5, Body = "Tam bir yaz kokusu, deniz esintisini şişeye koymuşlar.", CreatedAt = now.AddDays(-36) },
            new() { UserId = u[0].Id, PerfumeId = p[22].Id, Rating = 4, Body = "Ferah ama biraz daha kalıcı olsa mükemmeldi.", CreatedAt = now.AddDays(-9) },
            new() { UserId = u[1].Id, PerfumeId = p[25].Id, Rating = 5, Body = "Vanilya ve karamel cenneti. Tatlı sevenlere bayram.", CreatedAt = now.AddDays(-33) },
            new() { UserId = u[1].Id, PerfumeId = p[26].Id, Rating = 4, Body = "Sıcak ve sarmalayıcı, kış akşamları için ideal.", CreatedAt = now.AddDays(-7) },
            new() { UserId = u[2].Id, PerfumeId = p[28].Id, Rating = 5, Body = "Deri ve odun dengesi muazzam. Karizmatik bir koku.", CreatedAt = now.AddDays(-30) },
            new() { UserId = u[2].Id, PerfumeId = p[29].Id, Rating = 3, Body = "İyi ama açılışı biraz keskin geldi bana.", CreatedAt = now.AddDays(-4) },
            new() { UserId = u[3].Id, PerfumeId = p[32].Id, Rating = 5, Body = "İris ve pudra zarafeti. Çok sofistike.", CreatedAt = now.AddDays(-28) },
            new() { UserId = u[4].Id, PerfumeId = p[35].Id, Rating = 4, Body = "Baharatlar güzel ama biraz ağır olabilir gündüz için.", CreatedAt = now.AddDays(-25) },
            new() { UserId = u[4].Id, PerfumeId = p[36].Id, Rating = 5, Body = "Füme ve odun, gece çıkışlarının vazgeçilmezi oldu.", CreatedAt = now.AddDays(-2) },
            new() { UserId = u[5].Id, PerfumeId = p[38].Id, Rating = 5, Body = "Yasemin patlaması! Çok feminen ve zarif.", CreatedAt = now.AddDays(-22) },
            new() { UserId = u[6].Id, PerfumeId = p[41].Id, Rating = 4, Body = "Klasik fougère, hiç eskimeyen bir imza.", CreatedAt = now.AddDays(-20) },
            new() { UserId = u[6].Id, PerfumeId = p[42].Id, Rating = 4, Body = "Aromatik ve temiz, ofis için tam isabet.", CreatedAt = now.AddDays(-3) },
            new() { UserId = u[7].Id, PerfumeId = p[43].Id, Rating = 4, Body = "Meyvemsi ve neşeli, modumu hep yükseltiyor.", CreatedAt = now.AddDays(-18) },
            new() { UserId = u[8].Id, PerfumeId = p[46].Id, Rating = 5, Body = "Tütün ve vanilya, sıcacık bir sarılma gibi.", CreatedAt = now.AddDays(-16) },
            new() { UserId = u[8].Id, PerfumeId = p[47].Id, Rating = 4, Body = "Akşamları çok yakışıyor, kalıcılığı da iyi.", CreatedAt = now.AddDays(-2) },
            new() { UserId = u[9].Id, PerfumeId = p[49].Id, Rating = 4, Body = "Yeşil ve toprak kokusu, orman yürüyüşü gibi.", CreatedAt = now.AddDays(-15) },
            // Çapraz görüşler — başkalarının sahip olduğu parfümleri deneyip yorumlamış
            new() { UserId = u[1].Id, PerfumeId = p[21].Id, Rating = 3, Body = "Aquatik benim tarzım değil ama kaliteli bir iş.", CreatedAt = now.AddDays(-12) },
            new() { UserId = u[8].Id, PerfumeId = p[28].Id, Rating = 5, Body = "Kerem haklı, bu deri kokusu efsane.", CreatedAt = now.AddDays(-10) },
        };
        db.Reviews.AddRange(reviews);
        db.SaveChanges();

        // --- DailyLogs ---
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyLogs = new List<DailyLog>
        {
            new() { UserId = u[0].Id, PerfumeId = p[21].Id, Date = today,             Sprays = 3, Note = "Sahil yürüyüşü.", CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[22].Id, Date = today.AddDays(-2), Sprays = 2, Note = null, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[25].Id, Date = today,             Sprays = 4, Note = "Tatlı bir gün istedim.", CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[26].Id, Date = today.AddDays(-1), Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[28].Id, Date = today,             Sprays = 2, Note = "İş görüşmesi.", CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[29].Id, Date = today.AddDays(-3), Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[32].Id, Date = today,             Sprays = 2, Note = "Davet.", CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[36].Id, Date = today,             Sprays = 4, Note = "Gece çıkışı.", CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[35].Id, Date = today.AddDays(-2), Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[38].Id, Date = today,             Sprays = 3, Note = "İlkbahar havası.", CreatedAt = now },
            new() { UserId = u[6].Id, PerfumeId = p[41].Id, Date = today.AddDays(-1), Sprays = 2, Note = null, CreatedAt = now },
            new() { UserId = u[7].Id, PerfumeId = p[43].Id, Date = today,             Sprays = 3, Note = "Neşeli bir gün!", CreatedAt = now },
            new() { UserId = u[8].Id, PerfumeId = p[46].Id, Date = today,             Sprays = 4, Note = "Akşam buluşması.", CreatedAt = now },
            new() { UserId = u[8].Id, PerfumeId = p[47].Id, Date = today.AddDays(-2), Sprays = 3, Note = null, CreatedAt = now },
            new() { UserId = u[9].Id, PerfumeId = p[49].Id, Date = today.AddDays(-1), Sprays = 2, Note = "Doğa yürüyüşü.", CreatedAt = now },
        };
        db.DailyLogs.AddRange(dailyLogs);
        db.SaveChanges();

        // --- Likes (User+Perfume unique) ---
        var likes = new List<Like>
        {
            new() { UserId = u[0].Id, PerfumeId = p[21].Id, CreatedAt = now },
            new() { UserId = u[0].Id, PerfumeId = p[23].Id, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[25].Id, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[27].Id, CreatedAt = now },
            new() { UserId = u[1].Id, PerfumeId = p[21].Id, CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[28].Id, CreatedAt = now },
            new() { UserId = u[2].Id, PerfumeId = p[31].Id, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[32].Id, CreatedAt = now },
            new() { UserId = u[3].Id, PerfumeId = p[33].Id, CreatedAt = now },
            new() { UserId = u[4].Id, PerfumeId = p[36].Id, CreatedAt = now },
            new() { UserId = u[5].Id, PerfumeId = p[38].Id, CreatedAt = now },
            new() { UserId = u[6].Id, PerfumeId = p[41].Id, CreatedAt = now },
            new() { UserId = u[7].Id, PerfumeId = p[43].Id, CreatedAt = now },
            new() { UserId = u[8].Id, PerfumeId = p[46].Id, CreatedAt = now },
            new() { UserId = u[8].Id, PerfumeId = p[28].Id, CreatedAt = now },
            new() { UserId = u[9].Id, PerfumeId = p[49].Id, CreatedAt = now },
        };
        db.Likes.AddRange(likes);
        db.SaveChanges();

        // --- Follows (mock kullanıcılar arası bir sosyal ağ) ---
        var follows = new List<Follow>
        {
            new() { FollowerId = u[0].Id, FollowingId = u[1].Id, CreatedAt = now.AddDays(-30) },
            new() { FollowerId = u[0].Id, FollowingId = u[2].Id, CreatedAt = now.AddDays(-29) },
            new() { FollowerId = u[1].Id, FollowingId = u[0].Id, CreatedAt = now.AddDays(-28) },
            new() { FollowerId = u[1].Id, FollowingId = u[7].Id, CreatedAt = now.AddDays(-27) },
            new() { FollowerId = u[2].Id, FollowingId = u[8].Id, CreatedAt = now.AddDays(-26) },
            new() { FollowerId = u[3].Id, FollowingId = u[5].Id, CreatedAt = now.AddDays(-25) },
            new() { FollowerId = u[4].Id, FollowingId = u[8].Id, CreatedAt = now.AddDays(-24) },
            new() { FollowerId = u[5].Id, FollowingId = u[3].Id, CreatedAt = now.AddDays(-23) },
            new() { FollowerId = u[6].Id, FollowingId = u[4].Id, CreatedAt = now.AddDays(-22) },
            new() { FollowerId = u[7].Id, FollowingId = u[1].Id, CreatedAt = now.AddDays(-21) },
            new() { FollowerId = u[8].Id, FollowingId = u[2].Id, CreatedAt = now.AddDays(-20) },
            new() { FollowerId = u[9].Id, FollowingId = u[0].Id, CreatedAt = now.AddDays(-19) },
        };
        db.Follows.AddRange(follows);
        db.SaveChanges();

        // --- Communities (mock kullanıcılar tarafından kurulan) ---
        var aquaticCommunity = new Community
        {
            Name = "Aquatik & Marine",
            Description = "Deniz, tuz ve ferahlık. Yaz kokularının buluşma noktası.",
            OwnerId = u[0].Id,
            CreatedAt = now.AddDays(-30),
        };
        var gurmandCommunity = new Community
        {
            Name = "Tatlı Kaçamaklar",
            Description = "Vanilya, karamel, çikolata — gurmand kokuların tatlı dünyası.",
            OwnerId = u[1].Id,
            CreatedAt = now.AddDays(-28),
        };
        db.Communities.AddRange(aquaticCommunity, gurmandCommunity);
        db.SaveChanges();

        // --- CommunityMembers (Community+User unique) ---
        var members = new List<CommunityMember>
        {
            new() { CommunityId = aquaticCommunity.Id, UserId = u[0].Id, Role = "admin",  JoinedAt = now.AddDays(-30) },
            new() { CommunityId = aquaticCommunity.Id, UserId = u[6].Id, Role = "member", JoinedAt = now.AddDays(-24) },
            new() { CommunityId = aquaticCommunity.Id, UserId = u[9].Id, Role = "member", JoinedAt = now.AddDays(-18) },
            new() { CommunityId = aquaticCommunity.Id, UserId = u[1].Id, Role = "member", JoinedAt = now.AddDays(-15) },
            new() { CommunityId = gurmandCommunity.Id, UserId = u[1].Id, Role = "admin",  JoinedAt = now.AddDays(-28) },
            new() { CommunityId = gurmandCommunity.Id, UserId = u[7].Id, Role = "member", JoinedAt = now.AddDays(-20) },
            new() { CommunityId = gurmandCommunity.Id, UserId = u[8].Id, Role = "member", JoinedAt = now.AddDays(-17) },
        };
        db.CommunityMembers.AddRange(members);
        db.SaveChanges();

        // --- CommunityPosts ---
        var posts = new List<CommunityPost>
        {
            new() { CommunityId = aquaticCommunity.Id, UserId = u[0].Id, PerfumeId = p[21].Id, Body = "Bu sezonun favorisi, sahilde sürdüğünüzde tam oturuyor.", CreatedAt = now.AddDays(-26) },
            new() { CommunityId = aquaticCommunity.Id, UserId = u[6].Id, PerfumeId = null,      Body = "Aquatik kokularda kalıcılık hep sorun oluyor, öneriniz var mı?", CreatedAt = now.AddDays(-23) },
            new() { CommunityId = aquaticCommunity.Id, UserId = u[9].Id, PerfumeId = p[24].Id, Body = "Bunu denedim, beklediğimden daha odunsu çıktı ama hoşuma gitti.", CreatedAt = now.AddDays(-12) },
            new() { CommunityId = gurmandCommunity.Id, UserId = u[1].Id, PerfumeId = p[25].Id, Body = "Tatlı sevenler buraya! Bu vanilya bombası kesinlikle denenmeli.", CreatedAt = now.AddDays(-24) },
            new() { CommunityId = gurmandCommunity.Id, UserId = u[8].Id, PerfumeId = p[46].Id, Body = "Tütün-vanilya kombinasyonu gurmandın en olgun hali bence.", CreatedAt = now.AddDays(-16) },
            new() { CommunityId = gurmandCommunity.Id, UserId = u[7].Id, PerfumeId = null,      Body = "Yazın gurmand giyilir mi? Bence hafif meyvemsi olanlar olur.", CreatedAt = now.AddDays(-9) },
        };
        db.CommunityPosts.AddRange(posts);
        db.SaveChanges();

        // --- Notifications (follow/like/review olaylarını yansıtır) ---
        var notifications = new List<Notification>
        {
            new() { RecipientId = u[1].Id, ActorId = u[0].Id, Type = "follow", IsRead = true,  CreatedAt = now.AddDays(-30) },
            new() { RecipientId = u[0].Id, ActorId = u[1].Id, Type = "follow", IsRead = true,  CreatedAt = now.AddDays(-28) },
            new() { RecipientId = u[8].Id, ActorId = u[2].Id, Type = "follow", IsRead = false, CreatedAt = now.AddDays(-26) },
            new() { RecipientId = u[0].Id, ActorId = u[9].Id, Type = "follow", IsRead = false, CreatedAt = now.AddDays(-19) },
            new() { RecipientId = u[0].Id, ActorId = u[1].Id, Type = "like",   PerfumeId = p[21].Id, IsRead = true,  CreatedAt = now.AddDays(-12) },
            new() { RecipientId = u[2].Id, ActorId = u[8].Id, Type = "like",   PerfumeId = p[28].Id, IsRead = false, CreatedAt = now.AddDays(-10) },
            new() { RecipientId = u[0].Id, ActorId = u[1].Id, Type = "review", PerfumeId = p[21].Id, IsRead = false, CreatedAt = now.AddDays(-12) },
            new() { RecipientId = u[2].Id, ActorId = u[8].Id, Type = "review", PerfumeId = p[28].Id, IsRead = false, CreatedAt = now.AddDays(-10) },
            new() { RecipientId = u[0].Id, ActorId = u[6].Id, Type = "community_invite", CommunityId = aquaticCommunity.Id, IsRead = true, CreatedAt = now.AddDays(-24) },
            new() { RecipientId = u[1].Id, ActorId = u[7].Id, Type = "community_invite", CommunityId = gurmandCommunity.Id, IsRead = false, CreatedAt = now.AddDays(-20) },
        };
        db.Notifications.AddRange(notifications);
        db.SaveChanges();

        // --- Messages (takipleşen kullanıcılar arası sohbetler) ---
        var messages = new List<Message>
        {
            new() { SenderId = u[0].Id, RecipientId = u[1].Id, Body = "Selam! Gurmand topluluğun çok keyifli görünüyor.", IsRead = true,  CreatedAt = now.AddDays(-27).AddMinutes(-30) },
            new() { SenderId = u[1].Id, RecipientId = u[0].Id, Body = "Teşekkürler! Sen de aquatik tarafında işin ehlisin :)", IsRead = true,  CreatedAt = now.AddDays(-27).AddMinutes(-20) },
            new() { SenderId = u[0].Id, RecipientId = u[1].Id, Body = "O vanilyalı kokuyu denedim, gerçekten iddialıymış.", IsRead = false, CreatedAt = now.AddDays(-12).AddMinutes(-10) },

            new() { SenderId = u[2].Id, RecipientId = u[8].Id, Body = "Deri kokular konusunda senin önerilerine güveniyorum, yeni bir şey var mı?", IsRead = true,  CreatedAt = now.AddDays(-25).AddMinutes(-40) },
            new() { SenderId = u[8].Id, RecipientId = u[2].Id, Body = "Tütün bazlı bir şişe aldım geçen, sana da uyar bence.", IsRead = true,  CreatedAt = now.AddDays(-25).AddMinutes(-25) },

            new() { SenderId = u[4].Id, RecipientId = u[8].Id, Body = "Akşam kokuları için bir liste yapsak mı topluluğa?", IsRead = false, CreatedAt = now.AddDays(-15).AddMinutes(-15) },
        };
        db.Messages.AddRange(messages);
        db.SaveChanges();

        Console.WriteLine("[MockSeed] Ek mock veri eklendi: 10 kullanıcı, koleksiyonlar, yorumlar, loglar, beğeniler, takipler, 2 topluluk, postlar, bildirimler, mesajlar.");
    }

    // ---------------------------------------------------------------------
    // Mock veriyi ve ona FK ile bağlı TÜM kayıtları (sonradan üretilenler dahil)
    // doğru sırada siler. SEED_MOCK=clean ile tetiklenir.
    // Restrict olan FK'ler (Follow, Message, Notification.Actor, Community.Owner)
    // nedeniyle silme sırası kritik: önce çocuklar, en son User.
    // ---------------------------------------------------------------------
    public static void CleanMockData(AppDbContext db)
    {
        var mockUserIds = db.Users
            .Where(u => u.Email.EndsWith(MockEmailSuffix))
            .Select(u => u.Id)
            .ToList();

        if (mockUserIds.Count == 0)
        {
            Console.WriteLine("[MockClean] Silinecek mock kullanıcı bulunamadı.");
            return;
        }

        var ids = mockUserIds.ToHashSet();

        // Mock kullanıcıların sahip olduğu topluluklar (ve onlara ait üye/post/bildirimler).
        var mockCommunityIds = db.Communities
            .Where(c => ids.Contains(c.OwnerId))
            .Select(c => c.Id)
            .ToList();
        var commIds = mockCommunityIds.ToHashSet();

        // 1) Messages — gönderen VEYA alıcısı mock olan tüm mesajlar
        db.Messages.RemoveRange(db.Messages.Where(m => ids.Contains(m.SenderId) || ids.Contains(m.RecipientId)));

        // 2) Notifications — alıcısı/aktörü mock olan VEYA mock topluluğa ait
        db.Notifications.RemoveRange(db.Notifications.Where(n =>
            ids.Contains(n.RecipientId) || ids.Contains(n.ActorId) ||
            (n.CommunityId != null && commIds.Contains(n.CommunityId.Value))));

        // 3) CommunityPosts — mock kullanıcının yazdığı VEYA mock topluluktaki tüm postlar
        db.CommunityPosts.RemoveRange(db.CommunityPosts.Where(cp =>
            ids.Contains(cp.UserId) || commIds.Contains(cp.CommunityId)));

        // 4) CommunityMembers — mock kullanıcının üyelikleri VEYA mock topluluğun üyeleri
        db.CommunityMembers.RemoveRange(db.CommunityMembers.Where(cm =>
            ids.Contains(cm.UserId) || commIds.Contains(cm.CommunityId)));

        // 5) Communities — mock kullanıcının sahip olduğu topluluklar
        db.Communities.RemoveRange(db.Communities.Where(c => commIds.Contains(c.Id)));

        // 6) Follows — follower VEYA following mock olan
        db.Follows.RemoveRange(db.Follows.Where(f => ids.Contains(f.FollowerId) || ids.Contains(f.FollowingId)));

        // 7) Likes
        db.Likes.RemoveRange(db.Likes.Where(l => ids.Contains(l.UserId)));

        // 8) Reviews
        db.Reviews.RemoveRange(db.Reviews.Where(r => ids.Contains(r.UserId)));

        // 9) DailyLogs
        db.DailyLogs.RemoveRange(db.DailyLogs.Where(d => ids.Contains(d.UserId)));

        // 10) Collections
        db.Collections.RemoveRange(db.Collections.Where(c => ids.Contains(c.UserId)));

        db.SaveChanges();

        // 11) En son: mock kullanıcıların kendisi
        db.Users.RemoveRange(db.Users.Where(u => ids.Contains(u.Id)));
        db.SaveChanges();

        Console.WriteLine($"[MockClean] {mockUserIds.Count} mock kullanıcı ve bağlı tüm veriler silindi.");
    }
}
