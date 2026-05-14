# Deploy Rehberi

Bu döküman web_parfum_dotnet uygulamasını Linux sunucuya **Cloudflare proxy arkasında** ve **mevcut paylaşılan PostgreSQL container'ı** kullanarak deploy etmek içindir.

## İki kurulum senaryosu

| Senaryo | Compose dosyası | Postgres |
|---|---|---|
| **Lokal geliştirme** | `docker-compose.yml` | Yeni container açar |
| **Production (mevcut shared Postgres)** | `docker-compose.prod.yml` | `shared-db` network'undeki mevcut `postgres` container'ına bağlanır |

## Gereksinimler (Production)

- Sunucuda: Docker + Docker Compose, git
- `shared-db` adında external Docker network mevcut
- O network'te `postgres` adında çalışan bir Postgres container'ı
- Domain Cloudflare DNS'inde sunucuya yönlendirilmiş, turuncu bulut (proxy) açık
- Cloudflare SSL/TLS mode: **Flexible** (Cloudflare ↔ ziyaretçi HTTPS, Cloudflare ↔ sunucu HTTP)

## Hızlı Kurulum (Production)

### 1. Mevcut Postgres'te yeni bir database aç

App'in mevcut Postgres'in default `postgres` DB'sini kirletmemesi için ayrı bir DB açmak iyi pratiktir:

```bash
docker exec -it postgres psql -U postgres -c "CREATE DATABASE web_parfum;"
```

Eğer kendine ait bir Postgres user da açmak istersen (daha sıkı izolasyon):

```bash
docker exec -it postgres psql -U postgres <<EOF
CREATE USER web_parfum_app WITH PASSWORD 'app_user_strong_pw';
GRANT ALL PRIVILEGES ON DATABASE web_parfum TO web_parfum_app;
EOF
```

### 2. Repo'yu klonla ve .env'i hazırla

```bash
git clone <repo-url>
cd web_parfum
cp .env.example .env
nano .env
```

`.env`'de en az şunları set et:

```env
# Mevcut postgres container'ındaki parolayı yaz
POSTGRES_PASSWORD=<mevcut_postgres_parolasi>

# 1. adımda oluşturduğun DB
POSTGRES_DB=web_parfum

# postgres user'ı (default kullanıyorsan postgres, dedicated açtıysan web_parfum_app)
POSTGRES_USER=postgres

# App'in dışa açık olduğu port. Cloudflare proxy'lediğinden 80 kullanabilirsin.
APP_PORT=80

# Postgres container'ının adı (default: postgres)
DB_HOST=postgres
```

### 3. Çalıştır

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

İlk seferde build + 24k parfüm seed ~30-60 saniye sürer.

```bash
# Logları izle
docker compose -f docker-compose.prod.yml logs -f app
# "Now listening on: http://[::]:8080" görünce hazır
```

Şimdi `https://your-domain.com` erişilebilir.

## Cloudflare Ayarları

### DNS
- `A` kaydı: `@` veya `www` → sunucu IP'si
- Bulut: **turuncu (proxied)**

### SSL/TLS
- Mode: **Flexible**

### Sunucu firewall
```bash
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw enable
```

Daha sıkı: yalnızca Cloudflare IP'lerinden 80'e izin ver (`https://www.cloudflare.com/ips-v4/`).

## Güncelleme

```bash
cd web_parfum
git pull
docker compose -f docker-compose.prod.yml up -d --build
```

## Veritabanı Yedeği

```bash
# Sadece bizim app'in DB'sini yedekle
docker exec postgres pg_dump -U postgres web_parfum > backup_$(date +%F).sql

# Geri yükle
cat backup.sql | docker exec -i postgres psql -U postgres web_parfum
```

## Sorun Giderme

**"network shared-db not found"**: Sunucuda `shared-db` adında external network olduğundan emin ol:
```bash
docker network ls | grep shared-db
```

**"Password authentication failed"**: `.env`'deki `POSTGRES_PASSWORD` mevcut postgres container'ındaki parola ile aynı mı? Test et:
```bash
docker exec -e PGPASSWORD="<env'deki parola>" postgres psql -U postgres -d web_parfum -c "SELECT 1;"
```

**Sayfa açılıyor ama login olamıyorum**: Cloudflare SSL/TLS mode'unu kontrol et — Flexible olmalı.

**500 hatası, redirect loop**: `Program.cs`'te `UseHttpsRedirection` Production'da kapalı; ama emin olmak için `ASPNETCORE_ENVIRONMENT=Production` env değişkenini doğrula.

**Veritabanını sıfırla** (DİKKAT — sadece web_parfum DB'sini sıfırlar, diğer DB'lere dokunmaz):
```bash
docker compose -f docker-compose.prod.yml down
docker exec postgres psql -U postgres -c "DROP DATABASE web_parfum;"
docker exec postgres psql -U postgres -c "CREATE DATABASE web_parfum;"
docker compose -f docker-compose.prod.yml up -d --build
```

## Mimari Notu

```
[Ziyaretçi] → HTTPS → [Cloudflare] → HTTP → [Sunucu :80] → [web_parfum_app container :8080]
                                                                     ↓ (shared-db network)
                                                              [postgres container :5432]
```

- App container `shared-db` external network'üne join olur
- App, Postgres'i container adıyla bulur (`Host=postgres`)
- Postgres dışa expose olmaya gerek yok (sadece Docker internal ağda)
- App `${APP_PORT}` (default 80) ile dış dünyaya açılır
- `ForwardedHeaders` middleware Cloudflare'in `X-Forwarded-Proto: https` header'ını okur → cookie auth Secure flag'i doğru kurulur
