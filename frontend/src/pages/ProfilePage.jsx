import React, { useEffect, useState } from 'react';
import { Droplet, BookOpen, FlaskConical } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { dailylogApi } from '../api/dailylog';
import { useAuth } from '../context/AuthContext';

function BarItem({ label, count, max, color }) {
  const pct = max > 0 ? Math.round((count / max) * 100) : 0;
  return (
    <div className="flex items-center gap-3">
      <span className="w-32 text-sm font-medium text-gray-700 truncate shrink-0">{label}</span>
      <div className="flex-1 bg-gray-100 rounded-full h-2.5">
        <div
          className={`${color} h-2.5 rounded-full transition-all duration-500`}
          style={{ width: `${pct}%` }}
        />
      </div>
      <span className="w-6 text-xs text-gray-400 text-right">{count}</span>
    </div>
  );
}

export default function ProfilePage() {
  const { user } = useAuth();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    dailylogApi.getProfile()
      .then(setProfile)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  const maxNote = profile?.topNotes?.[0]?.count ?? 1;
  const maxAccord = profile?.topAccords?.[0]?.count ?? 1;

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[900px] w-full px-6 py-8 space-y-8">

          {/* Kullanıcı Kartı */}
          <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8 flex items-center gap-6">
            <img
              src={user?.avatar || `https://i.pravatar.cc/150?u=${user?.username}`}
              alt={user?.username}
              className="w-20 h-20 rounded-full border-2 border-amber-100"
            />
            <div>
              <h1 className="font-serif text-3xl font-bold text-gray-950">{user?.username}</h1>
              <p className="text-text-gray text-sm mt-0.5">@{user?.username}</p>
              {!loading && profile && (
                <p className="text-amber-600 text-sm font-semibold mt-2">
                  {profile.totalLogs} koku logu · {profile.topNotes?.length ?? 0} farklı nota
                </p>
              )}
            </div>
          </div>

          {loading && (
            <div className="space-y-4">
              {[1, 2].map(i => <div key={i} className="bg-white rounded-3xl border border-gray-100 h-48 animate-pulse" />)}
            </div>
          )}

          {error && (
            <div className="bg-red-50 text-red-600 text-sm px-4 py-3 rounded-xl">{error}</div>
          )}

          {!loading && !error && profile?.totalLogs === 0 && (
            <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-12 text-center text-gray-400">
              <FlaskConical size={48} className="mx-auto mb-4 opacity-30" />
              <p className="font-medium">Henüz koku logu yok.</p>
              <p className="text-sm mt-1">Günlük log ekledikçe koku profilin burada oluşur.</p>
            </div>
          )}

          {!loading && !error && profile && profile.totalLogs > 0 && (
            <>
              {/* En Çok Kullanılan Notalar */}
              <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8">
                <h2 className="font-serif text-2xl font-bold mb-2 flex items-center gap-2">
                  <Droplet className="text-amber-500" size={22} /> Nota Profilin
                </h2>
                <p className="text-text-gray text-sm mb-6">En çok taşıdığın koku notaları.</p>
                <div className="space-y-3">
                  {profile.topNotes.slice(0, 15).map(({ note, count }) => (
                    <BarItem key={note} label={note} count={count} max={maxNote} color="bg-amber-400" />
                  ))}
                </div>
              </div>

              {/* En Çok Kullanılan Accord'lar */}
              {profile.topAccords.length > 0 && (
                <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8">
                  <h2 className="font-serif text-2xl font-bold mb-2 flex items-center gap-2">
                    <BookOpen className="text-amber-500" size={22} /> Koku Karakterin
                  </h2>
                  <p className="text-text-gray text-sm mb-6">Seni tanımlayan ana accord'lar.</p>
                  <div className="flex flex-wrap gap-3">
                    {profile.topAccords.map(({ accord, count }, i) => {
                      const size = i === 0 ? 'text-base px-5 py-2.5' : i < 3 ? 'text-sm px-4 py-2' : 'text-xs px-3 py-1.5';
                      return (
                        <span
                          key={accord}
                          className={`${size} bg-amber-50 text-amber-700 rounded-full font-semibold border border-amber-100`}
                        >
                          {accord}
                          <span className="ml-1.5 text-amber-400 font-normal text-xs">{count}x</span>
                        </span>
                      );
                    })}
                  </div>
                  <div className="mt-8 space-y-3">
                    {profile.topAccords.map(({ accord, count }) => (
                      <BarItem key={accord} label={accord} count={count} max={maxAccord} color="bg-rose-400" />
                    ))}
                  </div>
                </div>
              )}
            </>
          )}

        </main>
      </div>
    </div>
  );
}
