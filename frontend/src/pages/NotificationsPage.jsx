import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Bell, Heart, UserPlus, Star, CheckCheck } from 'lucide-react';
import Sidebar from '../components/Sidebar';
import { notificationsApi } from '../api/notifications';
import { useNotifications } from '../context/NotificationContext';

const TYPE_META = {
  like:   { icon: Heart,    color: 'text-red-400',   text: 'parfümünü beğendi' },
  follow: { icon: UserPlus, color: 'text-amber-500', text: 'seni takip etmeye başladı' },
  review: { icon: Star,     color: 'text-blue-400',  text: 'koleksiyonundaki parfüme yorum yaptı' },
  community_invite: { icon: Bell, color: 'text-purple-400', text: 'seni bir topluluğa davet etti' },
};

function NotifItem({ notif, onRead }) {
  const meta = TYPE_META[notif.type] || TYPE_META.like;
  const Icon = meta.icon;

  return (
    <div
      onClick={() => !notif.isRead && onRead(notif.id)}
      className={`flex items-start gap-4 p-4 rounded-2xl border transition-colors cursor-pointer ${
        notif.isRead ? 'bg-white border-gray-100' : 'bg-amber-50 border-amber-100'
      }`}
    >
      <Link to={`/user/${notif.actor.username}`} onClick={e => e.stopPropagation()}>
        <img
          src={notif.actor.avatar || `https://i.pravatar.cc/150?u=${notif.actor.username}`}
          alt={notif.actor.username}
          className="w-10 h-10 rounded-full shrink-0"
        />
      </Link>
      <div className="flex-1 min-w-0">
        <p className="text-sm">
          <Link to={`/user/${notif.actor.username}`} className="font-bold hover:text-amber-700" onClick={e => e.stopPropagation()}>
            @{notif.actor.username}
          </Link>
          {' '}
          <span className="text-gray-600">{meta.text}</span>
        </p>
        <p className="text-xs text-gray-400 mt-0.5">{new Date(notif.createdAt).toLocaleDateString('tr-TR')}</p>
      </div>
      <Icon size={18} className={`shrink-0 mt-0.5 ${meta.color}`} />
    </div>
  );
}

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const { setUnreadCount } = useNotifications();

  useEffect(() => {
    notificationsApi.getAll()
      .then(data => setNotifications(data.notifications))
      .finally(() => setLoading(false));
  }, []);

  const handleRead = async (id) => {
    await notificationsApi.markRead(id);
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    setUnreadCount(prev => Math.max(0, prev - 1));
  };

  const handleReadAll = async () => {
    await notificationsApi.markAllRead();
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    setUnreadCount(0);
  };

  return (
    <div className="min-h-screen bg-bg-light flex">
      <Sidebar />
      <div className="flex-1 ml-64 flex justify-center">
        <main className="max-w-[680px] w-full px-6 py-8 space-y-6">

          <div className="flex items-center justify-between pb-4 border-b border-gray-100">
            <h2 className="font-serif text-3xl font-bold flex items-center gap-2">
              <Bell size={28} className="text-amber-500" /> Bildirimler
            </h2>
            {notifications.some(n => !n.isRead) && (
              <button
                onClick={handleReadAll}
                className="flex items-center gap-1.5 text-sm text-amber-700 hover:text-amber-900 font-medium"
              >
                <CheckCheck size={16} /> Tümünü okundu işaretle
              </button>
            )}
          </div>

          {loading && (
            <div className="space-y-3">
              {[1, 2, 3].map(i => <div key={i} className="bg-white rounded-2xl h-16 animate-pulse border border-gray-100" />)}
            </div>
          )}

          {!loading && notifications.length === 0 && (
            <div className="text-center py-16 text-gray-400">
              <Bell size={40} className="mx-auto mb-3 text-gray-200" />
              <p className="font-medium">Henüz bildirim yok.</p>
            </div>
          )}

          {!loading && notifications.length > 0 && (
            <div className="space-y-3">
              {notifications.map(notif => (
                <NotifItem key={notif.id} notif={notif} onRead={handleRead} />
              ))}
            </div>
          )}

        </main>
      </div>
    </div>
  );
}
