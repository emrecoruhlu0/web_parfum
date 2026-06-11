# Koku. — Arayüz Element Envanteri (Remake Hazırlığı)

> Bu doküman, `web_parfum_dotnet` (ASP.NET Core MVC) arayüzünde kullanılan **tüm UI elementlerinin** sayfa
> sayfa dökümüdür. Amaç: detaylı bir remake (yeniden tasarım / farklı stack'e taşıma) öncesi tam bir
> referans çıkarmak. Her bölümde kaynak `.cshtml` / CSS / JS dosyalarına atıf vardır.
>
> **Yeni tasarım sistemi ve uygulama planı:** [DESIGN.md](./DESIGN.md)

İçindekiler:
1. [Global / Teknoloji Tabanı](#1-global--teknoloji-tabanı)
2. [Layout / Çatı Elementleri](#2-layout--çatı-elementleri)
3. [Atomik / Tekrar Eden Bileşenler](#3-atomik--tekrar-eden-bileşenler)
4. [Sayfa Sayfa Element Listesi](#4-sayfa-sayfa-element-listesi)
5. [JavaScript Davranış Envanteri](#5-javascript-davranış-envanteri)
6. [Remake İçin Kritik Notlar / Riskli Alanlar](#6-remake-i̇çin-kritik-notlar--riskli-alanlar)

---

## 1. Global / Teknoloji Tabanı

| Katman | Detay |
|---|---|
| CSS Framework | **Bootstrap 5.3.3** (CDN) |
| İkon seti | **Bootstrap Icons 1.11.3** (`bi-*`) |
| Fontlar | **Playfair Display** (serif başlıklar, `.font-serif`), **Inter** (gövde metni) |
| Placeholder görseller | `placehold.co` (marka baş harfleri ile dinamik üretilir) |
| Özel CSS | `wwwroot/css/site.css` (~1170 satır) |
| JS | `site.js`, `perfume-autocomplete.js`, Bootstrap bundle |

### Tasarım Token'ları (`:root`)
- `--parfum-amber: #d97706` — ana vurgu rengi (butonlar, aktif durumlar)
- `--parfum-dark: #1a1a2e` — sidebar / auth arka planı
- `--parfum-bg: #f8f5f0` — sayfa arka planı

### Renk paleti (CSS'ten türetilen)
| Amaç | Renk |
|---|---|
| Amber (primary) | `#d97706`, hover `#b45309` |
| Amber-soft zemin | `rgba(217,119,6,0.12)` |
| Success | `#16a34a`, soft `rgba(22,163,74,0.12)` |
| Koyu (dark) | `#1a1a2e` |
| Nötr metin | `#6b5a44`, `#9c8267`, `#888` |
| Kenarlık / hatlar | `#f0e8db`, `#ece1cf`, `#f0ece6` |

---

## 2. Layout / Çatı Elementleri

**Kaynak:** `Views/Shared/_Layout.cshtml`

| Element | CSS / Not |
|---|---|
| Sabit sol **Sidebar** | `.sidebar`, 240px, koyu lacivert; sadece giriş yapınca render edilir |
| Marka logosu | "Koku." serif (`.font-serif`) |
| Sidebar nav linkleri | `.sidebar-link`, `.active` — Ana Akış, Keşfet, Koleksiyon, Günlük Log, Topluluklar, Mesajlar, Bildirimler |
| Bildirim badge'i | Kırmızı pill, okunmamış sayısı (`ViewData["UnreadCount"]`) |
| Alt kullanıcı bloğu | Profil linki + **Çıkış** butonu (form + AntiForgeryToken) |
| **Mobil üst bar** | `.mobile-topbar` — Hamburger (`bi-list`), marka, bildirim zili |
| **Sidebar backdrop** | `.sidebar-backdrop` — mobil overlay |
| Auth-dışı `<main>` | Sidebar'sız sade gövde (login/register için) |
| Aktif sayfa işaretleme | `ViewData["ActivePage"]` ile sidebar link `.active` |

Responsive kırılımlar (`site.css`): `991.98px` (sidebar gizlenir, mobil bar gelir), `575.98px`, `359.98px`.

---

## 3. Atomik / Tekrar Eden Bileşenler

Tüm sayfalarda paylaşılan, remake'te **component** olarak çıkarılması gereken parçalar:

| Bileşen | CSS class | Kullanıldığı yer |
|---|---|---|
| Kart | `.parfum-card` | Her yerde temel konteyner |
| Amber buton | `.btn-amber` | Birincil aksiyon |
| Avatar (küçük/büyük) | `.profile-avatar-sm`, `.profile-avatar-lg` | Kullanıcı baş harfi, gradient |
| Topluluk avatarı | `.community-avatar`, `.community-avatar-lg` | Topluluk kartları |
| Parfüm thumbnail | `.perfume-thumb-sm`, `.perfume-card-img-wrap`, `.perfume-card-img` | Grid / liste |
| Yıldız rating (statik) | `.star`, `.star-empty` (`bi-star-fill` / `bi-star`) | Detay, profil, feed |
| Renk yardımcıları | `.text-amber`, `.bg-amber`, `.bg-amber-soft`, `.bg-success-soft`, `.text-success` | Her yerde |
| **Parfüm autocomplete** | `.perfume-autocomplete*`, `.perfume-chip-selected` | DailyLog, Feed, Community |
| **Not chip'leri** | `.note-chip` (`--note-color` CSS değişkeni) | Detay |
| **Akor chip'leri** | `.accord-chip` (renkli, linklenebilir) | Detay, filtreler, grid |
| Filtre chip'i | `.filter-chip`, `.filter-chip-color`, `.chip-count` | Keşfet |
| Alt-sekme pill grubu | `.subtab-buttons`, `.subtab-btn` | Profil koleksiyon |
| Boş durum (empty state) | `.text-center.py-5` + ikon + başlık + CTA | Neredeyse her liste |
| Onay diyalogu | `onclick="return confirm(...)"` | Silme işlemleri |
| Takvim hücresi | `.calendar-grid`, `.calendar-cell`, `.calendar-dot` | DailyLog |
| Sohbet balonu | `.chat-bubble-mine`, `.chat-bubble-theirs` | Mesajlar |
| Bildirim öğesi | `.notification-item`, `.notification-icon` | Bildirimler |
| Konuşma satırı | `.conversation-row` | Mesajlar listesi |
| Öneri feedback | `.rec-feedback`, `.rec-fb-btn` (👍/👎) | Feed, Profil-Koku Profili |

---

## 4. Sayfa Sayfa Element Listesi

### 4.1 Auth — `Auth/Login.cshtml`, `Auth/Register.cshtml`
- Tam ekran koyu arka plan (`.auth-container`) + ortalanmış beyaz kart (`.auth-card`)
- Marka başlığı + alt başlık
- Validation summary (`alert-danger`)
- Form alanları:
  - **Login:** E-posta, Şifre
  - **Register:** Kullanıcı Adı, E-posta, Şifre
- Amber submit butonu (tam genişlik)
- Çapraz link (Giriş ↔ Kayıt)

### 4.2 Ana Akış — `Feed/Index.cshtml`
- Başlık + "Paylaş" (modal tetikler) + "Keşfet" butonları
- **"Sana Özel Öneriler"** yatay kaydırmalı şerit (scroll-snap) — öneri kartları, uyum % badge, feedback (👍/👎)
- **Hızlı paylaşım kartı** (avatar + tetikleyici buton)
- **Aktivite akışı**: `review` / `community` / `log` tipli kartlar
  - Tip badge'leri (Yorum / Topluluk / Günlük Log)
  - Parfüm mini-kartı, yıldızlar, log meta (sıkım / tarih / not)
- Boş durumlar (takip yok / aktivite yok) + **Popüler Parfümler** fallback grid'i
- **Paylaş Modal** (sekmeli: Yorum / Günlük Log)
  - Parfüm autocomplete
  - **Tıklanabilir yıldız rating widget'ı (inline JS)**
  - Textarea (yorum), tarih, sıkım sayısı, not

### 4.3 Keşfet — `Perfumes/Index.cshtml` + `Perfumes/_PerfumeGrid.cshtml`
> En karmaşık sayfa — AJAX'lı in-place filtreleme.
- **Filtre kartı** (`.filter-card`):
  - Büyük arama input'u (debounce 300ms)
  - Sıralama select'i (Popüler / Puan / Yeni)
  - **Cinsiyet** renkli chip'leri (ikonlu, sayaçlı)
  - **Akor** renkli chip'leri + "daha fazla / az" collapse (12 görünür)
  - **Marka dropdown** (aranabilir, çoklu seçim, check işareti, sayaç, temizle)
  - **Aktif filtre çubuğu** (kaldırılabilir pill'ler + "Tümünü temizle")
- **Sonuç grid'i** (`_PerfumeGrid`): parfüm kartları (görsel hover zoom, akor badge, puan badge)
- Boş sonuç durumu
- **Pagination** (« sayfa numaraları »)

### 4.4 Parfüm Detay — `Perfumes/Details.cshtml`
- **Sol kolon:** büyük görsel + ad/marka, meta badge'ler (yıl / cinsiyet / ülke), puan badge, beğeni & koleksiyon sayaçları
  - **Beğen butonu** (AJAX toggle, ♥/♡)
  - **Koleksiyon durum butonları** (Sahibim / Denedim / İstek Listem — çoklu, toggle)
- **Sağ kolon:**
  - **Notalar** (Üst / Orta / Alt — renkli not chip'leri, ikonlu)
  - **Ana Akorlar** (tıklanabilir akor chip'leri → akora göre filtreli arama)
  - **Yorumlar:** yazma formu (select puan + textarea), yorum listesi, kendi yorumunu silme
- **Benzer Parfümler** grid'i (eşleşen akor badge'leri)

### 4.5 Yeni Parfüm — `Perfumes/Create.cshtml`
- Geniş form kartı: Ad, Marka, Ülke, Cinsiyet (select), Yıl, Görsel URL, 3 not alanı (üst/orta/alt), 5 akor alanı
- Validation summary + alan hataları, Kaydet / İptal

### 4.6 Koleksiyon — `Collection/Index.cshtml`
- Başlık + "Parfüm Bul"
- **Sekme pill'leri** (Sahibim / İstek Listem / Denedim — sayaç badge'li)
- Parfüm kartları grid'i
- **Şişe seviyesi slider'ı** (`range`, JS canlı %, Kaydet) — sadece "owned" sekmesinde
- Detay linki + Sil (onay diyaloglu)
- Boş durum

### 4.7 Günlük Log — `DailyLog/Index.cshtml`
> Görsel olarak en zengin sayfa.
- Ay navigasyonu (önceki / sonraki ay)
- **Takvim grid'i** (`.calendar-grid`):
  - Hücreler akor rengine boyalı, ikon, log noktaları (`.calendar-dot`), "+N" göstergesi
  - Bugün vurgusu, tooltip, tıklayınca modal açar
- **"Sahip Olduğun Parfümler"** hızlı-log kart grid'i (`.owned-grid`, akor renkli sol kenarlık)
- **Inline arama kısayolu** (autocomplete → seçince modal'a aktarır)
- Aylık log listesi (akor renkli kart + sil)
- **Log Ekle Modal** (tarih / parfüm autocomplete / sıkım / not) + prefill JS

### 4.8 Topluluklar — `Communities/Index.cshtml`, `Details.cshtml`, `Create.cshtml`
- **Index:** topluluk kartları grid'i (avatar, "Üyesin" badge, üye sayısı, Katıl / Görüntüle) + boş durum
- **Details:**
  - Başlık kartı (büyük avatar, üye / kurucu, Katıl / Ayrıl)
  - 2 kolon → **Gönderi oluşturma** (textarea + parfüm autocomplete) + gönderi akışı
  - **Üyeler listesi** (admin badge'li)
  - Gönderi silme (sahip / kurucu)
- **Create:** form (ad, açıklama, kapak URL) + geri butonu

### 4.9 Mesajlar — `Messages/Index.cshtml`, `Conversation.cshtml`
- **Index:** konuşma listesi (`.conversation-row` — avatar, son mesaj, okunmamış badge) + boş durum
- **Conversation:**
  - Başlık (geri + avatar + ad)
  - **Sohbet balonları** (`.chat-bubble-mine` / `-theirs`, saat damgası)
  - Gönderme formu (Enter ile gönder, Shift+Enter yeni satır, auto-scroll JS)

### 4.10 Bildirimler — `Notifications/Index.cshtml`
- Başlık + okunmamış badge + "Tümünü okundu işaretle"
- **Bildirim öğeleri** (`.notification-item.unread`):
  - Tipe göre ikon (follow / like / review / community_invite)
  - Aktör + metin, "Parfüme Git" badge, "Okundu işaretle"
- Önceki / Sonraki pagination + boş durum

### 4.11 Beğendiklerim — `Likes/Index.cshtml`
- Başlık + sayaç badge
- Yatay parfüm kartları grid'i (görsel + ad / marka / puan + ♥) + boş durum

### 4.12 Profil — `Profile/View.cshtml`, `Search.cshtml`, `Settings.cshtml`
- **View** (en kapsamlı sayfa):
  - **Sol kolon:** profil kartı (avatar, bio, Düzenle / Takip Et butonu), takipçi/takip sayaçları, istatistik kartı (koleksiyon/yorum/log), "En sık nota / akor" badge'leri
  - **Sağ kolon — sekmeler:** Koleksiyon / Günlük Loglar / Yorumlar / **Koku Profili**
    - Koleksiyon **subtab pill'leri** (`.subtab-buttons` — Sahip / İstek / Denedim)
    - **Koku Profili sekmesi:**
      - Sevilen akorlar **progress bar'ları** (amber gradient)
      - Kaçınılan akorlar progress bar'ları (kırmızı)
      - Favori nota badge'leri
      - **Kişisel öneri kartları** (uyum %, eşleşen akor badge, feedback 👍/👎)
- **Search:** arama kartı + kullanıcı sonuç kartları (Takip / Mesaj butonları, "Sen" badge)
- **Settings:** Username + Bio form'u + validation

### 4.13 Öneri Feedback Partial — `Shared/_RecommendationFeedback.cshtml`
- 👍 / 👎 toggle butonları (`.rec-fb-btn`)
- Davranış `_RecommendationFeedbackScripts.cshtml` (inline `<style>` + `<script>`) ile çalışır

### 4.14 Diğer / Yardımcı View'lar
- `Home/Index.cshtml`, `Home/Privacy.cshtml`
- `Shared/Error.cshtml`
- `Shared/_ValidationScriptsPartial.cshtml` (jQuery validation)

---

## 5. JavaScript Davranış Envanteri

Remake'te framework'e taşınacak **istemci tarafı davranışlar**:

| Davranış | Kaynak | Özet |
|---|---|---|
| **Like toggle (AJAX)** | `site.js` | `[data-like-toggle]` → `POST /Likes/Toggle`, buton + sayı günceller, 401'de login'e yönlendirir |
| **Parfüm autocomplete** | `perfume-autocomplete.js` | `[data-perfume-autocomplete]` → `GET /Perfumes/Search`, debounce 200ms, klavye nav (↑↓ Enter Esc), seçili chip + gizli input. `window.initPerfumeAutocomplete()` ile dinamik içerikte yeniden init |
| **Keşfet filtre motoru** | `Perfumes/Index.cshtml` (inline) | Set tabanlı çoklu filtre, `fetch` + `AbortController`, `history.replaceState`, chip/dropdown senkronu, aktif çubuk yeniden render, debounce arama |
| **Marka dropdown** | `Perfumes/Index.cshtml` (inline) | Aç/kapa, içinde arama filtresi, dış tıklamada kapanma |
| **Yıldız rating widget** | `Feed/Index.cshtml` (inline) | Hover/click ile dolu yıldız, gizli `rating` input |
| **DailyLog takvim modal** | `DailyLog/Index.cshtml` (inline) | Hücre tıklayınca tarih dolu modal; sahip kartı / inline arama → `prefillModalPerfume`; `MutationObserver` ile chip yakalama |
| **Şişe seviyesi slider** | `Collection/Index.cshtml` (inline) | `range` input → canlı yüzde label |
| **Öneri feedback (AJAX)** | `_RecommendationFeedbackScripts.cshtml` | `.rec-fb-btn` → `POST RecommendationFeedback/Submit`, "beğenmedim"de kartı fade-out + remove |
| **Mobil sidebar toggle** | `_Layout.cshtml` (inline) | Hamburger → sidebar + backdrop aç/kapa, link tıklayınca kapat |
| **Mesaj auto-scroll & Enter** | `Conversation.cshtml` (inline) | Liste en alta kaydırma, Enter ile gönder |

> Not: `_RecommendationFeedbackScripts.cshtml` içinde `.rec-fb-btn` için **inline `<style>** bloğu** da var
> (active renkleri vb.) — remake'te bu da taşınmalı.

---

## 6. Remake İçin Kritik Notlar / Riskli Alanlar

En çok dikkat gerektirecek, "yeniden yazılması zor" davranışsal parçalar:

1. **Keşfet AJAX filtre motoru** — Set tabanlı çoklu filtre, URL senkronu, AbortController.
   Component-temelli bir framework'e taşınırken state yönetimi tamamen değişir. En yüksek efor burada.
2. **Parfüm autocomplete** — 3 farklı sayfada chip mantığıyla kullanılıyor; tekrar kullanılabilir
   tek bir component'e indirgenmeli (`Feed`, `DailyLog`, `Community`).
3. **DailyLog takvim + modal prefill** — `MutationObserver`/observer pattern içeriyor; modern bir
   reaktif component'te bu mantık çok sadeleşir, ama davranış paritesi korunmalı.
4. **Yıldız rating widget'ı** — Feed modalında inline; tekrar kullanılabilir component olmalı
   (statik `.star` gösterimiyle birleştirilebilir).
5. **AccordVisual / NoteVisual helper'ları** (server-side, `Helpers/`) — Akor & nota → renk + ikon
   eşlemesi. Remake client-side olacaksa bu eşleme tablosu istemciye taşınmalı.
6. **Inline JS + inline `<style>` dağınıklığı** — Davranış ve stil birçok `.cshtml` içinde gömülü.
   Remake'te tek bir tasarım sistemine / component kütüphanesine toplanmalı.
7. **Placeholder görsel stratejisi** — `placehold.co` + `onerror` fallback her görselde tekrar ediyor;
   merkezi bir `<PerfumeImage>` component'i ile sadeleştirilebilir.

---

## 7. Sayfa → Kaynak Dosya Haritası (hızlı referans)

| Sayfa | View dosyası |
|---|---|
| Layout | `Views/Shared/_Layout.cshtml` |
| Giriş / Kayıt | `Views/Auth/Login.cshtml`, `Register.cshtml` |
| Ana Akış | `Views/Feed/Index.cshtml` |
| Keşfet | `Views/Perfumes/Index.cshtml`, `_PerfumeGrid.cshtml` |
| Parfüm Detay | `Views/Perfumes/Details.cshtml` |
| Yeni Parfüm | `Views/Perfumes/Create.cshtml` |
| Koleksiyon | `Views/Collection/Index.cshtml` |
| Günlük Log | `Views/DailyLog/Index.cshtml` |
| Topluluklar | `Views/Communities/Index.cshtml`, `Details.cshtml`, `Create.cshtml` |
| Mesajlar | `Views/Messages/Index.cshtml`, `Conversation.cshtml` |
| Bildirimler | `Views/Notifications/Index.cshtml` |
| Beğendiklerim | `Views/Likes/Index.cshtml` |
| Profil | `Views/Profile/View.cshtml`, `Search.cshtml`, `Settings.cshtml` |
| Öneri feedback | `Views/Shared/_RecommendationFeedback.cshtml`, `_RecommendationFeedbackScripts.cshtml` |
| Stil | `wwwroot/css/site.css` |
| JS | `wwwroot/js/site.js`, `perfume-autocomplete.js` |
