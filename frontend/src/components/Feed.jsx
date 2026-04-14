import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Sparkles, Image as ImageIcon, MapPin, FlaskConical, Star, BookOpen } from 'lucide-react';
import ScentCard from './ScentCard';
import { perfumesApi } from '../api/perfumes';
import { socialApi } from '../api/social';
import { useAuth } from '../context/AuthContext';

function ActivityItem({ item }) {
  const { type, data } = item;
  const placeholder = `https://placehold.co/56x56/FDF2F8/E11D48?text=${encodeURIComponent(data.perfume?.brand?.slice(0, 2) ?? '?')}`;

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-4 flex gap-4 hover:shadow-md transition-shadow">
      <Link to={`/user/${data.user.username}`}>
        <img
          src={data.user.avatar || `https://i.pravatar.cc/150?u=${data.user.username}`}
          alt={data.user.username}
          className="w-10 h-10 rounded-full shrink-0"
        />
      </Link>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-1.5 mb-2 flex-wrap">
          <Link to={`/user/${data.user.username}`} className="font-bold text-sm hover:text-amber-700 transition">
            @{data.user.username}
          </Link>
          {type === 'review' ? (
            <span className="text-xs text-gray-400 flex items-center gap-1">
              <Star size={12} className="text-amber-400" /> yorum yaptı
            </span>
          ) : (
            <span className="text-xs text-gray-400 flex items-center gap-1">
              <BookOpen size={12} className="text-amber-400" /> koku taktı
            </span>
          )}
        </div>
        <Link to={`/perfume/${data.perfume.id}`} className="flex items-center gap-3 bg-gray-50 rounded-xl p-3 hover:bg-amber-50 transition-colors">
          <img
            src={data.perfume.imageUrl || placeholder}
            alt={data.perfume.name}
            onError={(e) => { e.target.src = placeholder; }}
            className="w-12 h-12 rounded-lg object-cover border border-gray-100 shrink-0"
          />
          <div className="min-w-0">
            <p className="font-bold text-sm text-gray-900 truncate">{data.perfume.name}</p>
            <p className="text-xs text-gray-400">{data.perfume.brand}</p>
            {type === 'review' && (
              <p className="text-xs text-amber-500 font-semibold mt-0.5">
                {'★'.repeat(data.rating)}{'☆'.repeat(5 - data.rating)}
              </p>
            )}
            {type === 'log' && data.sprays && (
              <p className="text-xs text-gray-400 mt-0.5">{data.sprays} puf</p>
            )}
          </div>
        </Link>
        {(type === 'review' && data.body) && (
          <p className="text-sm text-gray-600 mt-2 italic">"{data.body}"</p>
        )}
        {(type === 'log' && data.note) && (
          <p className="text-sm text-gray-600 mt-2 italic">"{data.note}"</p>
        )}
      </div>
    </div>
  );
}

export default function Feed() {
  const { user } = useAuth();
  const [perfumes, setPerfumes] = useState([]);
  const [likedIds, setLikedIds] = useState([]);
  const [activity, setActivity] = useState([]);
  const [tab, setTab] = useState('discover'); // 'discover' | 'social'
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (tab === 'discover') {
      setLoading(true);
      perfumesApi.getAll({ limit: 10 })
        .then(async (data) => {
          setPerfumes(data.perfumes);
          if (data.perfumes.length > 0) {
            const ids = data.perfumes.map(p => p.id);
            const liked = await socialApi.getLikedIds(ids).catch(() => []);
            setLikedIds(liked);
          }
        })
        .catch(err => setError(err.message))
        .finally(() => setLoading(false));
    } else {
      setLoading(true);
      socialApi.getFeed()
        .then(setActivity)
        .catch(err => setError(err.message))
        .finally(() => setLoading(false));
    }
  }, [tab]);

  return (
    <div className="space-y-8">
      <header className="flex items-center justify-between pb-4 border-b border-gray-100">
        <h2 className="font-serif text-3xl font-bold">Koku Günlüğü</h2>
        <button className="bg-text-dark text-white px-5 py-2.5 rounded-full font-semibold text-sm hover:bg-gray-800 transition shadow-sm flex items-center gap-2">
          <Sparkles size={16} /> Kokunu Paylaş
        </button>
      </header>

      {/* Post kutusu */}
      <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm flex gap-4">
        <img
          src={user?.avatar || `https://i.pravatar.cc/150?u=${user?.username}`}
          alt="User"
          className="w-12 h-12 rounded-full"
        />
        <div className="flex-1 space-y-4">
          <textarea
            placeholder="Bugün hangi kokuyu seçtin? Notaları sana ne hissettiriyor..."
            className="w-full bg-gray-50 p-4 rounded-xl border border-gray-100 text-sm focus:ring-1 focus:ring-amber-300 focus:border-amber-300 outline-none resize-none"
            rows="3"
          />
          <div className="flex justify-between items-center">
            <div className="flex gap-2 text-text-gray">
              <button className="p-2 hover:bg-gray-100 rounded-lg"><ImageIcon size={20} /></button>
              <button className="p-2 hover:bg-gray-100 rounded-lg"><FlaskConical size={20} /></button>
              <button className="p-2 hover:bg-gray-100 rounded-lg"><MapPin size={20} /></button>
            </div>
            <button className="bg-amber-600 text-white px-6 py-2 rounded-xl font-semibold text-sm hover:bg-amber-700 transition">Paylaş</button>
          </div>
        </div>
      </div>

      {/* Tab seçici */}
      <div className="flex gap-2">
        <button
          onClick={() => setTab('discover')}
          className={`px-5 py-2 rounded-xl font-semibold text-sm transition-colors ${
            tab === 'discover' ? 'bg-amber-600 text-white' : 'bg-white text-gray-600 border border-gray-100 hover:bg-gray-50'
          }`}
        >
          Keşfet
        </button>
        <button
          onClick={() => setTab('social')}
          className={`px-5 py-2 rounded-xl font-semibold text-sm transition-colors ${
            tab === 'social' ? 'bg-amber-600 text-white' : 'bg-white text-gray-600 border border-gray-100 hover:bg-gray-50'
          }`}
        >
          Takip Edilenler
        </button>
      </div>

      {/* Yükleniyor */}
      {loading && (
        <div className="space-y-4">
          {[1, 2, 3].map(i => (
            <div key={i} className="bg-white rounded-3xl border border-gray-100 h-48 animate-pulse" />
          ))}
        </div>
      )}

      {error && <div className="bg-red-50 text-red-600 text-sm px-4 py-3 rounded-xl">{error}</div>}

      {/* Keşfet: parfüm kartları */}
      {!loading && !error && tab === 'discover' && (
        <div className="space-y-6">
          {perfumes.map(perfume => (
            <ScentCard key={perfume.id} perfume={perfume} initialLiked={likedIds.includes(perfume.id)} />
          ))}
        </div>
      )}

      {/* Sosyal: aktivite akışı */}
      {!loading && !error && tab === 'social' && (
        activity.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <p className="font-medium">Henüz aktivite yok.</p>
            <p className="text-sm mt-1">Birini takip etmeye başla!</p>
          </div>
        ) : (
          <div className="space-y-4">
            {activity.map((item, i) => <ActivityItem key={i} item={item} />)}
          </div>
        )
      )}
    </div>
  );
}
