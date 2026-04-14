import React from 'react';
import { TrendingUp, UserPlus } from 'lucide-react';

const trendingNotes = ['Ud', 'Safran', 'Buhur', 'Yasemin', 'Deri'];

const suggestedPerfumers = [
  { name: 'Kerem Koku-cu', avatar: 'https://i.pravatar.cc/150?u=kerem' },
  { name: 'Scent Master', avatar: 'https://i.pravatar.cc/150?u=scent' },
];

export default function Widgets() {
  return (
    <div className="space-y-8">
      {/* Arama Barı */}
      <div className="glass p-3 rounded-2xl border border-white/50 shadow-inner">
        <input type="search" placeholder="Parfüm veya koku notası ara..." className="w-full bg-transparent p-2 text-sm outline-none"/>
      </div>

      {/* Trend Olan Notalar */}
      <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
        <h4 className="flex items-center gap-2 font-serif text-xl font-bold mb-4">
          <TrendingUp className="text-amber-500" size={20}/> Popüler Notalar
        </h4>
        <div className="flex flex-wrap gap-2">
          {trendingNotes.map(note => (
            <span key={note} className="px-3 py-1.5 bg-accent-peach text-accent-rose rounded-full text-xs font-semibold hover:bg-amber-100 transition cursor-pointer">#{note}</span>
          ))}
        </div>
      </div>

      {/* Önerilen Kişiler */}
      <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
        <h4 className="flex items-center gap-2 font-serif text-xl font-bold mb-4">
          <UserPlus className="text-amber-500" size={20}/> Koku Kaşifleri
        </h4>
        <div className="space-y-4">
          {suggestedPerfumers.map(p => (
            <div key={p.name} className="flex items-center justify-between gap-3">
                <div className="flex items-center gap-3">
                    <img src={p.avatar} alt={p.name} className="w-10 h-10 rounded-full"/>
                    <p className="font-semibold text-sm">{p.name}</p>
                </div>
                <button className="bg-gray-100 text-gray-800 text-xs px-3 py-1.5 rounded-lg font-semibold hover:bg-gray-200">Takip Et</button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}