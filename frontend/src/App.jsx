import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { NotificationProvider } from './context/NotificationContext';
import FeedPage from './pages/FeedPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CollectionPage from './pages/CollectionPage';
import PerfumePage from './pages/PerfumePage';
import DailyLogPage from './pages/DailyLogPage';
import ProfilePage from './pages/ProfilePage';
import UserPage from './pages/UserPage';
import NotificationsPage from './pages/NotificationsPage';
import CommunitiesPage from './pages/CommunitiesPage';
import CommunityDetailPage from './pages/CommunityDetailPage';
import MessagesPage from './pages/MessagesPage';
import DiscoverPage from './pages/DiscoverPage';

function PrivateRoute({ children }) {
  const { isAuth, loading } = useAuth();
  if (loading) return null;
  return isAuth ? children : <Navigate to="/login" replace />;
}

function PublicRoute({ children }) {
  const { isAuth, loading } = useAuth();
  if (loading) return null;
  return !isAuth ? children : <Navigate to="/" replace />;
}

export default function App() {
  return (
    <AuthProvider>
      <NotificationProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<PrivateRoute><FeedPage /></PrivateRoute>} />
            <Route path="/collection" element={<PrivateRoute><CollectionPage /></PrivateRoute>} />
            <Route path="/perfume/:id" element={<PrivateRoute><PerfumePage /></PrivateRoute>} />
            <Route path="/log" element={<PrivateRoute><DailyLogPage /></PrivateRoute>} />
            <Route path="/profile" element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
            <Route path="/user/:username" element={<PrivateRoute><UserPage /></PrivateRoute>} />
            <Route path="/notifications" element={<PrivateRoute><NotificationsPage /></PrivateRoute>} />
            <Route path="/communities" element={<PrivateRoute><CommunitiesPage /></PrivateRoute>} />
            <Route path="/communities/:id" element={<PrivateRoute><CommunityDetailPage /></PrivateRoute>} />
            <Route path="/messages" element={<PrivateRoute><MessagesPage /></PrivateRoute>} />
            <Route path="/discover" element={<PrivateRoute><DiscoverPage /></PrivateRoute>} />
            <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
            <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </NotificationProvider>
    </AuthProvider>
  );
}
