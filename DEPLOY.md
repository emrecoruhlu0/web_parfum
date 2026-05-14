# Deploy Rehberi

Bu döküman web_parfum_dotnet uygulamasını Linux sunucuya **Cloudflare proxy arkasında** deploy etmek içindir.

## Gereksinimler

- Sunucuda: Docker + Docker Compose, git
- Domain'in Cloudflare DNS'inde sunucuya yönlendirilmiş ve turuncu bulut (proxy) açık
- Cloudflare panelinde SSL/TLS mode: **Flexible** (Cloudflare ↔ ziyaretçi HTTPS, Cloudflare ↔ sunucu HTTP)

## Hızlı Kurulum

```bash
# 1. Repo'yu klonla
git clone <repo-url>
cd web_parfum

# 2. Env dosyasını hazırla
cp .env.example .env
nano .env
# POSTGRES_PASSWORD'ü güçlü bir parolayla değiştir
# APP_PORT'u 80 yapabilirsin (Cloudflare 80 → 443 proxy'ler)

# 3. Çalıştır
docker compose up -d --build

# 4. Logları izle (ilk seferinde seed 30-60 saniye sürer)
docker compose logs -f app
# "Now listening on: http://[::]:8080" görünce hazır
```

Şimdi `https://your-domain.com` üzerinden erişilebilir olmalı.

## Cloudflare Ayarları

### DNS
- `A` kaydı: `@` veya `www` → sunucu IP'si
- Bulut: **turuncu (proxied)**

### SSL/TLS
- Mode: **Flexible** (en kolay, sunucuda SSL gerekmez)
- Alternatif: **Full** mode + sunucuda Caddy self-signed sertifika (daha güvenli ama karmaşık)

### Sunucu firewall (UFW örneği)
```bash
# Sadece Cloudflare IP'lerinden 80 portuna izin ver (opsiyonel ama önerilen)
# https://www.cloudflare.com/ips-v4/ adresinden listeyi al
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw enable
```

Daha sıkı: yalnızca Cloudflare IP'lerinden 80'e izin ver (origin'i koruma).

## .env Örnek (Production)

```env
POSTGRES_PASSWORD=Px9$kL2#mN8@vQ5wR3
APP_PORT=80
ASPNETCORE_ENVIRONMENT=Production
```

`APP_PORT=80`: Cloudflare direkt 80'i proxy'ler. Eğer sunucuda başka web servisi yoksa pratik.

## Güncelleme

```bash
cd web_parfum
git pull
docker compose up -d --build
```

## Veritabanı Yedeği

```bash
# Backup al
docker exec web_parfum_db pg_dump -U postgres web_parfum > backup_$(date +%F).sql

# Geri yükle
cat backup.sql | docker exec -i web_parfum_db psql -U postgres web_parfum
```

## Sorun Giderme

**`docker compose up` başarısız:**
```bash
docker compose logs app    # uygulama logları
docker compose logs postgres
```

**"Password authentication failed"**: `.env` dosyasında parolayı set ettin mi? `docker compose down && docker compose up -d` ile yeniden başlat.

**Sayfa açılıyor ama login olamıyorum**: Cloudflare SSL/TLS mode'unu kontrol et. Flexible olmalı (veya Full + sunucuda SSL).

**500 hatası, log'da `redirect loop`**: `Program.cs`'te `UseHttpsRedirection` Production'da kapalı; eğer hala oluyorsa `ASPNETCORE_ENVIRONMENT=Production` env değişkenini kontrol et.

**Veritabanını sıfırla** (DİKKAT — tüm veri silinir):
```bash
docker compose down -v
docker compose up -d --build
```

## Mimari Notu

- App container: ASP.NET Core MVC, port 8080 (içerde), `${APP_PORT}` (dışarda)
- DB container: PostgreSQL 17, port 5432 sadece localhost'a expose (`127.0.0.1:5432`)
- Cloudflare → sunucu `${APP_PORT}` → app container 8080
- App, `ForwardedHeaders` middleware'i ile Cloudflare'in `X-Forwarded-Proto` header'ını okur, gerçek HTTPS sanır → cookie auth Secure flag'i doğru çalışır.
