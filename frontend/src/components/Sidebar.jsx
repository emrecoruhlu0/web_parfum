import React from 'react';
import { Home, Compass, FlaskConical, User, Settings, Droplet, LogOut, Users, MessageCircle, Bell } from 'lucide-react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useNotifications } from '../context/NotificationContext';

const navItems = [
  { name: 'Ana Akış', icon: Home, to: '/' },
  { name: 'Koku Keşfet', icon: Compass, to: '/discover' },
  { name: 'Koku Laboratuvarı', icon: FlaskConical, to: '/lab' },
  { name: 'Koleksiyonum', icon: Droplet, to: '/collection' },
  { name: 'Topluluklar', icon: Users, to: '/communities' },
  { name: 'Mesajlar', icon: MessageCircle, to: '/messages' },
  { name: 'Bildirimler', icon: Bell, to: '/notifications', badge: true },
  { name: 'Profil', icon: User, to: '/profile' },
];

export default function Sidebar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { unreadCount } = useNotifications();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside className="w-64 h-screen fixed top-0 left-0 bg-white border-r border-gray-100 p-6 flex flex-col justify-between">
      <div>
        {/* Logo */}
        <div className="flex items-center gap-2 mb-12">
          <Droplet className="text-amber-500" size={32} />
          <h1 className="font-serif text-3xl font-bold tracking-tight text-gray-950">
            Koku<span className="text-amber-600">.</span>
          </h1>
        </div>

        {/* Menü Linkleri */}
        <nav className="space-y-2">
          {navItems.map((item) => (
            <NavLink
              key={item.name}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) =>
                `group flex items-center gap-4 px-4 py-3 rounded-xl text-lg font-medium transition-colors ${
                  isActive
                    ? 'bg-amber-50 text-amber-700'
                    : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
                }`
              }
            >
              {({ isActive }) => (
                <>
                  <span className="relative">
                    <item.icon
                      size={24}
                      strokeWidth={isActive ? 2 : 1.5}
                      className={isActive ? 'text-amber-600' : 'text-gray-400 group-hover:text-gray-600'}
                    />
                    {item.badge && unreadCount > 0 && (
                      <span className="absolute -top-1.5 -right-1.5 bg-red-500 text-white text-[10px] font-bold rounded-full min-w-[16px] h-4 flex items-center justify-center px-0.5">
                        {unreadCount > 99 ? '99+' : unreadCount}
                      </span>
                    )}
                  </span>
                  {item.name}
                </>
              )}
            </NavLink>
          ))}
        </nav>
      </div>

      {/* Alt: Ayarlar ve Kullanıcı */}
      <div className="space-y-4 pt-6 border-t border-gray-100">
        <a href="#" className="flex items-center gap-4 px-4 py-3 text-gray-600 hover:text-gray-900 font-medium rounded-xl hover:bg-gray-50 transition-colors">
          <Settings size={24} strokeWidth={1.5} /> Ayarlar
        </a>
        <div className="flex items-center gap-3 bg-gray-50 p-3 rounded-2xl border border-gray-100">
          <img
            src={user?.avatar || `https://i.pravatar.cc/150?u=${user?.username}`}
            alt={user?.username}
            className="w-10 h-10 rounded-full"
          />
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-sm truncate">{user?.username}</p>
            <p className="text-xs text-text-gray truncate">@{user?.username}</p>
          </div>
          <button onClick={handleLogout} title="Çıkış Yap" className="text-gray-400 hover:text-red-500 transition-colors">
            <LogOut size={18} />
          </button>
        </div>
      </div>
    </aside>
  );
}
