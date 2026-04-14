import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Heart, MessageCircle, Wind, Droplets } from 'lucide-react';
import { socialApi } from '../api/social';

function NoteTag({ label }) {
  return (
    <span className="px-2 py-0.5 bg-white text-gray-800 rounded text-xs font-medium">{label}</span>
  );
}

function NotesRow({ label, notes, color }) {
  if (!notes) return null;
  const items = notes.split(',').map(n => n.trim()).filter(Boolean).slice(0, 4);
  return (
    <div className="flex items-center gap-2">
      <span className={`w-10 text-xs font-semibold ${color}`}>{label}</span>
      <div className="flex gap-1 flex-wrap">
        {items.map(n => <NoteTag key={n} label={n} />)}
      </div>
    </div>
  );
}

export default function ScentCard({ perfume, initialLiked = false }) {
  const [isLiked, setIsLiked] = useState(initialLiked);
  const [likeCount, setLikeCount] = useState(perfume._count?.likes ?? 0);

  const handleLike = async () => {
    // Optimistic update
    setIsLiked(prev => !prev);
    setLikeCount(prev => isLiked ? prev - 1 : prev + 1);
    try {
      const data = await socialApi.toggleLike(perfume.id);
      setIsLiked(data.liked);
      setLikeCount(data.count);
    } catch {
      // Hata durumunda geri al
      setIsLiked(prev => !prev);
      setLikeCount(prev => isLiked ? prev + 1 : prev - 1);
    }
  };

  const placeholderImg = `https://placehold.co/200x200/FDF2F8/E11D48?text=${encodeURIComponent(perfume.brand?.slice(0, 2) ?? '?')}`;

  return (
    <div className="relative group bg-white p-6 rounded-3xl border border-gray-100 shadow-sm hover:shadow-lg transition-shadow duration-300">
      <div className="absolute -inset-1 bg-gradient-to-r from-accent-peach via-accent-amber to-accent-rose rounded-[2rem] blur opacity-5 group-hover:opacity-15 transition duration-1000" />

      <div className="relative">
        {/* Üst: marka + puan */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <p className="text-xs text-text-gray font-medium uppercase tracking-widest">{perfume.brand}</p>
            {perfume.country && <p className="text-xs text-gray-400">{perfume.country}</p>}
          </div>
          {perfume.ratingValue && (
            <div className="bg-text-dark text-white px-3.5 py-1 rounded-full text-sm font-bold shadow-md">
              {perfume.ratingValue.toFixed(1)} <span className="text-xs text-gray-400">/ 10</span>
            </div>
          )}
        </div>

        {/* İçerik: görsel + bilgiler */}
        <div className="flex gap-6 mb-6">
          <Link to={`/perfume/${perfume.id}`} className="w-40 aspect-square rounded-2xl overflow-hidden bg-gray-50 flex items-center justify-center border border-gray-100 shrink-0">
            <img
              src={perfume.imageUrl || placeholderImg}
              alt={perfume.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
              onError={(e) => { e.target.src = placeholderImg; }}
            />
          </Link>
          <div className="flex-1 space-y-4">
            <div className="bg-gray-50 border border-gray-100 px-4 py-3 rounded-xl">
              <h3 className="font-extrabold text-gray-950 tracking-tight text-xl">{perfume.name}</h3>
              {perfume.gender && <p className="text-xs text-text-gray mt-0.5">{perfume.gender} · {perfume.year ?? '—'}</p>}
            </div>

            {(perfume.topNotes || perfume.middleNotes || perfume.baseNotes) && (
              <div className="space-y-2 text-sm bg-accent-peach/50 p-3 rounded-lg border border-accent-peach">
                <NotesRow label="Üst" notes={perfume.topNotes} color="text-accent-amber" />
                <NotesRow label="Kalp" notes={perfume.middleNotes} color="text-accent-rose" />
                <NotesRow label="Dip" notes={perfume.baseNotes} color="text-gray-500" />
              </div>
            )}

            {(perfume.accord1 || perfume.accord2) && (
              <div className="flex flex-wrap gap-1">
                {[perfume.accord1, perfume.accord2, perfume.accord3].filter(Boolean).map(a => (
                  <span key={a} className="px-2 py-0.5 bg-amber-50 text-amber-700 rounded-full text-xs font-medium">
                    {a}
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Etkileşim */}
        <div className="flex items-center justify-between pt-4 border-t border-gray-100">
          <div className="flex gap-6">
            <button
              onClick={handleLike}
              className="group/like flex items-center gap-2 text-gray-500 hover:text-red-500 transition-colors"
            >
              <Heart
                size={20}
                strokeWidth={isLiked ? 0 : 1.5}
                className={isLiked ? 'fill-red-500 text-red-500' : 'group-hover/like:text-red-400'}
              />
              <span className="text-sm font-semibold">{likeCount}</span>
            </button>
            <Link
              to={`/perfume/${perfume.id}`}
              className="group/msg flex items-center gap-2 text-gray-500 hover:text-blue-500 transition-colors"
            >
              <MessageCircle size={20} className="group-hover/msg:text-blue-400" />
              <span className="text-sm font-semibold">{perfume._count?.reviews ?? 0}</span>
            </Link>
          </div>
          <div className="flex gap-4 text-xs text-text-gray">
            <span className="flex items-center gap-1"><Wind size={14} /> Silaj</span>
            <span className="flex items-center gap-1"><Droplets size={14} /> Kalıcılık</span>
          </div>
        </div>
      </div>
    </div>
  );
}
