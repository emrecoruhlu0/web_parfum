import React, { useEffect, useState, useCallback, useRef } from 'react';
import { Link } from 'react-router-dom';
import {
  Search, SlidersHorizontal, X, ChevronDown, ChevronUp,
  Snowflake, Sun, Leaf, Wind, Star, TrendingUp, Globe, FlaskConical,
} from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { perfumesApi } from '../api/perfumes';

// ─── Sabitler ────────────────────────────────────────────────────────────────

const SEASON_META = [
  { key: 'ilkbahar', label: 'İlkbahar', icon: Leaf,      color: 'bg-green-50 text-green-700 border-green-200',   activeColor: 'bg-green-500 text-white border-green-500' },
  { key: 'yaz',      label: 'Yaz',      icon: Sun,       color: 'bg-yellow-50 text-yellow-700 border-yellow-200', activeColor: 'bg-yellow-500 text-white border-yellow-500' },
  { key: 'sonbahar', label: 'Sonbahar', icon: Wind,      color: 'bg-orange-50 text-orange-700 border-orange-200', activeColor: 'bg-orange-500 text-white border-orange-500' },
  { key: 'kis',      label: 'Kış',      icon: Snowflake, color: 'bg-blue-50 text-blue-700 border-blue-200',       activeColor: 'bg-blue-500 text-white border-blue-500' },
];

const TIER_OPTIONS = [
  { key: '',         label: 'Tümü' },
  { key: 'designer', label: '💼 Designer' },
  { key: 'niche',    label: '🧪 Niche' },
];

const GENDER_OPTIONS = [
  { key: '',       label: 'Tümü' },
  { key: 'women',  label: '👩 Kadın' },
  { key: 'men',    label: '👨 Erkek' },
  { key: 'unisex', label: '🧑 Unisex' },
];

const SORT_OPTIONS = [
  { key: 'popular', label: 'Popüler' },
  { key: 'rating',  label: 'En Yüksek Puan' },
  { key: 'newest',  label: 'En Yeni' },
  { key: 'oldest',  label: 'En Eski' },
];

const ACCORD_ICONS = {
  woody: '🪵', citrus: '🍋', aromatic: '🌿', sweet: '🍬', fruity: '🍑',
  powdery: '☁️', floral: '🌸', 'warm spicy': '🌶️', 'white floral': '🤍',
  'fresh spicy': '🫚', amber: '🟡', vanilla: '🍦', musky: '🌫️', green: '🌱',
  rose: '🌹', fresh: '💧', patchouli: '🪴', earthy: '🟤', leather: '👜', oud: '🪵',
};

const BRAND_LOGOS = {
  dior: 'Dior', chanel: 'Chanel', guerlain: 'Guerlain', givenchy: 'Givenchy',
  'yves-saint-laurent': 'YSL', 'giorgio-armani': 'Armani', 'calvin-klein': 'CK',
  'carolina-herrera': 'CH', xerjoff: 'Xerjoff', 'lattafa-perfumes': 'Lattafa',
  avon: 'Avon', zara: 'Zara', oriflame: 'Oriflame', gucci: 'Gucci', prada: 'Prada',
  burberry: 'Burberry',
};

// ─── Alt Bileşenler ──────────────────────────────────────────────────────────

function FilterChip({ label, active, onClick, color, activeColor }) {
  return (
    <button
      onClick={onClick}
      className={`px-3 py-1.5 rounded-full text-xs font-semibold border transition-all ${active ? activeColor : color}`}
    >
      {label}
    </button>
  );
}

function SectionHeader({ icon: Icon, title, color = 'text-amber-600' }) {
  return (
    <div className="flex items-center gap-2 mb-3">
      <Icon size={18} className={color} />
      <h3 className="font-bold text-gray-800 text-sm uppercase tracking-wide">{title}</h3>
    </div>
  );
}

function PerfumeResultCard({ perfume }) {
  const placeholder = `https://placehold.co/80x80/FDF2F8/E11D48?text=${encodeURIComponent(perfume.brand?.slice(0, 2) ?? '?')}`;
  return (
    <Link
      to={`/perfume/${perfume.id}`}
      className="bg-white rounded-2xl border border-gray-100 shadow-sm p-4 flex gap-3 hover:shadow-md transition-shadow"
    >
      <img
        src={perfume.imageUrl || placeholder}
        alt={perfume.name}
        onError={e => { e.target.src = placeholder; }}
        className="w-14 h-14 rounded-xl object-cover border border-gray-100 shrink-0"
      />
      <div className="min-w-0 flex-1">
        <p className="font-bold text-sm text-gray-900 truncate">{perfume.name}</p>
        <p className="text-xs text-gray-400 mt-0.5 capitalize">{perfume.brand}</p>
        <div className="flex items-center gap-2 mt-1 flex-wrap">
          {perfume.gender && (
            <span className="text-[10px] bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">{perfume.gender}</span>
          )}
          {perfume.year && (
            <span className="text-[10px] bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">{perfume.year}</span>
          )}
          {perfume.accord1 && (
            <span className="text-[10px] bg-amber-50 text-amber-600 px-1.5 py-0.5 rounded font-medium">{perfume.accord1}</span>
          )}
        </div>
      </div>
      {perfume.ratingValue && (
        <div className="shrink-0 text-right">
          <p className="text-xs font-bold text-amber-600">★ {perfume.ratingValue.toFixed(1)}</p>
          {perfume.ratingCount && <p className="text-[10px] text-gray-400">{perfume.ratingCount.toLocaleString()}</p>}
        </div>
      )}
    </Link>
  );
}

// ─── Ana Sayfa ────────────────────────────────────────────────────────────────

export default function DiscoverPage() {
  const [meta, setMeta] = useState(null);
  const [query, setQuery] = useState('');
  const [filters, setFilters] = useState({ season: '', accord: '', tier: '', gender: '', country: '', sort: 'popular' });
  const [results, setResults] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const searchTimeout = useRef(null);

  useEffect(() => {
    perfumesApi.getMeta().then(setMeta);
  }, []);

  const doSearch = useCallback(async (q, f, p = 1) => {
    setLoading(true);
    setSearched(true);
    try {
      const data = await perfumesApi.getAll({
        search: q || undefined,
        season: f.season || undefined,
        accord: f.accord || undefined,
        tier: f.tier || undefined,
        gender: f.gender || undefined,
        country: f.country || undefined,
        sort: f.sort,
        page: p,
        limit: 20,
      });
      if (p === 1) setResults(data.perfumes);
      else setResults(prev => [...prev, ...data.perfumes]);
      setTotal(data.total);
      setPage(p);
    } finally {
      setLoading(false);
    }
  }, []);

  // Filtre değişince otomatik ara
  useEffect(() => {
    const hasFilter = filters.season || filters.accord || filters.tier || filters.gender || filters.country;
    if (hasFilter || query) {
      clearTimeout(searchTimeout.current);
      searchTimeout.current = setTimeout(() => doSearch(query, filters, 1), 300);
    }
    return () => clearTimeout(searchTimeout.current);
  }, [filters, query, doSearch]);

  const setFilter = (key, val) => setFilters(prev => ({ ...prev, [key]: prev[key] === val ? '' : val }));

  const activeFilterCount = Object.entries(filters).filter(([k, v]) => k !== 'sort' && v).length + (query ? 1 : 0);

  const showGrid = !searched;

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[860px] w-full px-6 py-8 space-y-6">

          {/* Başlık */}
          <header className="pb-4 border-b border-gray-100">
            <h2 className="font-serif text-3xl font-bold flex items-center gap-2">
              <Search size={26} className="text-amber-500" /> Koku Keşfet
            </h2>
            <p className="text-sm text-gray-500 mt-1">24.000+ parfüm arasından keşfet</p>
          </header>

          {/* Arama Çubuğu */}
          <div className="relative">
            <Search size={18} className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400" />
            <input
              value={query}
              onChange={e => setQuery(e.target.value)}
              placeholder="İsim, marka, nota ara... (örn. oud, bergamot, dior)"
              className="w-full bg-white border border-gray-200 rounded-2xl pl-11 pr-12 py-3.5 text-sm outline-none focus:ring-2 focus:ring-amber-300 shadow-sm"
            />
            {query && (
              <button onClick={() => setQuery('')} className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600">
                <X size={16} />
              </button>
            )}
          </div>

          {/* Mevsim Butonları */}
          <div className="flex gap-2 flex-wrap">
            {SEASON_META.map(s => {
              const Icon = s.icon;
              const active = filters.season === s.key;
              return (
                <button
                  key={s.key}
                  onClick={() => setFilter('season', s.key)}
                  className={`flex items-center gap-1.5 px-4 py-2 rounded-xl border text-sm font-semibold transition-all ${active ? s.activeColor : s.color}`}
                >
                  <Icon size={15} /> {s.label}
                </button>
              );
            })}
            <div className="ml-auto flex items-center gap-2">
              <button
                onClick={() => setShowFilters(v => !v)}
                className={`flex items-center gap-1.5 px-4 py-2 rounded-xl border text-sm font-semibold transition-all ${
                  activeFilterCount > 0 ? 'bg-amber-600 text-white border-amber-600' : 'bg-white text-gray-600 border-gray-200 hover:bg-gray-50'
                }`}
              >
                <SlidersHorizontal size={15} /> Filtreler
                {activeFilterCount > 0 && <span className="bg-white text-amber-600 rounded-full text-xs w-5 h-5 flex items-center justify-center font-bold">{activeFilterCount}</span>}
                {showFilters ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
              </button>
            </div>
          </div>

          {/* Genişletilmiş Filtreler */}
          {showFilters && (
            <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 space-y-5">
              {/* Cinsiyet */}
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-2">Cinsiyet</p>
                <div className="flex gap-2 flex-wrap">
                  {GENDER_OPTIONS.map(g => (
                    <FilterChip
                      key={g.key}
                      label={g.label}
                      active={filters.gender === g.key && g.key !== ''}
                      onClick={() => setFilter('gender', g.key)}
                      color="bg-gray-50 text-gray-600 border-gray-200"
                      activeColor="bg-gray-800 text-white border-gray-800"
                    />
                  ))}
                </div>
              </div>

              {/* Tier */}
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-2">Kategori</p>
                <div className="flex gap-2 flex-wrap">
                  {TIER_OPTIONS.map(t => (
                    <FilterChip
                      key={t.key}
                      label={t.label}
                      active={filters.tier === t.key && t.key !== ''}
                      onClick={() => setFilter('tier', t.key)}
                      color="bg-gray-50 text-gray-600 border-gray-200"
                      activeColor="bg-purple-600 text-white border-purple-600"
                    />
                  ))}
                </div>
              </div>

              {/* Sıralama */}
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-2">Sıralama</p>
                <div className="flex gap-2 flex-wrap">
                  {SORT_OPTIONS.map(s => (
                    <FilterChip
                      key={s.key}
                      label={s.label}
                      active={filters.sort === s.key}
                      onClick={() => setFilters(prev => ({ ...prev, sort: s.key }))}
                      color="bg-gray-50 text-gray-600 border-gray-200"
                      activeColor="bg-amber-600 text-white border-amber-600"
                    />
                  ))}
                </div>
              </div>

              {/* Accord */}
              {meta?.topAccords && (
                <div>
                  <p className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-2">Koku Profili (Accord)</p>
                  <div className="flex gap-2 flex-wrap">
                    {meta.topAccords.slice(0, 16).map(({ accord }) => (
                      <FilterChip
                        key={accord}
                        label={`${ACCORD_ICONS[accord] || '🌸'} ${accord}`}
                        active={filters.accord === accord}
                        onClick={() => setFilter('accord', accord)}
                        color="bg-amber-50 text-amber-700 border-amber-200"
                        activeColor="bg-amber-600 text-white border-amber-600"
                      />
                    ))}
                  </div>
                </div>
              )}

              {/* Ülke */}
              {meta?.countries && (
                <div>
                  <p className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-2">Menşei Ülke</p>
                  <div className="flex gap-2 flex-wrap">
                    {meta.countries.map(({ country }) => (
                      <FilterChip
                        key={country}
                        label={`🌍 ${country}`}
                        active={filters.country === country}
                        onClick={() => setFilter('country', country)}
                        color="bg-blue-50 text-blue-700 border-blue-200"
                        activeColor="bg-blue-600 text-white border-blue-600"
                      />
                    ))}
                  </div>
                </div>
              )}

              {activeFilterCount > 0 && (
                <button
                  onClick={() => { setFilters({ season: '', accord: '', tier: '', gender: '', country: '', sort: 'popular' }); setQuery(''); setSearched(false); }}
                  className="text-sm text-red-500 hover:text-red-700 font-medium flex items-center gap-1"
                >
                  <X size={14} /> Filtreleri Temizle
                </button>
              )}
            </div>
          )}

          {/* ── Keşfet Grid (arama yokken) ── */}
          {showGrid && meta && (
            <div className="space-y-8">

              {/* Mevsime Göre */}
              <section>
                <SectionHeader icon={Sun} title="Mevsime Göre Keşfet" />
                <div className="grid grid-cols-4 gap-3">
                  {SEASON_META.map(s => {
                    const Icon = s.icon;
                    return (
                      <button
                        key={s.key}
                        onClick={() => setFilter('season', s.key)}
                        className={`flex flex-col items-center gap-2 py-5 rounded-2xl border font-semibold text-sm transition-all hover:shadow-md ${s.color}`}
                      >
                        <Icon size={28} />
                        {s.label}
                      </button>
                    );
                  })}
                </div>
              </section>

              {/* Accord / Koku Profili */}
              <section>
                <SectionHeader icon={FlaskConical} title="Koku Profiline Göre" color="text-rose-500" />
                <div className="grid grid-cols-4 gap-2">
                  {meta.topAccords.slice(0, 12).map(({ accord, count }) => (
                    <button
                      key={accord}
                      onClick={() => setFilter('accord', accord)}
                      className="bg-white border border-gray-100 rounded-xl p-3 text-left hover:border-amber-200 hover:bg-amber-50 transition-colors shadow-sm"
                    >
                      <span className="text-2xl">{ACCORD_ICONS[accord] || '🌸'}</span>
                      <p className="text-xs font-bold text-gray-800 mt-1 capitalize">{accord}</p>
                      <p className="text-[10px] text-gray-400">{count.toLocaleString()} parfüm</p>
                    </button>
                  ))}
                </div>
              </section>

              {/* Designer vs Niche */}
              <section>
                <SectionHeader icon={Star} title="Designer & Niche" color="text-purple-500" />
                <div className="grid grid-cols-2 gap-3">
                  <button
                    onClick={() => setFilter('tier', 'designer')}
                    className="bg-gradient-to-br from-slate-50 to-slate-100 border border-slate-200 rounded-2xl p-5 text-left hover:shadow-md transition-shadow"
                  >
                    <p className="text-2xl mb-2">💼</p>
                    <p className="font-bold text-gray-900">Designer</p>
                    <p className="text-xs text-gray-500 mt-1">Dior, Chanel, Armani ve daha fazlası</p>
                  </button>
                  <button
                    onClick={() => setFilter('tier', 'niche')}
                    className="bg-gradient-to-br from-purple-50 to-purple-100 border border-purple-200 rounded-2xl p-5 text-left hover:shadow-md transition-shadow"
                  >
                    <p className="text-2xl mb-2">🧪</p>
                    <p className="font-bold text-gray-900">Niche</p>
                    <p className="text-xs text-gray-500 mt-1">Bağımsız ve sanatsal markalar</p>
                  </button>
                </div>
              </section>

              {/* Cinsiyet */}
              <section>
                <SectionHeader icon={TrendingUp} title="Cinsiyete Göre" color="text-pink-500" />
                <div className="grid grid-cols-3 gap-3">
                  {[
                    { key: 'women', label: 'Kadın', emoji: '💐', color: 'from-pink-50 to-rose-50 border-pink-200' },
                    { key: 'men',   label: 'Erkek', emoji: '🌊', color: 'from-blue-50 to-cyan-50 border-blue-200' },
                    { key: 'unisex',label: 'Unisex',emoji: '✨', color: 'from-amber-50 to-yellow-50 border-amber-200' },
                  ].map(g => (
                    <button
                      key={g.key}
                      onClick={() => setFilter('gender', g.key)}
                      className={`bg-gradient-to-br ${g.color} border rounded-2xl p-4 text-left hover:shadow-md transition-shadow`}
                    >
                      <p className="text-2xl mb-2">{g.emoji}</p>
                      <p className="font-bold text-gray-900 text-sm">{g.label}</p>
                    </button>
                  ))}
                </div>
              </section>

              {/* En Popüler Markalar */}
              <section>
                <SectionHeader icon={Globe} title="Popüler Markalar" color="text-blue-500" />
                <div className="grid grid-cols-4 gap-2">
                  {meta.topBrands.map(({ brand, count }) => {
                    const displayName = BRAND_LOGOS[brand] || brand.split('-').map(w => w[0].toUpperCase() + w.slice(1)).join(' ');
                    const placeholder = `https://placehold.co/48x48/FDF2F8/E11D48?text=${encodeURIComponent(displayName.slice(0, 2))}`;
                    return (
                      <button
                        key={brand}
                        onClick={() => { setQuery(brand); }}
                        className="bg-white border border-gray-100 rounded-xl p-3 flex flex-col items-center gap-2 hover:border-amber-200 hover:bg-amber-50 transition-colors shadow-sm"
                      >
                        <img src={placeholder} alt={displayName} className="w-10 h-10 rounded-lg" />
                        <p className="text-xs font-bold text-gray-800 text-center leading-tight">{displayName}</p>
                        <p className="text-[10px] text-gray-400">{count}</p>
                      </button>
                    );
                  })}
                </div>
              </section>

              {/* Ülkelere Göre */}
              <section>
                <SectionHeader icon={Globe} title="Menşei Ülkeye Göre" color="text-green-500" />
                <div className="flex flex-wrap gap-2">
                  {meta.countries.map(({ country, count }) => (
                    <button
                      key={country}
                      onClick={() => setFilter('country', country)}
                      className="bg-white border border-gray-100 rounded-xl px-4 py-2.5 flex items-center gap-2 hover:border-green-200 hover:bg-green-50 transition-colors shadow-sm"
                    >
                      <span className="text-sm font-bold text-gray-800">🌍 {country}</span>
                      <span className="text-xs text-gray-400">{count.toLocaleString()}</span>
                    </button>
                  ))}
                </div>
              </section>

            </div>
          )}

          {/* ── Arama Sonuçları ── */}
          {searched && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <p className="text-sm text-gray-500">
                  {loading ? 'Aranıyor...' : `${total.toLocaleString()} parfüm bulundu`}
                </p>
                <select
                  value={filters.sort}
                  onChange={e => setFilters(prev => ({ ...prev, sort: e.target.value }))}
                  className="text-xs bg-white border border-gray-200 rounded-lg px-3 py-1.5 outline-none"
                >
                  {SORT_OPTIONS.map(s => <option key={s.key} value={s.key}>{s.label}</option>)}
                </select>
              </div>

              {loading && page === 1 && (
                <div className="space-y-3">
                  {[1,2,3,4].map(i => <div key={i} className="bg-white rounded-2xl h-20 animate-pulse border border-gray-100" />)}
                </div>
              )}

              {!loading && results.length === 0 && (
                <div className="text-center py-16 text-gray-400">
                  <Search size={40} className="mx-auto mb-3 text-gray-200" />
                  <p className="font-medium">Sonuç bulunamadı.</p>
                  <p className="text-sm mt-1">Farklı filtreler veya arama terimleri dene.</p>
                </div>
              )}

              <div className="space-y-3">
                {results.map(p => <PerfumeResultCard key={p.id} perfume={p} />)}
              </div>

              {results.length < total && !loading && (
                <button
                  onClick={() => doSearch(query, filters, page + 1)}
                  className="w-full py-3 bg-white border border-gray-200 rounded-2xl text-sm font-semibold text-gray-600 hover:bg-gray-50 transition"
                >
                  Daha Fazla Yükle ({results.length}/{total})
                </button>
              )}

              {loading && page > 1 && (
                <div className="text-center py-4"><div className="w-6 h-6 border-2 border-amber-400 border-t-transparent rounded-full animate-spin mx-auto" /></div>
              )}
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
