import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Droplet, Trash2, ChevronDown } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { collectionApi } from '../api/collection';

const TABS = [
  { key: 'owned', label: 'Koleksiyonum', emoji: '🧴' },
  { key: 'wishlist', label: 'İstek Listesi', emoji: '✨' },
  { key: 'tried', label: 'Denediklerim', emoji: '👃' },
];

function BottleBar({ level, perfumeId, onUpdate }) {
  return (
    <div className="mt-3">
      <div className="flex justify-between items-center mb-1">
        <span className="text-xs text-gray-500 font-medium">Şişe Doluluk</span>
        <span className="text-xs font-bold text-amber-600">%{level ?? 100}</span>
      </div>
      <div className="w-full bg-gray-100 rounded-full h-2">
        <div
          className="bg-amber-400 h-2 rounded-full transition-all duration-300"
          style={{ width: `${level ?? 100}%` }}
        />
      </div>
      <input
        type="range"
        min="0"
        max="100"
        value={level ?? 100}
        onChange={(e) => onUpdate(perfumeId, Number(e.target.value))}
        className="w-full mt-1 accent-amber-500 cursor-pointer"
      />
    </div>
  );
}

function CollectionCard({ item, onRemove, onBottleUpdate }) {
  const { perfume, status, bottleLevel } = item;
  const placeholder = `https://placehold.co/120x120/FDF2F8/E11D48?text=${encodeURIComponent(perfume.brand?.slice(0, 2) ?? '?')}`;

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-4 hover:shadow-md transition-shadow">
      <div className="flex gap-4">
        <Link to={`/perfume/${perfume.id}`}>
          <img
            src={perfume.imageUrl || placeholder}
            alt={perfume.name}
            onError={(e) => { e.target.src = placeholder; }}
            className="w-20 h-20 rounded-xl object-cover bg-gray-50 border border-gray-100 hover:opacity-80 transition"
          />
        </Link>
        <div className="flex-1 min-w-0">
          <div className="flex justify-between items-start">
            <div className="min-w-0">
              <Link to={`/perfume/${perfume.id}`}>
                <h3 className="font-bold text-gray-900 truncate hover:text-amber-700 transition">{perfume.name}</h3>
              </Link>
              <p className="text-xs text-gray-500 mt-0.5">{perfume.brand}</p>
              {perfume.ratingValue && (
                <p className="text-xs text-amber-600 font-semibold mt-1">★ {perfume.ratingValue.toFixed(1)}</p>
              )}
            </div>
            <button
              onClick={() => onRemove(perfume.id)}
              className="text-gray-300 hover:text-red-400 transition-colors ml-2 shrink-0"
            >
              <Trash2 size={16} />
            </button>
          </div>
          {status === 'owned' && (
            <BottleBar level={bottleLevel} perfumeId={perfume.id} onUpdate={onBottleUpdate} />
          )}
        </div>
      </div>
    </div>
  );
}

export default function CollectionPage() {
  const [activeTab, setActiveTab] = useState('owned');
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchItems = (status) => {
    setLoading(true);
    setError('');
    collectionApi.getAll(status)
      .then(setItems)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchItems(activeTab);
  }, [activeTab]);

  const handleRemove = async (perfumeId) => {
    await collectionApi.remove(perfumeId);
    setItems(prev => prev.filter(i => i.perfume.id !== perfumeId));
  };

  const handleBottleUpdate = async (perfumeId, bottleLevel) => {
    setItems(prev =>
      prev.map(i => i.perfume.id === perfumeId ? { ...i, bottleLevel } : i)
    );
    // Debounce yerine basit fire-and-forget
    collectionApi.update(perfumeId, { bottleLevel }).catch(() => {});
  };

  const currentTab = TABS.find(t => t.key === activeTab);

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[900px] w-full px-6 py-8">

          {/* Başlık */}
          <header className="mb-8">
            <h1 className="font-serif text-4xl font-bold text-gray-950">Koleksiyonum</h1>
            <p className="text-text-gray text-sm mt-1">Parfüm yolculuğunu takip et.</p>
          </header>

          {/* Tabs */}
          <div className="flex gap-2 mb-6">
            {TABS.map(tab => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={`px-5 py-2.5 rounded-xl font-semibold text-sm transition-colors ${
                  activeTab === tab.key
                    ? 'bg-amber-600 text-white shadow-sm'
                    : 'bg-white text-gray-600 border border-gray-100 hover:bg-gray-50'
                }`}
              >
                {tab.emoji} {tab.label}
              </button>
            ))}
          </div>

          {/* İçerik */}
          {loading && (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {[1, 2, 3, 4].map(i => (
                <div key={i} className="bg-white rounded-2xl border border-gray-100 h-32 animate-pulse" />
              ))}
            </div>
          )}

          {error && (
            <div className="bg-red-50 text-red-600 text-sm px-4 py-3 rounded-xl">{error}</div>
          )}

          {!loading && !error && items.length === 0 && (
            <div className="text-center py-20 text-gray-400">
              <Droplet size={48} className="mx-auto mb-4 opacity-30" />
              <p className="font-medium">{currentTab.label} boş.</p>
              <p className="text-sm mt-1">Parfüm keşfederek buraya ekleyebilirsin.</p>
              <Link
                to="/"
                className="inline-block mt-6 bg-amber-600 text-white px-6 py-2.5 rounded-xl font-semibold text-sm hover:bg-amber-700 transition"
              >
                Parfüm Keşfet
              </Link>
            </div>
          )}

          {!loading && !error && items.length > 0 && (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {items.map(item => (
                <CollectionCard
                  key={item.id}
                  item={item}
                  onRemove={handleRemove}
                  onBottleUpdate={handleBottleUpdate}
                />
              ))}
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
