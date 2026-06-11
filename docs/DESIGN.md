# Koku. — Frontend Tasarım Sistemi & Yeniden Tasarım Planı

> **Sürüm:** 1.0 · **Tarih:** Haziran 2026  
> **Kapsam:** `web_parfum_dotnet` ASP.NET Core MVC arayüzünün tam frontend remake'i  
> **İlişkili dokümanlar:** [ui-inventory.md](./ui-inventory.md) (mevcut element envanteri), [README.md](../README.md)

---

## 1. Özet

**Koku.** parfüm meraklıları için Türkçe bir sosyal platformdur. Kullanıcılar parfüm keşfeder, koleksiyonlarını yönetir, günlük kullanımlarını kaydeder ve koku profillerine göre öneri alır.

Bu doküman, mevcut Bootstrap tabanlı arayüzün **baştan tasarlanması** için tek kaynak gerçeğidir (source of truth). Backend, API ve veri modeli değişmez; yalnızca görünüm, layout, etkileşim ve istemci tarafı davranış yeniden ele alınır.

### Tasarım tezi

> Parfüm e-ticaret vitrini değil; **kişisel koku defteri**.

Kullanıcı burada şişe satın almaz — kokusunu hatırlar, kaydeder ve paylaşır. Arayüz bu farkı hissettirmeli: kağıt dokusu, kayıt defteri hassasiyeti, akor renklerinin veri olarak konuşması.

### İmza öğe: *Sillage çizgisi*

Kullanıcının baskın akor renginden türetilen, sayfa sol kenarında 2–3px dikey gradient. Sidebar aktif durumu, profil ve ana akışta görünür. Dekor değil — `UserTasteProfileService` verisinin görsel yansıması.

### Bilinçli estetik risk

Koyu SaaS sidebar'ı (`#1a1a2e`) terk edilir. Yerine **açık kağıt tonlu navigasyon** gelir; aktif durum amber dolgu yerine **akor renkli sol kenar çizgisi** ile işaretlenir. Bu, parfüm dünyasına özgü veri-güdümlü navigasyondur ve generic dashboard kalıbından ayrışır.

---

## 2. Mevcut durumdan çıkış

| Mevcut | Sorun | Yeni yön |
|--------|-------|----------|
| `#f8f5f0` krem zemin | AI şablon kümesi #1 | `#F0EAE0` kağıt tonu, ince grain texture |
| Playfair Display | Her lüks parfüm sitesinde | **Fraunces** (display) |
| Inter | Nötr, tanıdık | **Figtree** (gövde) |
| `#d97706` amber | Tailwind amber-600 kopyası | **Reçine `#5C3D2E`** (oud/ahşap, mat) |
| Koyu sidebar | SaaS dashboard | Açık kağıt sidebar + sillage |
| Bootstrap default badge'ler | Detay sayfasında tutarsız | Tasarım sistemi badge'leri |

**Korunacaklar (değerli, domain-specific):**
- `AccordVisual` / `NoteVisual` renk eşlemeleri
- Günlük log takviminde akor renkli hücreler
- Keşfet AJAX filtre motoru (davranış aynı, görünüm yenilenir)
- Parfüm autocomplete chip mantığı
- Tüm sayfa işlevleri (`ui-inventory.md` referans)

---

## 3. Tasarım ilkeleri

1. **Veri = dekor.** Akor ve nota renkleri yalnızca gerçek parfüm verisini temsil eder; rastgele gradient veya stok fotoğraf kullanılmaz.
2. **Bir risk, geri kalan disiplin.** Sillage çizgisi ve açık sidebar cesur; tipografi, spacing ve butonlar sade kalır.
3. **Kayıt defteri netliği.** Tarih, uyum yüzdesi, log sayısı monospace ile; başlıklar editorial serif ile.
4. **Boşluk anlatır.** Boş liste = yönlendirme; hata = ne oldu + nasıl düzeltilir.
5. **Mobil önce değil, mobil eşit.** Günlük log takvimi ve keşfet filtreleri mobilde birincil kullanım senaryosu.

---

## 4. Renk sistemi

### 4.1 Marka token'ları (`:root`)

```css
:root {
  /* Zemin */
  --koku-paper:        #F0EAE0;   /* sayfa arka planı */
  --koku-paper-raised: #F7F3EC;   /* sidebar, hafif yükseltilmiş alanlar */
  --koku-surface:      #FFFFFF;   /* kartlar */
  --koku-mist:         #E4DDD2;   /* kenarlıklar, ayırıcılar */
  --koku-mist-strong:  #D4CBC0;   /* input border, pasif */

  /* Metin */
  --koku-ink:          #1A1614;   /* birincil metin */
  --koku-ink-soft:     #5C534A;   /* ikincil metin */
  --koku-ink-muted:    #8A8076;   /* etiket, placeholder */
  --koku-ink-faint:    #B0A89E;   /* devre dışı */

  /* Aksiyon */
  --koku-resin:        #5C3D2E;   /* birincil buton, aktif vurgu */
  --koku-resin-hover:  #4A3024;
  --koku-resin-soft:   rgba(92, 61, 46, 0.10);
  --koku-gold:         #A67C52;   /* ikincil vurgu, yıldız */
  --koku-gold-soft:    rgba(166, 124, 82, 0.14);

  /* Durum */
  --koku-success:      #3D6B4F;
  --koku-success-soft: rgba(61, 107, 79, 0.12);
  --koku-danger:       #9E3B30;
  --koku-danger-soft:  rgba(158, 59, 48, 0.10);
  --koku-info:         #3A6B7C;

  /* Gölge */
  --koku-shadow-sm:    0 1px 3px rgba(26, 22, 20, 0.06);
  --koku-shadow-md:    0 4px 16px rgba(26, 22, 20, 0.08);
  --koku-shadow-lg:    0 12px 40px rgba(26, 22, 20, 0.12);

  /* Radius */
  --koku-radius-sm:    8px;
  --koku-radius-md:    12px;
  --koku-radius-lg:    16px;
  --koku-radius-pill:  999px;

  /* Sillage (kullanıcı akoru — server'dan inline set edilir) */
  --koku-sillage:      var(--koku-resin); /* fallback */
}
```

### 4.2 Akor & nota renkleri

`Helpers/AccordVisual.cs` ve `Helpers/NoteVisual.cs` içindeki hex değerleri **değiştirilmez**. Bunlar domain veri görselleştirmesidir. Yeni tasarımda yalnızca chip, takvim hücresi ve filtre pill'lerinin **çerçeve/kontrast** kuralları güncellenir:

- Açık zeminli chip'lerde metin her zaman `#1A1614` veya kontrast hesabıyla `TextHex`
- Chip hover: `transform: translateY(-1px)` + `--koku-shadow-sm` (mevcut davranış korunur)
- Takvim hücresi: akor rengi `%18 opacity` arka plan + `%40 opacity` sol border

### 4.3 Koyu mod (Faz 2 — opsiyonel)

İlk remake light-only. Koyu mod planı:
- Zemin: `#141210`, yüzey: `#1E1B18`, kağıt metin: `#E8E2D8`
- Akor renkleri aynı hex, opacity artırılır

---

## 5. Tipografi

### 5.1 Font aileleri

| Rol | Font | Ağırlıklar | Kullanım |
|-----|------|-------------|----------|
| Display | [Fraunces](https://fonts.google.com/specimen/Fraunces) | 500, 600, 700 | Sayfa başlıkları, marka "Koku.", parfüm adları |
| Body | [Figtree](https://fonts.google.com/specimen/Figtree) | 400, 500, 600 | Gövde, buton, nav, form |
| Data | [IBM Plex Mono](https://fonts.google.com/specimen/IBM+Mono) | 400, 500 | Tarih, uyum %, log sayısı, takvim gün numarası |

**CDN (Layout `<head>`):**
```html
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link href="https://fonts.googleapis.com/css2?family=Fraunces:opsz,wght@9..144,500;9..144,600;9..144,700&family=Figtree:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500&display=swap" rel="stylesheet" />
```

### 5.2 Tip ölçeği

| Token | Boyut | Satır | Ağırlık | Kullanım |
|-------|-------|-------|---------|----------|
| `--text-display` | 2rem / 1.5rem mob | 1.15 | Fraunces 700 | H1 sayfa başlığı |
| `--text-title` | 1.375rem | 1.25 | Fraunces 600 | Kart başlığı, parfüm adı |
| `--text-subtitle` | 1.125rem | 1.3 | Figtree 600 | Bölüm başlığı |
| `--text-body` | 0.9375rem (15px) | 1.55 | Figtree 400 | Paragraf |
| `--text-small` | 0.8125rem (13px) | 1.45 | Figtree 400 | Meta, yardımcı |
| `--text-label` | 0.6875rem (11px) | 1.3 | Figtree 600 | Uppercase section label, letter-spacing 0.06em |
| `--text-data` | 0.8125rem | 1.4 | Plex Mono 500 | Sayılar, tarihler |

### 5.3 Sınıf eşlemesi (migration)

| Eski | Yeni |
|------|------|
| `.font-serif` | `.font-display` |
| Bootstrap `h1`–`h6` | Kontrollü kullanım; başlıklar `.font-display` |

---

## 6. Spacing & layout

### 6.1 Grid

- **Desktop (≥992px):** Sidebar 220px sabit + içerik `max-width: 1120px`, ortalanmış
- **Tablet (768–991px):** Sidebar overlay; içerik tam genişlik, padding 20px
- **Mobil (≤767px):** Sticky topbar 52px; içerik padding 16px; alt safe-area

### 6.2 Spacing ölçeği (4px taban)

`4 · 8 · 12 · 16 · 20 · 24 · 32 · 40 · 48 · 64`

Kart iç padding: `--space-5` (20px) desktop, `--space-4` (16px) mobil.

### 6.3 App shell

```
┌──────────┬────────────────────────────────────────────┐
│          │ ▌ sillage (2px, kullanıcı akoru)           │
│  Koku.   │                                            │
│          │  [Sayfa başlığı + aksiyonlar]              │
│  nav     │  ─────────────────────────────────────     │
│  links   │                                            │
│          │  [İçerik alanı]                            │
│          │                                            │
│  ──────  │                                            │
│  profil  │                                            │
│  çıkış   │                                            │
└──────────┴────────────────────────────────────────────┘
     220px                    fluid, max 1120px
```

**Sidebar özellikleri:**
- Arka plan: `--koku-paper-raised`
- Sağ border: 1px `--koku-mist`
- Link: Figtree 500, `--koku-ink-soft`; hover `--koku-ink` + `--koku-resin-soft` zemin
- Aktif: `--koku-ink` + sol 3px `var(--koku-sillage)` border + `--koku-resin-soft` zemin
- İkonlar: Bootstrap Icons (korunur), 1.1rem

**Mobil topbar:**
- Yükseklik 52px, `--koku-paper-raised` zemin, alt border `--koku-mist`
- Marka Fraunces; hamburger + bildirim zili

---

## 7. Bileşen kütüphanesi

Her bileşen remake'te aynı Razor partial veya CSS sınıfı olarak kalabilir; isimler güncellenir.

### 7.1 Kart — `.koku-card` (eski: `.parfum-card`)

```css
.koku-card {
  background: var(--koku-surface);
  border: 1px solid var(--koku-mist);
  border-radius: var(--koku-radius-md);
  box-shadow: var(--koku-shadow-sm);
  transition: box-shadow 0.2s ease;
}
.koku-card:hover {
  box-shadow: var(--koku-shadow-md);
}
```

Kullanım: feed öğeleri, profil kartı, filtre paneli, topluluk kartı.

### 7.2 Birincil buton — `.btn-koku` (eski: `.btn-amber`)

- Arka plan `--koku-resin`, metin `#fff`
- Hover `--koku-resin-hover`
- Focus: 2px outline `--koku-gold`, offset 2px
- Padding: 10px 20px; radius `--koku-radius-pill` veya `--koku-radius-sm` (bağlama göre)

**İkincil:** `.btn-koku-ghost` — şeffaf, border `--koku-mist-strong`, metin `--koku-ink-soft`  
**Tehlike:** `.btn-koku-danger` — `--koku-danger`

### 7.3 Avatar

| Sınıf | Boyut | Stil |
|-------|-------|------|
| `.avatar-sm` | 40px | Daire; zemin `linear-gradient(135deg, var(--koku-resin), var(--koku-gold))` |
| `.avatar-lg` | 80px | Aynı gradient; Fraunces baş harf |
| `.avatar-community` | 48px | Radius `--koku-radius-sm`; koyu reçine zemin, gold harf |

### 7.4 Parfüm görseli — `.perfume-media`

- Aspect ratio: 1:1 grid, 4:5 mobil kart
- Radius: üst `--koku-radius-md`, alt düz (kart içinde)
- Placeholder: marka baş harfi, `--koku-gold-soft` zemin, `--koku-resin` metin (placehold.co korunabilir)
- Hover: `scale(1.03)` 0.4s ease
- **Yeni:** merkezi `<partial name="_PerfumeImage" />` — `onerror` fallback tek yerde

### 7.5 Yıldız derecelendirme

- Dolu: `--koku-gold`
- Boş: `--koku-mist-strong`
- İnteraktif (Feed modal): hover preview + click select; mevcut inline JS partial'a taşınır

### 7.6 Chip aileleri

| Bileşen | Sınıf | Not |
|---------|-------|-----|
| Nota | `.note-chip` | `--note-color` CSS var; stil güncellenir |
| Akor | `.accord-chip` | Renkli zemin korunur; gölge hafifletilir |
| Filtre | `.filter-chip` | Pill; aktif `--koku-resin` zemin |
| Aktif filtre | `.filter-active-pill` | Kaldırılabilir; × butonu |
| Seçili parfüm | `.perfume-chip` | Autocomplete seçimi |

### 7.7 Form elemanları

- Input: `--koku-surface` zemin, `--koku-mist-strong` border, focus `--koku-resin` border + `--koku-resin-soft` glow
- Label: `--text-label` stili veya Figtree 500 `--text-small`
- Search (keşfet): pill input, sol `bi-search`, `--koku-paper` zemin

### 7.8 Boş durum — `.empty-state`

```
[ikon 2.5rem, --koku-ink-faint]
[Başlık Fraunces 600]
[Açıklama --koku-ink-muted]
[CTA .btn-koku]
```

Örnek kopya: "Henüz log yok" → "İlk kaydını ekle, takvimin dolmaya başlasın." + "Log ekle"

### 7.9 Bildirim & mesaj

- `.notification-item.unread`: sol 3px `--koku-sillage`, `--koku-resin-soft` zemin
- `.chat-bubble-mine`: `--koku-resin` zemin
- `.chat-bubble-theirs`: `--koku-paper-raised` zemin, `--koku-ink` metin

### 7.10 Takvim — `.calendar-*`

Mevcut grid yapısı korunur; görsel güncelleme:
- Hücre min-height: 80px desktop, 60px mobil
- Bugün: `--koku-gold-soft` zemin + `--koku-resin` border
- Loglu gün: akor rengi `%20` arka plan
- Gün numarası: Plex Mono 500
- Dot'lar: 6px, akor rengi

### 7.11 Öneri feedback — `.rec-feedback`

👍/👎 butonları; aktif `--koku-resin-soft` / `--koku-danger-soft`. Inline style bloğu `_RecommendationFeedbackScripts.cshtml`'den `site.css`'e taşınır.

---

## 8. Sayfa sayfa redesign brifi

Her sayfa için: **amaç → hero/odak → layout değişikliği → özel notlar**

### 8.1 Auth — Login, Register

**Amaç:** Güven veren, markayı tanıtan giriş.  
**Layout:** Split ekran (desktop): sol %45 soyut "nota piramidi" (CSS gradient + Fraunces alıntı), sağ %55 form kartı. Mobil: tek kolon, üstte marka.  
**Değişiklik:** Koyu tam ekran (`auth-container`) kaldırılır. Form kartı `--koku-surface`, geniş gölge.  
**Kopya:** "Parfüm dünyasına giriş yap" → "Koleksiyonuna kaldığın yerden devam et."

### 8.2 Home (misafir landing)

**Amaç:** Kayıt dönüşümü.  
**Hero:** "Bugün ne sürdün?" sorusu — ürünün kalbi. Altında 3 özellik kartı (koleksiyon, log, topluluk).  
**Layout:** Tam genişlik editorial; auth split ile görsel tutarlılık.

### 8.3 Ana Akış — Feed/Index

**Amaç:** Günlük ritüel + sosyal keşif.  
**Hero:** Hızlı paylaşım CTA tam genişlik (avatar + "Bugün hangi parfümü sürdün?").  
**Öneriler şeridi:** Yatay scroll-snap korunur; kartlar daha dar, uyum % monospace.  
**Aktivite:** Sol kenarda ince tip çizgisi (review / log / community); badge'ler `.text-label` stili.  
**Modal:** Sekmeli paylaşım; yıldız widget partial'a çıkarılır.

### 8.4 Keşfet — Perfumes/Index + _PerfumeGrid

**Amaç:** En karmaşık etkileşim — filtre + keşif.  
**Layout:** Filtre kartı üstte sticky (mobilde collapsible "Filtrele" butonu). Grid: `minmax(160px, 1fr)` desktop, 2 kolon mobil.  
**Filtre:** Mevcut chip/dropdown mantığı aynı; görsel token güncellemesi.  
**Kart:** Görsel + ad (Fraunces) + marka (muted) + akor mini-badge + puan (mono).  
**Not:** AJAX filtre motoru dokunulmaz; yalnızca HTML/CSS class migration.

### 8.5 Parfüm Detay — Perfumes/Details

**Amaç:** Parfümün kimliğini okumak ve aksiyon almak.  
**Layout:** Desktop 2 kolon — sol görsel (sticky), sağ nota/akor/yorum. Mobil: görsel üst, aksiyonlar görsel altında sabit bar (beğen + koleksiyon).  
**Değişiklik:** Bootstrap default badge'ler (`bg-warning`, `bg-info`) kaldırılır → `.koku-badge` varyantları.  
**Nota piramidi:** Üst/Orta/Alt dikey akış; her katman `.text-label` + chip satırı.  
**Benzer parfümler:** Yatay şerit veya 4'lü grid.

### 8.6 Yeni Parfüm — Perfumes/Create

**Amaç:** Hızlı veri girişi.  
**Layout:** Tek kolon form, bölümler (Temel bilgi / Notalar / Akorlar).  
**UX:** Akor alanları için autocomplete önerisi (Faz 2); Faz 1'de mevcut 5 text input korunur.

### 8.7 Koleksiyon — Collection/Index

**Amaç:** Koleksiyon yönetimi.  
**Subtab:** `.subtab-buttons` → `.koku-segmented` (pill grup, aktif beyaz yüzey).  
**Şişe slider:** Görsel şişe doluluk göstergesi (CSS, Faz 2); Faz 1'de mevcut range korunur.

### 8.8 Günlük Log — DailyLog/Index

**Amaç:** Platformun en ayırt edici sayfası — **takvim hero**.  
**Layout:** Takvim üstte tam genişlik; altında "Sahip olduğun parfümler" hızlı grid; en altta aylık liste.  
**Görsel:** Takvim hücreleri akor renkli; ay başlığı Fraunces; navigasyon okları minimal.  
**Modal:** Log ekle — autocomplete + tarih + sıkım + not.

### 8.9 Topluluklar — Index, Details, Create

**Index:** Kart grid; avatar + üye sayısı (mono) + katıl butonu.  
**Details:** 2 kolon desktop (gönderi + üyeler); mobilde üyeler collapse.  
**Create:** Sade form kartı.

### 8.10 Mesajlar — Index, Conversation

**Index:** Konuşma listesi; okunmamış `--koku-resin-soft` zemin.  
**Conversation:** Balonlar güncellenir; gönderme alanı sticky bottom. Enter gönder, Shift+Enter satır.

### 8.11 Bildirimler — Notifications/Index

Liste; tip ikonları korunur. Okunmamış sillage border. "Tümünü okundu işaretle" ghost buton.

### 8.12 Beğendiklerim — Likes/Index

Grid; kalp ikonu `--koku-danger` veya dolu resin.

### 8.13 Profil — View, Search, Settings

**View:** Sol kolon profil kartı (sillage border); sağ sekmeler.  
**Koku Profili sekmesi:** Akor progress bar'ları `--koku-resin` gradient; kaçınılan akorlar `--koku-danger`.  
**Search:** Arama + kullanıcı kartları.  
**Settings:** Minimal form.

### 8.14 Hata & yardımcı

- `Shared/Error.cshtml`: sade empty-state tarzı
- `Home/Privacy.cshtml`: prose layout

---

## 9. Hareket & mikro-etkileşim

| Olay | Animasyon | Süre |
|------|-----------|------|
| Kart hover | box-shadow artışı | 200ms ease |
| Buton press | scale(0.98) | 100ms |
| Sidebar mobil | translateX + backdrop fade | 250ms ease |
| Modal açılış | fade + translateY(8px) | 200ms |
| Öneri 👎 | kart fade-out + height collapse | 300ms |
| Sayfa yükleme | yok (Faz 1) | — |

**`prefers-reduced-motion: reduce`:** Tüm transform ve transition devre dışı.

---

## 10. Erişilebilirlik

- Tüm interaktif öğelerde görünür focus ring (`--koku-gold` 2px)
- Renk kontrastı: WCAG AA minimum (metin/arka plan 4.5:1)
- Takvim hücreleri: `aria-label` = "12 Mart, 2 log"
- Autocomplete: `role="listbox"`, klavye nav korunur
- Form hataları: `aria-invalid` + alan altı mesaj
- Dil: `html lang="tr"` (mevcut)

---

## 11. Teknik mimari

### 11.1 Stack kararı

| Katman | Karar |
|--------|-------|
| CSS Framework | **Bootstrap 5.3** korunur (grid, modal, form, utilities) |
| Özel CSS | `site.css` → `koku.css` + `koku-components.css` bölünmesi |
| İkonlar | Bootstrap Icons korunur |
| JS | Mevcut `site.js`, `perfume-autocomplete.js` + inline script'ler partial'lara taşınır |

**Neden React'e geçmiyoruz (şimdilik):** Backend Razor MVC; remake görsel katman. Davranış parity'si `ui-inventory.md` §5 ile korunur. İleride API-first SPA ayrı karar.

### 11.2 Dosya yapısı (hedef)

```
wwwroot/
├── css/
│   ├── koku-tokens.css      # :root, font-face
│   ├── koku-layout.css      # sidebar, shell, responsive
│   ├── koku-components.css  # kart, buton, chip, takvim...
│   └── koku.css             # @import hub (Layout'ta tek link)
├── js/
│   ├── site.js
│   ├── perfume-autocomplete.js
│   ├── star-rating.js       # Feed modal'dan çıkarılır
│   └── mobile-sidebar.js    # Layout inline'dan çıkarılır
Views/Shared/
├── _Layout.cshtml
├── _PerfumeImage.cshtml     # YENİ — merkezi görsel + fallback
├── _StarRating.cshtml       # YENİ
└── _EmptyState.cshtml       # YENİ
```

### 11.3 Sillage implementasyonu

1. `Filters/UnreadCountFilter.cs` yanına veya mevcut filter'a `ViewData["DominantAccordHex"]` ekle
2. `UserTasteProfileService`'den baskın akor rengi çek (hafif, cache'lenebilir)
3. `_Layout.cshtml` `<body style="--koku-sillage: @hex">` veya `<main>` üzerinde

### 11.4 Class migration tablosu

| Eski sınıf | Yeni sınıf |
|------------|------------|
| `.parfum-card` | `.koku-card` |
| `.btn-amber` | `.btn-koku` |
| `.font-serif` | `.font-display` |
| `.profile-avatar-sm` | `.avatar-sm` |
| `.profile-avatar-lg` | `.avatar-lg` |
| `.text-amber` | `.text-koku-accent` |
| `.bg-amber-soft` | `.bg-koku-soft` |
| `.sidebar-link` | `.nav-link-koku` |
| `.auth-container` | `.auth-split` |
| `.auth-card` | `.auth-panel` |

Geçiş döneminde eski sınıflar alias olarak kalabilir (`@deprecated` yorumu ile).

---

## 12. Uygulama planı (fazlar)

### Faz 0 — Hazırlık (0.5 gün)
- [ ] Bu dokümanı onayla
- [ ] `ui-inventory.md` ile class migration listesini doğrula
- [ ] Ekran görüntüsü baseline (mevcut UI archive)

### Faz 1 — Token & shell (1–2 gün)
- [x] `koku-tokens.css`, font CDN güncellemesi
- [x] `_Layout.cshtml` — yeni sidebar, mobil bar, sillage
- [x] Auth split layout (Login, Register, Home misafir)
- [x] Global buton, kart, form stilleri

### Faz 2 — Çekirdek sayfalar (2–3 gün)
- [ ] Feed/Index (+ paylaşım modal, star-rating partial)
- [ ] Perfumes/Index + _PerfumeGrid (filtre UI)
- [ ] Perfumes/Details
- [ ] DailyLog/Index (takvim hero)

### Faz 3 — Sosyal & profil (2 gün)
- [ ] Profile/View (+ koku profili sekmesi)
- [ ] Collection/Index
- [ ] Communities (3 view)
- [ ] Messages, Notifications

### Faz 4 — Kalan & temizlik (1 gün)
- [ ] Likes, Profile Search/Settings, Create, Error, Privacy
- [ ] `_PerfumeImage`, `_EmptyState` partial'ları
- [ ] Inline CSS/JS → merkezi dosyalara taşıma
- [ ] Eski `.parfum-*` alias'ları kaldır

### Faz 5 — QA & parite (1 gün)
- [ ] `ui-inventory.md` §5 davranış checklist
- [ ] Responsive: 360 / 768 / 992 / 1280
- [ ] Erişilebilirlik spot check
- [ ] `prefers-reduced-motion` testi

**Toplam tahmini:** 7–9 iş günü (tek geliştirici).

---

## 13. Başarı kriterleri

1. **Ayırt edilebilirlik:** Playfair + amber + koyu sidebar üçlüsü tamamen giderilmiş
2. **Parite:** `ui-inventory.md`'deki tüm UI elementleri ve JS davranışları çalışır
3. **Performans:** Ek font yükü <100KB; CSS toplam <50KB gzip
4. **Tutarlılık:** Hiçbir sayfada Bootstrap default badge/alert görünmez (özelleştirilmiş varyantlar)
5. **Mobil:** Günlük log takvimi ve keşfet filtreleri 375px genişlikte kullanılabilir

---

## 14. Kopya rehberi (ses tonu)

| Bağlam | Yap | Yapma |
|--------|-----|-------|
| Buton | "Log ekle", "Kaydet", "Paylaş" | "Gönder", "Submit" |
| Boş durum | Ne yapacağını söyle | "Henüz veri yok" |
| Hata | Ne oldu + nasıl düzelir | "Bir hata oluştu" |
| Öneri | "Koku profiline göre" | "AI powered" |
| Koleksiyon | "Sahibim", "İstek listem" | "Owned", "Wishlist" |

Dil: Türkçe, sentence case, kısa fiiller.

---

## 15. Wireframe referansları (ASCII)

### Feed (desktop)
```
┌─────────────────────────────────────────────────────────┐
│ Ana Akış                          [Paylaş] [Keşfet]   │
├─────────────────────────────────────────────────────────┤
│ ┌─ Sana özel ─────────────────────────────────────┐   │
│ │ [kart][kart][kart][kart] →→→ scroll              │   │
│ └──────────────────────────────────────────────────┘   │
│ ┌──────────────────────────────────────────────────┐   │
│ │ (A) Bugün hangi parfümü sürdün? Paylaş…          │   │
│ └──────────────────────────────────────────────────┘   │
│ ┌─ @ayse · Günlük log ─────────────────────────────┐   │
│ │ [thumb] Sauvage · 3 sıkım · "Ofis günü"          │   │
│ └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

### Keşfet (desktop)
```
┌─ Filtreler ─────────────────────────────────────────────┐
│ 🔍 Parfüm ara…                    [Popüler ▾]          │
│ Cinsiyet  [Erkek] [Kadın] [Unisex]                     │
│ Akorlar   [odunsu] [çiçeksi] [narenciye] … +daha fazla │
│ Marka     [Chanel × Dior ×  +2 ▾]                      │
│ Aktif: oud ×  woody ×                    Tümünü temizle│
└────────────────────────────────────────────────────────┘
┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐
│img │ │img │ │img │ │img │ │img │
│name│ │name│ │name│ │name│ │name│
└────┘ └────┘ └────┘ └────┘ └────┘
```

### DailyLog (desktop)
```
┌─ Mart 2026 ─── 12 log ─────────────────────────────────┐
│  Pzt   Sal   Çar   Per   Cum   Cmt   Paz                │
│ ┌──┐  ┌──┐  ┌██┐ ┌──┐  ┌██┐ ┌──┐  ┌──┐               │
│ │ 3│  │ 4│  │ 5│  │ 6│  │ 7│  │ 8│  │ 9│               │
│ └──┘  └──┘  └██┘ └──┘  └██┘ └──┘  └──┘  ← akor renkli │
└────────────────────────────────────────────────────────┘
┌─ Hızlı log ──────────────────────────────────────────────┐
│ [owned][owned][owned][owned]…                           │
└────────────────────────────────────────────────────────┘
```

---

## 16. Revizyon geçmişi

| Sürüm | Tarih | Değişiklik |
|-------|-------|------------|
| 1.0 | 2026-06-11 | İlk tasarım sistemi ve remake planı |

---

*Sonraki adım: Faz 1 uygulamasına başlamak için bu dokümanı onayla. Uygulama sırası §12'deki checklist takip edilir.*
