import React, { useEffect, useState, useCallback } from 'react';
import { ChevronLeft, ChevronRight, Plus, X, Trash2, Search } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { dailylogApi } from '../api/dailylog';
import { perfumesApi } from '../api/perfumes';

const DAYS = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];
const MONTHS = ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran','Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'];

function toYYYYMM(date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
}

function toDateStr(date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}

// Takvim grid'i için ayın günlerini üret (Mon-start)
function buildCalendar(year, month) {
  const firstDay = new Date(year, month, 1);
  // JS: 0=Sun, offset to Mon-start
  const startOffset = (firstDay.getDay() + 6) % 7;
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const cells = [];
  for (let i = 0; i < startOffset; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(d);
  return cells;
}

function AddLogModal({ date, onClose, onAdd }) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState([]);
  const [selected, setSelected] = useState(null);
  const [note, setNote] = useState('');
  const [sprays, setSprays] = useState('');
  const [searching, setSearching] = useState(false);
  const [saving, setSaving] = useState(false);

  const handleSearch = async (q) => {
    setQuery(q);
    if (q.length < 2) { setResults([]); return; }
    setSearching(true);
    try {
      const data = await perfumesApi.getAll({ search: q, limit: 6 });
      setResults(data.perfumes);
    } finally {
      setSearching(false);
    }
  };

  const handleSave = async () => {
    if (!selected) return;
    setSaving(true);
    try {
      const log = await dailylogApi.add(selected.id, date, note || undefined, sprays ? Number(sprays) : undefined);
      onAdd(log);
      onClose();
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 px-4">
      <div className="bg-white rounded-3xl shadow-xl w-full max-w-md p-6">
        <div className="flex justify-between items-center mb-4">
          <h3 className="font-serif text-xl font-bold">Log Ekle — {date}</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700"><X size={20} /></button>
        </div>

        {/* Parfüm Ara */}
        {!selected ? (
          <div>
            <div className="relative mb-3">
              <Search size={16} className="absolute left-3 top-3 text-gray-400" />
              <input
                autoFocus
                type="text"
                value={query}
                onChange={(e) => handleSearch(e.target.value)}
                placeholder="Parfüm adı veya marka ara..."
                className="w-full pl-9 pr-4 py-2.5 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-amber-300"
              />
            </div>
            {searching && <p className="text-xs text-gray-400 text-center py-2">Aranıyor...</p>}
            <div className="space-y-1 max-h-52 overflow-y-auto">
              {results.map(p => (
                <button
                  key={p.id}
                  onClick={() => setSelected(p)}
                  className="w-full text-left px-3 py-2.5 rounded-xl hover:bg-amber-50 transition-colors flex items-center gap-3"
                >
                  <div className="w-10 h-10 rounded-lg bg-amber-50 border border-gray-100 flex items-center justify-center text-xs font-bold text-amber-700 shrink-0">
                    {p.brand?.slice(0, 2)}
                  </div>
                  <div className="min-w-0">
                    <p className="font-semibold text-sm truncate">{p.name}</p>
                    <p className="text-xs text-gray-400">{p.brand}</p>
                  </div>
                </button>
              ))}
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <div className="flex items-center gap-3 bg-amber-50 px-4 py-3 rounded-xl">
              <div className="w-10 h-10 rounded-lg bg-white border border-gray-100 flex items-center justify-center text-xs font-bold text-amber-700 shrink-0">
                {selected.brand?.slice(0, 2)}
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-sm truncate">{selected.name}</p>
                <p className="text-xs text-gray-400">{selected.brand}</p>
              </div>
              <button onClick={() => setSelected(null)} className="text-gray-400 hover:text-gray-700"><X size={16} /></button>
            </div>

            <div>
              <label className="text-xs font-semibold text-gray-600 block mb-1">Kaç puf? (opsiyonel)</label>
              <input
                type="number"
                min="1"
                value={sprays}
                onChange={(e) => setSprays(e.target.value)}
                placeholder="3"
                className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-300"
              />
            </div>
            <div>
              <label className="text-xs font-semibold text-gray-600 block mb-1">Not (opsiyonel)</label>
              <textarea
                value={note}
                onChange={(e) => setNote(e.target.value)}
                placeholder="Bugün bu kokuyu tercih etmemin sebebi..."
                rows={3}
                className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-300 resize-none"
              />
            </div>
            <button
              onClick={handleSave}
              disabled={saving}
              className="w-full bg-amber-600 text-white py-2.5 rounded-xl font-semibold text-sm hover:bg-amber-700 transition disabled:opacity-60"
            >
              {saving ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default function DailyLogPage() {
  const today = new Date();
  const [current, setCurrent] = useState({ year: today.getFullYear(), month: today.getMonth() });
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalDate, setModalDate] = useState(null);
  const [selectedDay, setSelectedDay] = useState(null);

  const fetchLogs = useCallback((year, month) => {
    setLoading(true);
    const monthStr = `${year}-${String(month + 1).padStart(2, '0')}`;
    dailylogApi.getByMonth(monthStr)
      .then(setLogs)
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    fetchLogs(current.year, current.month);
  }, [current, fetchLogs]);

  const prevMonth = () => setCurrent(c => {
    const d = new Date(c.year, c.month - 1);
    return { year: d.getFullYear(), month: d.getMonth() };
  });
  const nextMonth = () => setCurrent(c => {
    const d = new Date(c.year, c.month + 1);
    return { year: d.getFullYear(), month: d.getMonth() };
  });

  const logsByDate = logs.reduce((acc, log) => {
    const key = toDateStr(new Date(log.date));
    if (!acc[key]) acc[key] = [];
    acc[key].push(log);
    return acc;
  }, {});

  const cells = buildCalendar(current.year, current.month);

  const handleDayClick = (day) => {
    const dateStr = toDateStr(new Date(current.year, current.month, day));
    setSelectedDay(dateStr);
  };

  const handleAddLog = (log) => {
    setLogs(prev => [log, ...prev]);
  };

  const handleRemoveLog = async (logId) => {
    await dailylogApi.remove(logId);
    setLogs(prev => prev.filter(l => l.id !== logId));
  };

  const selectedLogs = selectedDay ? (logsByDate[selectedDay] || []) : [];

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[900px] w-full px-6 py-8">

          <header className="mb-8">
            <h1 className="font-serif text-4xl font-bold text-gray-950">Koku Günlüğü</h1>
            <p className="text-text-gray text-sm mt-1">Her gün taktığın kokuyu takip et.</p>
          </header>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

            {/* Takvim */}
            <div className="lg:col-span-2 bg-white rounded-3xl border border-gray-100 shadow-sm p-6">
              {/* Ay Navigasyonu */}
              <div className="flex items-center justify-between mb-6">
                <button onClick={prevMonth} className="p-2 hover:bg-gray-100 rounded-xl transition">
                  <ChevronLeft size={20} />
                </button>
                <h2 className="font-serif text-xl font-bold">
                  {MONTHS[current.month]} {current.year}
                </h2>
                <button onClick={nextMonth} className="p-2 hover:bg-gray-100 rounded-xl transition">
                  <ChevronRight size={20} />
                </button>
              </div>

              {/* Gün başlıkları */}
              <div className="grid grid-cols-7 mb-2">
                {DAYS.map(d => (
                  <div key={d} className="text-center text-xs font-bold text-gray-400 py-1">{d}</div>
                ))}
              </div>

              {/* Takvim hücreleri */}
              {loading ? (
                <div className="grid grid-cols-7 gap-1">
                  {Array(35).fill(0).map((_, i) => (
                    <div key={i} className="h-10 bg-gray-50 rounded-xl animate-pulse" />
                  ))}
                </div>
              ) : (
                <div className="grid grid-cols-7 gap-1">
                  {cells.map((day, i) => {
                    if (!day) return <div key={i} />;
                    const dateStr = toDateStr(new Date(current.year, current.month, day));
                    const dayLogs = logsByDate[dateStr] || [];
                    const isToday = dateStr === toDateStr(today);
                    const isSelected = dateStr === selectedDay;

                    return (
                      <button
                        key={i}
                        onClick={() => handleDayClick(day)}
                        className={`relative h-10 rounded-xl text-sm font-medium transition-colors flex flex-col items-center justify-center gap-0.5 ${
                          isSelected
                            ? 'bg-amber-600 text-white'
                            : isToday
                            ? 'bg-amber-50 text-amber-700 font-bold'
                            : 'hover:bg-gray-50 text-gray-700'
                        }`}
                      >
                        {day}
                        {dayLogs.length > 0 && (
                          <div className={`w-1.5 h-1.5 rounded-full ${isSelected ? 'bg-white' : 'bg-amber-400'}`} />
                        )}
                      </button>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Sağ Panel: Seçili Gün */}
            <div className="bg-white rounded-3xl border border-gray-100 shadow-sm p-6 flex flex-col">
              {selectedDay ? (
                <>
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="font-semibold text-gray-900">{selectedDay}</h3>
                    <button
                      onClick={() => setModalDate(selectedDay)}
                      className="flex items-center gap-1 text-xs bg-amber-600 text-white px-3 py-1.5 rounded-lg font-semibold hover:bg-amber-700 transition"
                    >
                      <Plus size={14} /> Ekle
                    </button>
                  </div>

                  {selectedLogs.length === 0 ? (
                    <div className="flex-1 flex flex-col items-center justify-center text-gray-400 text-center">
                      <p className="text-sm">Bu gün için log yok.</p>
                      <button
                        onClick={() => setModalDate(selectedDay)}
                        className="mt-3 text-amber-600 text-sm font-semibold hover:underline"
                      >
                        + Log Ekle
                      </button>
                    </div>
                  ) : (
                    <div className="space-y-3 overflow-y-auto">
                      {selectedLogs.map(log => (
                        <div key={log.id} className="flex items-start gap-3 bg-gray-50 rounded-xl p-3">
                          <div className="w-9 h-9 rounded-lg bg-amber-50 border border-gray-100 flex items-center justify-center text-xs font-bold text-amber-700 shrink-0">
                            {log.perfume.brand?.slice(0, 2)}
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="font-semibold text-sm truncate">{log.perfume.name}</p>
                            <p className="text-xs text-gray-400">{log.perfume.brand}</p>
                            {log.sprays && <p className="text-xs text-amber-600 mt-0.5">{log.sprays} puf</p>}
                            {log.note && <p className="text-xs text-gray-500 mt-1 italic">"{log.note}"</p>}
                          </div>
                          <button
                            onClick={() => handleRemoveLog(log.id)}
                            className="text-gray-300 hover:text-red-400 transition-colors shrink-0"
                          >
                            <Trash2 size={14} />
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </>
              ) : (
                <div className="flex-1 flex flex-col items-center justify-center text-gray-400 text-center">
                  <p className="text-sm">Takvimden bir gün seç.</p>
                </div>
              )}
            </div>

          </div>
        </main>
      </div>

      {modalDate && (
        <AddLogModal
          date={modalDate}
          onClose={() => setModalDate(null)}
          onAdd={handleAddLog}
        />
      )}
    </div>
  );
}
