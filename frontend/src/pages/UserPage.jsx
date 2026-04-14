import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, UserPlus, UserCheck } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { socialApi } from '../api/social';
import { useAuth } from '../context/AuthContext';

const TABS = [
  { key: 'owned', label: '🧴 Koleksiyon' },
  { key: 'wishlist', label: '✨ İstek Listesi' },
  { key: 'tried', label: '👃 Denedikleri' },
];

export default function UserPage() {
  const { username } = useParams();
  const { user: me } = useAuth();
  const [profile, setProfile] = useState(null);
  const [collection, setCollection] = useState([]);
  const [activeTab, setActiveTab] = useState('owned');
  const [loading, setLoading] = useState(true);
  const [colLoading, setColLoading] = useState(false);
  const [following, setFollowing] = useState(false);
  const [followLoading, setFollowLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    socialApi.getUser(username)
      .then(data => {
        setProfile(data);
        setFollowing(data.isFollowing);
      })
      .finally(() => setLoading(false));
  }, [username]);

  useEffect(() => {
    if (!profile) return;
    setColLoading(true);
    socialApi.getUserCollection(username, activeTab)
      .then(setCollection)
      .finally(() => setColLoading(false));
  }, [username, activeTab, profile]);

  const handleFollow = async () => {
    setFollowLoading(true);
    try {
      const data = await socialApi.toggleFollow(profile.id);
      setFollowing(data.following);
      setProfile(prev => ({
        ...prev,
        _count: {
          ...prev._count,
          followers: data.following ? prev._count.followers + 1 : prev._count.followers - 1,
        },
      }));
    } finally {
      setFollowLoading(false);
    }
  };

  const isMe = me?.username === username;

  if (loading) return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-amber-400 border-t-transparent rounded-full animate-spin" />
      </div>
    </div>
  );

  if (!profile) return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex items-center justify-center text-gray-400">Kullanıcı bulunamadı.</div>
    </div>
  );

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[900px] w-full px-6 py-8 space-y-8">

          <Link to="/" className="inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-800 transition">
            <ArrowLeft size={16} /> Geri Dön
          </Link>

          {/* Profil Kartı */}
          <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8 flex items-center gap-6">
            <img
              src={profile.avatar || `https://i.pravatar.cc/150?u=${profile.username}`}
              alt={profile.username}
              className="w-20 h-20 rounded-full border-2 border-amber-100"
            />
            <div className="flex-1">
              <h1 className="font-serif text-3xl font-bold text-gray-950">{profile.username}</h1>
              <p className="text-text-gray text-sm mt-0.5">@{profile.username}</p>
              {profile.bio && <p className="text-gray-600 text-sm mt-2">{profile.bio}</p>}

              <div className="flex gap-6 mt-4 text-sm">
                <div className="text-center">
                  <p className="font-bold text-gray-900">{profile._count.followers}</p>
                  <p className="text-text-gray text-xs">Takipçi</p>
                </div>
                <div className="text-center">
                  <p className="font-bold text-gray-900">{profile._count.following}</p>
                  <p className="text-text-gray text-xs">Takip</p>
                </div>
                <div className="text-center">
                  <p className="font-bold text-gray-900">{profile._count.collections}</p>
                  <p className="text-text-gray text-xs">Koleksiyon</p>
                </div>
                <div className="text-center">
                  <p className="font-bold text-gray-900">{profile._count.reviews}</p>
                  <p className="text-text-gray text-xs">Yorum</p>
                </div>
              </div>
            </div>

            {!isMe && (
              <button
                onClick={handleFollow}
                disabled={followLoading}
                className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm transition disabled:opacity-60 ${
                  following
                    ? 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    : 'bg-amber-600 text-white hover:bg-amber-700'
                }`}
              >
                {following ? <><UserCheck size={16} /> Takip Ediliyor</> : <><UserPlus size={16} /> Takip Et</>}
              </button>
            )}
          </div>

          {/* Koleksiyon Tabs */}
          <div>
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
                  {tab.label}
                </button>
              ))}
            </div>

            {colLoading && (
              <div className="grid grid-cols-2 gap-4">
                {[1, 2, 3, 4].map(i => <div key={i} className="bg-white rounded-2xl h-28 animate-pulse border border-gray-100" />)}
              </div>
            )}

            {!colLoading && collection.length === 0 && (
              <p className="text-center text-gray-400 py-12 text-sm">Bu liste boş.</p>
            )}

            {!colLoading && collection.length > 0 && (
              <div className="grid grid-cols-2 gap-4">
                {collection.map(item => {
                  const placeholder = `https://placehold.co/80x80/FDF2F8/E11D48?text=${encodeURIComponent(item.perfume.brand?.slice(0, 2) ?? '?')}`;
                  return (
                    <Link
                      key={item.id}
                      to={`/perfume/${item.perfume.id}`}
                      className="bg-white rounded-2xl border border-gray-100 shadow-sm p-4 flex gap-3 hover:shadow-md transition-shadow"
                    >
                      <img
                        src={item.perfume.imageUrl || placeholder}
                        alt={item.perfume.name}
                        onError={(e) => { e.target.src = placeholder; }}
                        className="w-14 h-14 rounded-xl object-cover border border-gray-100 shrink-0"
                      />
                      <div className="min-w-0">
                        <p className="font-bold text-sm text-gray-900 truncate">{item.perfume.name}</p>
                        <p className="text-xs text-gray-400 mt-0.5">{item.perfume.brand}</p>
                        {item.perfume.ratingValue && (
                          <p className="text-xs text-amber-600 font-semibold mt-1">★ {item.perfume.ratingValue.toFixed(1)}</p>
                        )}
                      </div>
                    </Link>
                  );
                })}
              </div>
            )}
          </div>

        </main>
      </div>
    </div>
  );
}
