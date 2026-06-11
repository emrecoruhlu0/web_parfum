# Koku.

Parfüm meraklıları için Türkçe arayüzlü, full-stack sosyal platform. Kullanıcılar parfümleri keşfedebilir, koleksiyonlarına ekleyip günlük kullanımlarını takip edebilir, birbirini takip edip topluluklar kurabilir. Beğeni ve koleksiyon sinyallerinden çıkarılan **koku profili** ile kişiselleştirilmiş parfüm önerileri sunulur.

> Üniversite Web Programlama dersi projesi (3. sınıf, 2. dönem).

---

## Tech Stack

| Katman | Teknoloji |
|--------|-----------|
| Framework | ASP.NET Core MVC (.NET 9.0) |
| Veritabanı | PostgreSQL 17 |
| ORM | Entity Framework Core (Npgsql) |
| View | Razor Views (.cshtml) |
| Auth | Cookie tabanlı authentication (7 gün, sliding expiration) |
| Şifreleme | BCrypt.Net |
| Arama | PostgreSQL `pg_trgm` ile fuzzy/bulanık eşleştirme |
| Deployment | Docker Compose + GitHub Actions → VDS auto-deploy |

> Not: Projenin orijinal React + Vite + Tailwind / Node + Express + Prisma + SQLite versiyonu `archive/` altında arşivlenmiştir. **Aktif kod tabanı `web_parfum_dotnet/` içindeki .NET MVC sürümüdür.**

---

## Özellikler

- **Parfüm kataloğu** — Markalar, notalar (üst/orta/baz), akorlar, cinsiyet, yıl ve puanlama ile detaylı parfüm sayfaları.
- **Bulanık arama** — `pg_trgm` ile yazım hatalarına toleranslı, AJAX ile yerinde filtrelenen arama. Akor çiplerine tıklayarak filtreleme.
- **Koleksiyon yönetimi** — Bir parfümü "sahip olduğum / istek listem / denedim" gibi birden fazla durumla işaretleme.
- **Günlük log** — Hangi gün hangi parfümü kullandığını kaydetme.
- **Sosyal akış** — İnceleme yazma, beğeni, takip etme; kişiselleştirilmiş feed.
- **Topluluklar** — Topluluk oluşturma, üye olma, topluluk içi gönderiler.
- **Mesajlaşma & Bildirimler** — Kullanıcılar arası mesajlaşma ve okunmamış bildirim sayacı.
- **Koku profili & öneri motoru** — Beğeni, koleksiyon, günlük log ve öneri geri bildirimlerinden hesaplanan hibrit (içerik %70 + collaborative %30) öneri sistemi.

---

## Tasarım

Frontend yeniden tasarım planı ve tasarım sistemi: [`docs/DESIGN.md`](docs/DESIGN.md)  
Mevcut arayüz element envanteri: [`docs/ui-inventory.md`](docs/ui-inventory.md)

---

## Proje Yapısı

```
web_parfum/
├── web_parfum_dotnet/          # Aktif uygulama (ASP.NET Core MVC)
│   ├── Controllers/            # Auth, Feed, Perfumes, Profile, Collection,
│   │                           # Reviews, Likes, Follows, Communities,
│   │                           # Messages, Notifications, DailyLog,
│   │                           # RecommendationFeedback, Home
│   ├── Models/                 # User, Perfume, Collection, DailyLog, Review,
│   │                           # Like, Follow, Notification, Community,
│   │                           # CommunityMember, CommunityPost, Message,
│   │                           # RecommendationFeedback
│   ├── Services/               # UserTasteProfileService (öneri motoru),
│   │                           # NotificationService, CurrentUserService
│   ├── Data/                   # AppDbContext, DbInitializer, fra_cleaned.csv
│   ├── ViewModels/
│   ├── Views/                  # Razor views
│   ├── Migrations/             # EF Core migration'ları
│   ├── Program.cs
│   └── Dockerfile
├── landing/                    # Tanıtım/landing sayfası
├── archive/                    # Eski Node + React sürümü (arşiv)
├── docker-compose.yml          # Lokal geliştirme
├── docker-compose.prod.yml     # Production
└── .env.example
```

---

## Kurulum & Çalıştırma

### Gereksinimler
- [Docker](https://www.docker.com/) & Docker Compose
- (Alternatif: .NET 9.0 SDK + lokal PostgreSQL 17)

### 1. Ortam değişkenleri

`.env.example` dosyasını `.env` olarak kopyala ve değerleri düzenle:

```bash
cp .env.example .env
```

En azından `POSTGRES_PASSWORD` değerini ayarla.

### 2. Docker ile çalıştır (önerilen)

```bash
docker compose up --build
```

Bu komut PostgreSQL container'ını başlatır, uygulamayı build edip migration'ları/seed'i çalıştırır. Uygulama varsayılan olarak:

```
http://localhost:8080
```

adresinde açılır (`APP_PORT` ile değiştirilebilir).

### Mock veri (opsiyonel)

`docker-compose.yml` içindeki `SEED_MOCK` env değişkeni ile demo verisi yönetilir:

| Değer | Etki |
|-------|------|
| _(boş)_ | Sadece temel seed |
| `true` | Ek mock veri ekle (idempotent) |
| `clean` | Mock veriyi ve bağlı kayıtları geri al |

---

## Veritabanı

- **PostgreSQL 17**, Entity Framework Core ile yönetilir.
- Şema EF Core migration'ları (`web_parfum_dotnet/Migrations/`) üzerinden uygulanır; uygulama açılışında `DbInitializer` ile otomatik çalışır.
- Parfüm verisi `Data/fra_cleaned.csv` dosyasından seed edilir.
- Arama için `pg_trgm` extension'ı ve `GENERATED STORED` arama anahtarı kolonları kullanılır.

---

## Deployment

`production` branch'ine push → **GitHub Actions** → SSH ile VDS'e bağlanır → `docker compose -f docker-compose.prod.yml build/up` ile otomatik deploy yapılır. Production, Cloudflare/Nginx reverse proxy arkasında çalışacak şekilde (`ForwardedHeaders`) yapılandırılmıştır.

---

## Lisans

Eğitim amaçlı üniversite projesidir.
