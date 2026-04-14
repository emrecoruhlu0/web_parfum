import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, Star, Plus, Check } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { perfumesApi } from '../api/perfumes';
import { collectionApi } from '../api/collection';
import { reviewsApi } from '../api/reviews';
import { useAuth } from '../context/AuthContext';

const STATUS_OPTIONS = [
  { value: 'owned', label: '🧴 Koleksiyonumda Var' },
  { value: 'wishlist', label: '✨ İstek Listeme Ekle' },
  { value: 'tried', label: '👃 Denedim' },
];

function NoteSection({ label, notes, color }) {
  if (!notes) return null;
  const items = notes.split(',').map(n => n.trim()).filter(Boolean);
  return (
    <div>
      <p className={`text-xs font-bold uppercase tracking-widest mb-2 ${color}`}>{label}</p>
      <div className="flex flex-wrap gap-2">
        {items.map(n => (
          <span key={n} className="px-3 py-1 bg-white border border-gray-100 rounded-full text-xs font-medium text-gray-700 shadow-sm">
            {n}
          </span>
        ))}
      </div>
    </div>
  );
}

function StarRating({ value, onChange }) {
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map(i => (
        <button
          key={i}
          onClick={() => onChange(i)}
          className={`text-2xl transition-transform hover:scale-110 ${i <= value ? 'text-amber-400' : 'text-gray-200'}`}
        >
          ★
        </button>
      ))}
    </div>
  );
}

export default function PerfumePage() {
  const { id } = useParams();
  const { user } = useAuth();
  const [perfume, setPerfume] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Koleksiyon
  const [addStatus, setAddStatus] = useState('owned');
  const [addLoading, setAddLoading] = useState(false);
  const [addSuccess, setAddSuccess] = useState(false);

  // Review
  const [rating, setRating] = useState(0);
  const [reviewBody, setReviewBody] = useState('');
  const [reviewLoading, setReviewLoading] = useState(false);
  const [reviewError, setReviewError] = useState('');
  const [reviews, setReviews] = useState([]);

  useEffect(() => {
    perfumesApi.getById(Number(id))
      .then(data => {
        setPerfume(data);
        setReviews(data.reviews || []);
      })
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [id]);

  const handleAddToCollection = async () => {
    setAddLoading(true);
    try {
      await collectionApi.add(Number(id), addStatus);
      setAddSuccess(true);
      setTimeout(() => setAddSuccess(false), 2500);
    } catch (err) {
      // koleksiyonda zaten varsa sessizce geç
    } finally {
      setAddLoading(false);
    }
  };

  const handleReviewSubmit = async (e) => {
    e.preventDefault();
    if (rating === 0) { setReviewError('Lütfen bir puan ver.'); return; }
    setReviewLoading(true);
    setReviewError('');
    try {
      const review = await reviewsApi.add(Number(id), rating, reviewBody);
      setReviews(prev => [{ ...review, user: { username: user.username } }, ...prev]);
      setRating(0);
      setReviewBody('');
    } catch (err) {
      setReviewError(err.message);
    } finally {
      setReviewLoading(false);
    }
  };

  const placeholder = `https://placehold.co/300x300/FDF2F8/E11D48?text=${encodeURIComponent(perfume?.brand?.slice(0, 2) ?? '?')}`;

  if (loading) return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-amber-400 border-t-transparent rounded-full animate-spin" />
      </div>
    </div>
  );

  if (error) return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex items-center justify-center">
        <p className="text-red-500">{error}</p>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[900px] w-full px-6 py-8 space-y-8">

          {/* Geri */}
          <Link to="/" className="inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-800 transition">
            <ArrowLeft size={16} /> Geri Dön
          </Link>

          {/* Hero */}
          <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8 flex gap-10">
            <img
              src={perfume.imageUrl || placeholder}
              alt={perfume.name}
              onError={(e) => { e.target.src = placeholder; }}
              className="w-48 h-48 rounded-2xl object-cover border border-gray-100 shrink-0"
            />
            <div className="flex-1 space-y-4">
              <div>
                <p className="text-xs text-amber-600 font-bold uppercase tracking-widest">{perfume.brand}</p>
                <h1 className="font-serif text-4xl font-bold text-gray-950 mt-1">{perfume.name}</h1>
                <p className="text-sm text-gray-500 mt-1">
                  {[perfume.gender, perfume.country, perfume.year].filter(Boolean).join(' · ')}
                </p>
              </div>

              {perfume.ratingValue && (
                <div className="flex items-center gap-3">
                  <span className="text-3xl font-black text-amber-500">{perfume.ratingValue.toFixed(1)}</span>
                  <div>
                    <div className="flex gap-0.5">
                      {[1,2,3,4,5].map(i => (
                        <span key={i} className={`text-lg ${i <= Math.round(perfume.ratingValue / 2) ? 'text-amber-400' : 'text-gray-200'}`}>★</span>
                      ))}
                    </div>
                    <p className="text-xs text-gray-400">{perfume.ratingCount?.toLocaleString()} oy</p>
                  </div>
                </div>
              )}

              {/* Accord etiketleri */}
              {perfume.accord1 && (
                <div className="flex flex-wrap gap-2">
                  {[perfume.accord1, perfume.accord2, perfume.accord3, perfume.accord4, perfume.accord5]
                    .filter(Boolean)
                    .map(a => (
                      <span key={a} className="px-3 py-1 bg-amber-50 text-amber-700 rounded-full text-xs font-semibold">{a}</span>
                    ))}
                </div>
              )}

              {/* Koleksiyona Ekle */}
              <div className="flex gap-3 pt-2">
                <select
                  value={addStatus}
                  onChange={(e) => setAddStatus(e.target.value)}
                  className="border border-gray-200 rounded-xl px-3 py-2 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-amber-300"
                >
                  {STATUS_OPTIONS.map(o => (
                    <option key={o.value} value={o.value}>{o.label}</option>
                  ))}
                </select>
                <button
                  onClick={handleAddToCollection}
                  disabled={addLoading}
                  className={`flex items-center gap-2 px-5 py-2 rounded-xl font-semibold text-sm transition ${
                    addSuccess
                      ? 'bg-green-500 text-white'
                      : 'bg-amber-600 text-white hover:bg-amber-700'
                  } disabled:opacity-60`}
                >
                  {addSuccess ? <><Check size={16} /> Eklendi!</> : <><Plus size={16} /> Ekle</>}
                </button>
              </div>
            </div>
          </div>

          {/* Notalar */}
          {(perfume.topNotes || perfume.middleNotes || perfume.baseNotes) && (
            <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8 space-y-6">
              <h2 className="font-serif text-2xl font-bold">Koku Notaları</h2>
              <NoteSection label="Üst Notalar" notes={perfume.topNotes} color="text-amber-500" />
              <NoteSection label="Kalp Notalar" notes={perfume.middleNotes} color="text-rose-500" />
              <NoteSection label="Dip Notalar" notes={perfume.baseNotes} color="text-gray-500" />
            </div>
          )}

          {/* Yorum Yaz */}
          <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8">
            <h2 className="font-serif text-2xl font-bold mb-6">Yorum Yaz</h2>
            <form onSubmit={handleReviewSubmit} className="space-y-4">
              <StarRating value={rating} onChange={setRating} />
              <textarea
                value={reviewBody}
                onChange={(e) => setReviewBody(e.target.value)}
                placeholder="Bu koku sana ne hissettiriyor?"
                rows={3}
                className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-amber-300 resize-none"
              />
              {reviewError && <p className="text-red-500 text-sm">{reviewError}</p>}
              <button
                type="submit"
                disabled={reviewLoading}
                className="bg-amber-600 text-white px-6 py-2.5 rounded-xl font-semibold text-sm hover:bg-amber-700 transition disabled:opacity-60"
              >
                {reviewLoading ? 'Gönderiliyor...' : 'Yorumu Gönder'}
              </button>
            </form>
          </div>

          {/* Yorumlar */}
          {reviews.length > 0 && (
            <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8">
              <h2 className="font-serif text-2xl font-bold mb-6">Yorumlar ({reviews.length})</h2>
              <div className="space-y-6">
                {reviews.map(r => (
                  <div key={r.id} className="flex gap-4">
                    <img
                      src={r.user.avatar || `https://i.pravatar.cc/150?u=${r.user.username}`}
                      alt={r.user.username}
                      className="w-10 h-10 rounded-full shrink-0"
                    />
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="font-semibold text-sm">{r.user.username}</span>
                        <span className="text-amber-400 text-sm">{'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</span>
                      </div>
                      {r.body && <p className="text-sm text-gray-600">{r.body}</p>}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
