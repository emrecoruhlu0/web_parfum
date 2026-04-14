import React, { useEffect, useState } from 'react';
import { Users, Plus, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import Sidebar from '../components/Sidebar';
import { communitiesApi } from '../api/communities';

function CreateModal({ onClose, onCreate }) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const data = await communitiesApi.create({ name, description });
      onCreate(data);
      onClose();
    } catch (err) {
      setError(err.message || 'Bir hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-3xl shadow-xl w-full max-w-md p-6">
        <div className="flex items-center justify-between mb-6">
          <h3 className="font-serif text-xl font-bold">Topluluk Oluştur</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700"><X size={20} /></button>
        </div>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="text-sm font-semibold text-gray-700 block mb-1">Topluluk Adı *</label>
            <input
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="örn. Oud Sevenler"
              className="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 text-sm outline-none focus:ring-1 focus:ring-amber-300"
              required
            />
          </div>
          <div>
            <label className="text-sm font-semibold text-gray-700 block mb-1">Açıklama</label>
            <textarea
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="Topluluk hakkında kısa bir açıklama..."
              className="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 text-sm outline-none focus:ring-1 focus:ring-amber-300 resize-none"
              rows="3"
            />
          </div>
          {error && <p className="text-red-500 text-sm">{error}</p>}
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose} className="flex-1 py-2.5 rounded-xl border border-gray-200 text-sm font-semibold text-gray-600 hover:bg-gray-50">İptal</button>
            <button type="submit" disabled={loading} className="flex-1 py-2.5 rounded-xl bg-amber-600 text-white text-sm font-semibold hover:bg-amber-700 disabled:opacity-60">
              {loading ? 'Oluşturuluyor...' : 'Oluştur'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function CommunityCard({ community, onToggleJoin }) {
  const [loading, setLoading] = useState(false);

  const handleJoin = async () => {
    setLoading(true);
    try {
      const data = await communitiesApi.toggleJoin(community.id);
      onToggleJoin(community.id, data.joined);
    } finally {
      setLoading(false);
    }
  };

  const placeholder = `https://placehold.co/80x80/FDF2F8/E11D48?text=${encodeURIComponent(community.name.slice(0, 2))}`;

  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 flex gap-4 hover:shadow-md transition-shadow">
      <Link to={`/communities/${community.id}`} className="shrink-0">
        <img
          src={community.imageUrl || placeholder}
          alt={community.name}
          onError={e => { e.target.src = placeholder; }}
          className="w-14 h-14 rounded-xl object-cover border border-gray-100"
        />
      </Link>
      <div className="flex-1 min-w-0">
        <Link to={`/communities/${community.id}`} className="font-bold text-gray-900 hover:text-amber-700 transition">
          {community.name}
        </Link>
        {community.description && <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{community.description}</p>}
        <p className="text-xs text-gray-400 mt-1 flex items-center gap-1">
          <Users size={12} /> {community._count.members} üye
        </p>
      </div>
      <button
        onClick={handleJoin}
        disabled={loading}
        className={`self-center px-4 py-2 rounded-xl text-sm font-semibold transition disabled:opacity-60 shrink-0 ${
          community.isMember
            ? 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            : 'bg-amber-600 text-white hover:bg-amber-700'
        }`}
      >
        {community.isMember ? 'Üyesin' : 'Katıl'}
      </button>
    </div>
  );
}

export default function CommunitiesPage() {
  const [communities, setCommunities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [tab, setTab] = useState('all'); // 'all' | 'mine'

  useEffect(() => {
    communitiesApi.getAll()
      .then(setCommunities)
      .finally(() => setLoading(false));
  }, []);

  const handleToggleJoin = (id, joined) => {
    setCommunities(prev => prev.map(c =>
      c.id === id
        ? { ...c, isMember: joined, _count: { ...c._count, members: joined ? c._count.members + 1 : c._count.members - 1 } }
        : c
    ));
  };

  const handleCreate = (newCommunity) => {
    setCommunities(prev => [newCommunity, ...prev]);
  };

  const displayed = tab === 'mine' ? communities.filter(c => c.isMember) : communities;

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[680px] w-full px-6 py-8 space-y-6">

          <div className="flex items-center justify-between pb-4 border-b border-gray-100">
            <h2 className="font-serif text-3xl font-bold flex items-center gap-2">
              <Users size={28} className="text-amber-500" /> Topluluklar
            </h2>
            <button
              onClick={() => setShowCreate(true)}
              className="flex items-center gap-2 bg-amber-600 text-white px-4 py-2 rounded-xl text-sm font-semibold hover:bg-amber-700 transition"
            >
              <Plus size={16} /> Yeni Topluluk
            </button>
          </div>

          {/* Tab */}
          <div className="flex gap-2">
            {[{ key: 'all', label: 'Tümü' }, { key: 'mine', label: 'Üye Olduklarım' }].map(t => (
              <button
                key={t.key}
                onClick={() => setTab(t.key)}
                className={`px-4 py-2 rounded-xl text-sm font-semibold transition-colors ${
                  tab === t.key ? 'bg-amber-600 text-white' : 'bg-white text-gray-600 border border-gray-100 hover:bg-gray-50'
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>

          {loading && (
            <div className="space-y-3">
              {[1, 2, 3].map(i => <div key={i} className="bg-white rounded-2xl h-24 animate-pulse border border-gray-100" />)}
            </div>
          )}

          {!loading && displayed.length === 0 && (
            <div className="text-center py-16 text-gray-400">
              <Users size={40} className="mx-auto mb-3 text-gray-200" />
              <p className="font-medium">{tab === 'mine' ? 'Henüz hiçbir topluluğa katılmadın.' : 'Henüz topluluk yok.'}</p>
            </div>
          )}

          {!loading && displayed.length > 0 && (
            <div className="space-y-3">
              {displayed.map(c => (
                <CommunityCard key={c.id} community={c} onToggleJoin={handleToggleJoin} />
              ))}
            </div>
          )}

        </main>
      </div>

      {showCreate && <CreateModal onClose={() => setShowCreate(false)} onCreate={handleCreate} />}
    </div>
  );
}
