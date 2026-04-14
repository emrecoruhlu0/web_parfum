import React, { useEffect, useState, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Users, Send, Trash2, FlaskConical, X } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { communitiesApi } from '../api/communities';
import { perfumesApi } from '../api/perfumes';
import { useAuth } from '../context/AuthContext';

// ─── Parfüm mini kartı (akışta) ─────────────────────────────────────────────
function PerfumePreview({ perfume }) {
  const placeholder = `https://placehold.co/56x56/FDF2F8/E11D48?text=${encodeURIComponent(perfume.brand?.slice(0, 2) ?? '?')}`;
  return (
    <Link
      to={`/perfume/${perfume.id}`}
      className="flex items-center gap-3 bg-gray-50 border border-gray-100 rounded-xl p-3 mt-2 hover:bg-amber-50 hover:border-amber-100 transition-colors"
    >
      <img
        src={perfume.imageUrl || placeholder}
        alt={perfume.name}
        onError={e => { e.target.src = placeholder; }}
        className="w-12 h-12 rounded-lg object-cover border border-gray-100 shrink-0"
      />
      <div className="min-w-0">
        <p className="font-bold text-sm text-gray-900 truncate">{perfume.name}</p>
        <p className="text-xs text-gray-400 capitalize">{perfume.brand}</p>
        <div className="flex items-center gap-2 mt-0.5 flex-wrap">
          {perfume.accord1 && (
            <span className="text-[10px] bg-amber-50 text-amber-600 px-1.5 py-0.5 rounded font-medium">{perfume.accord1}</span>
          )}
          {perfume.ratingValue && (
            <span className="text-[10px] text-amber-500 font-semibold">★ {perfume.ratingValue.toFixed(1)}</span>
          )}
        </div>
      </div>
    </Link>
  );
}

// ─── Post kartı ──────────────────────────────────────────────────────────────
function PostItem({ post, canDelete, onDelete }) {
  return (
    <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-4 flex gap-3">
      <Link to={`/user/${post.user.username}`} className="shrink-0">
        <img
          src={post.user.avatar || `https://i.pravatar.cc/150?u=${post.user.username}`}
          alt={post.user.username}
          className="w-9 h-9 rounded-full"
        />
      </Link>
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-1.5">
            <Link to={`/user/${post.user.username}`} className="font-bold text-sm hover:text-amber-700 transition">
              @{post.user.username}
            </Link>
            <span className="text-xs text-gray-400">
              {new Date(post.createdAt).toLocaleDateString('tr-TR')}
            </span>
          </div>
          {canDelete && (
            <button onClick={() => onDelete(post.id)} className="text-gray-300 hover:text-red-400 transition shrink-0">
              <Trash2 size={14} />
            </button>
          )}
        </div>
        <p className="text-sm text-gray-700 mt-1 whitespace-pre-wrap">{post.body}</p>
        {post.perfume && <PerfumePreview perfume={post.perfume} />}
      </div>
    </div>
  );
}

// ─── Parfüm arama dropdown ───────────────────────────────────────────────────
function PerfumeSearchDropdown({ onSelect }) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const timeoutRef = useRef(null);

  const handleChange = (e) => {
    const val = e.target.value;
    setQuery(val);
    clearTimeout(timeoutRef.current);
    if (val.length < 2) { setResults([]); return; }
    setLoading(true);
    timeoutRef.current = setTimeout(async () => {
      const data = await perfumesApi.getAll({ search: val, limit: 6 }).catch(() => ({ perfumes: [] }));
      setResults(data.perfumes);
      setLoading(false);
    }, 300);
  };

  const placeholder_img = (brand) => `https://placehold.co/40x40/FDF2F8/E11D48?text=${encodeURIComponent((brand ?? '?').slice(0, 2))}`;

  return (
    <div className="relative">
      <input
        value={query}
        onChange={handleChange}
        placeholder="Parfüm ara... (en az 2 karakter)"
        className="w-full bg-gray-50 border border-gray-200 rounded-xl px-3 py-2 text-sm outline-none focus:ring-1 focus:ring-amber-300"
        autoFocus
      />
      {loading && <p className="text-xs text-gray-400 mt-1 px-1">Aranıyor...</p>}
      {results.length > 0 && (
        <div className="absolute z-20 w-full bg-white border border-gray-200 rounded-xl shadow-lg mt-1 max-h-48 overflow-y-auto">
          {results.map(p => (
            <button
              key={p.id}
              type="button"
              onClick={() => { onSelect(p); setQuery(''); setResults([]); }}
              className="w-full flex items-center gap-3 px-3 py-2.5 hover:bg-amber-50 text-left transition-colors"
            >
              <img
                src={p.imageUrl || placeholder_img(p.brand)}
                alt={p.name}
                onError={e => { e.target.src = placeholder_img(p.brand); }}
                className="w-9 h-9 rounded-lg object-cover border border-gray-100 shrink-0"
              />
              <div className="min-w-0">
                <p className="font-semibold text-sm text-gray-900 truncate">{p.name}</p>
                <p className="text-xs text-gray-400 capitalize">{p.brand}</p>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

// ─── Ana Sayfa ────────────────────────────────────────────────────────────────
export default function CommunityDetailPage() {
  const { id } = useParams();
  const { user } = useAuth();
  const [community, setCommunity] = useState(null);
  const [posts, setPosts] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [postsLoading, setPostsLoading] = useState(false);
  const [joinLoading, setJoinLoading] = useState(false);
  const [newPost, setNewPost] = useState('');
  const [selectedPerfume, setSelectedPerfume] = useState(null);
  const [showPerfumeSearch, setShowPerfumeSearch] = useState(false);
  const [posting, setPosting] = useState(false);

  useEffect(() => {
    communitiesApi.getOne(parseInt(id))
      .then(setCommunity)
      .finally(() => setLoading(false));
  }, [id]);

  useEffect(() => {
    setPostsLoading(true);
    communitiesApi.getPosts(parseInt(id), 1)
      .then(data => { setPosts(data.posts); setTotal(data.total); setPage(1); })
      .finally(() => setPostsLoading(false));
  }, [id]);

  const loadMore = async () => {
    setPostsLoading(true);
    const data = await communitiesApi.getPosts(parseInt(id), page + 1);
    setPosts(prev => [...prev, ...data.posts]);
    setPage(p => p + 1);
    setPostsLoading(false);
  };

  const handleJoin = async () => {
    setJoinLoading(true);
    try {
      const data = await communitiesApi.toggleJoin(community.id);
      setCommunity(prev => ({
        ...prev,
        isMember: data.joined,
        myRole: data.joined ? 'member' : null,
        _count: { ...prev._count, members: data.joined ? prev._count.members + 1 : prev._count.members - 1 },
      }));
    } finally {
      setJoinLoading(false);
    }
  };

  const handlePost = async (e) => {
    e.preventDefault();
    if (!newPost.trim()) return;
    setPosting(true);
    try {
      const post = await communitiesApi.createPost(community.id, newPost.trim(), selectedPerfume?.id ?? null);
      setPosts(prev => [post, ...prev]);
      setNewPost('');
      setSelectedPerfume(null);
      setShowPerfumeSearch(false);
    } finally {
      setPosting(false);
    }
  };

  const handleDelete = async (postId) => {
    await communitiesApi.deletePost(community.id, postId);
    setPosts(prev => prev.filter(p => p.id !== postId));
  };

  if (loading) return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex items-center justify-center">
        <div className="w-10 h-10 border-4 border-amber-400 border-t-transparent rounded-full animate-spin" />
      </div>
    </div>
  );

  if (!community) return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex items-center justify-center text-gray-400">Topluluk bulunamadı.</div>
    </div>
  );

  const placeholder = `https://placehold.co/80x80/FDF2F8/E11D48?text=${encodeURIComponent(community.name.slice(0, 2))}`;

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[720px] w-full px-6 py-8 space-y-6">

          <Link to="/communities" className="inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-800 transition">
            <ArrowLeft size={16} /> Topluluklara Dön
          </Link>

          {/* Topluluk Başlık Kartı */}
          <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-6 flex gap-5 items-center">
            <img
              src={community.imageUrl || placeholder}
              alt={community.name}
              onError={e => { e.target.src = placeholder; }}
              className="w-16 h-16 rounded-2xl object-cover border border-gray-100 shrink-0"
            />
            <div className="flex-1 min-w-0">
              <h1 className="font-serif text-2xl font-bold text-gray-950">{community.name}</h1>
              {community.description && <p className="text-sm text-gray-500 mt-1">{community.description}</p>}
              <div className="flex items-center gap-4 mt-2 text-xs text-gray-400">
                <span className="flex items-center gap-1"><Users size={12} /> {community._count.members} üye</span>
                <span>Kurucu: <Link to={`/user/${community.owner.username}`} className="text-amber-600 hover:underline">@{community.owner.username}</Link></span>
              </div>
            </div>
            {community.owner.id !== user?.id && (
              <button
                onClick={handleJoin}
                disabled={joinLoading}
                className={`shrink-0 px-5 py-2.5 rounded-xl font-semibold text-sm transition disabled:opacity-60 ${
                  community.isMember
                    ? 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    : 'bg-amber-600 text-white hover:bg-amber-700'
                }`}
              >
                {community.isMember ? 'Üyesin' : 'Katıl'}
              </button>
            )}
          </div>

          {/* Post Kutusu */}
          {community.isMember && (
            <form onSubmit={handlePost} className="bg-white rounded-2xl border border-gray-100 shadow-sm p-4 space-y-3">
              <div className="flex gap-3">
                <img
                  src={user?.avatar || `https://i.pravatar.cc/150?u=${user?.username}`}
                  alt={user?.username}
                  className="w-9 h-9 rounded-full shrink-0"
                />
                <textarea
                  value={newPost}
                  onChange={e => setNewPost(e.target.value)}
                  placeholder="Topluluğa bir şeyler yaz..."
                  rows="2"
                  className="flex-1 bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 text-sm outline-none focus:ring-1 focus:ring-amber-300 resize-none"
                  onKeyDown={e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handlePost(e); } }}
                />
              </div>

              {/* Seçili parfüm önizleme */}
              {selectedPerfume && (
                <div className="ml-12 flex items-center gap-3 bg-amber-50 border border-amber-100 rounded-xl p-3">
                  <img
                    src={selectedPerfume.imageUrl || `https://placehold.co/40x40/FDF2F8/E11D48?text=${encodeURIComponent((selectedPerfume.brand ?? '?').slice(0,2))}`}
                    alt={selectedPerfume.name}
                    className="w-10 h-10 rounded-lg object-cover border border-amber-100 shrink-0"
                  />
                  <div className="flex-1 min-w-0">
                    <p className="font-bold text-sm text-gray-900 truncate">{selectedPerfume.name}</p>
                    <p className="text-xs text-gray-400 capitalize">{selectedPerfume.brand}</p>
                  </div>
                  <button type="button" onClick={() => setSelectedPerfume(null)} className="text-gray-400 hover:text-red-400">
                    <X size={16} />
                  </button>
                </div>
              )}

              {/* Parfüm arama alanı */}
              {showPerfumeSearch && !selectedPerfume && (
                <div className="ml-12">
                  <PerfumeSearchDropdown onSelect={(p) => { setSelectedPerfume(p); setShowPerfumeSearch(false); }} />
                </div>
              )}

              <div className="flex items-center justify-between ml-12">
                <button
                  type="button"
                  onClick={() => setShowPerfumeSearch(v => !v)}
                  className={`flex items-center gap-1.5 text-xs font-medium px-3 py-1.5 rounded-lg transition-colors ${
                    showPerfumeSearch || selectedPerfume
                      ? 'bg-amber-100 text-amber-700'
                      : 'text-gray-500 hover:bg-gray-100'
                  }`}
                >
                  <FlaskConical size={14} /> Parfüm Ekle
                </button>
                <button
                  type="submit"
                  disabled={posting || !newPost.trim()}
                  className="flex items-center gap-2 bg-amber-600 text-white px-4 py-2 rounded-xl text-sm font-semibold hover:bg-amber-700 transition disabled:opacity-50"
                >
                  <Send size={14} /> Paylaş
                </button>
              </div>
            </form>
          )}

          {/* Akış */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="font-bold text-gray-700 text-sm uppercase tracking-wide">Topluluk Akışı</h3>
              <span className="text-xs text-gray-400">{total} gönderi</span>
            </div>

            {postsLoading && posts.length === 0 && (
              <div className="space-y-3">
                {[1,2,3].map(i => <div key={i} className="bg-white rounded-2xl h-16 animate-pulse border border-gray-100" />)}
              </div>
            )}

            {!postsLoading && posts.length === 0 && (
              <div className="text-center py-14 text-gray-400">
                <p className="font-medium">Henüz gönderi yok.</p>
                {community.isMember && <p className="text-sm mt-1">İlk gönderiyi sen yap!</p>}
              </div>
            )}

            {posts.map(post => (
              <PostItem
                key={post.id}
                post={post}
                canDelete={post.user.id === user?.id}
                onDelete={handleDelete}
              />
            ))}

            {posts.length < total && !postsLoading && (
              <button
                onClick={loadMore}
                className="w-full py-3 bg-white border border-gray-200 rounded-2xl text-sm font-semibold text-gray-600 hover:bg-gray-50 transition"
              >
                Daha Fazla Yükle ({posts.length}/{total})
              </button>
            )}
          </div>

        </main>
      </div>
    </div>
  );
}
