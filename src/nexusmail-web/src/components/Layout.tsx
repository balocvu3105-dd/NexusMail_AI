import React, { useState } from 'react';
import { NavLink, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Inbox, Search, Settings, Mail, LogOut } from 'lucide-react';
import { useAuthStore, getRefreshToken } from '../stores/authStore';
import { apiClient } from '../api/client';
import type { LogoutRequest } from '../types/auth';
import { NotificationBell } from './NotificationBell';
import { useNotificationStore } from '../stores/notificationStore';
import styles from './Layout.module.css';

export const Layout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, accessToken, workspaceId, isAuthenticated } = useAuthStore();
  const { connectSignalR, disconnectSignalR, fetchNotifications } = useNotificationStore();
  const [searchQuery, setSearchQuery] = useState('');

  // Sync search query from URL if we are on the search page
  React.useEffect(() => {
    if (location.pathname === '/search') {
      const params = new URLSearchParams(location.search);
      setSearchQuery(params.get('q') || '');
    } else {
      setSearchQuery('');
    }
  }, [location]);

  // Connect SignalR and Fetch initial notifications
  React.useEffect(() => {
    if (isAuthenticated && accessToken && workspaceId) {
      fetchNotifications();
      connectSignalR(accessToken, workspaceId);
    }
    
    return () => {
      // Disconnect on unmount (if layout unmounts e.g. on logout)
      disconnectSignalR();
    };
  }, [isAuthenticated, accessToken, workspaceId, connectSignalR, disconnectSignalR, fetchNotifications]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
    } else {
      navigate(`/search`);
    }
  };

  const handleLogout = async () => {
    try {
      const refreshToken = getRefreshToken();
      if (refreshToken) {
        const payload: LogoutRequest = { refreshToken };
        await apiClient.post('/identity/logout', payload);
      }
    } catch (err) {
      console.error('Logout API failed, continuing with local logout', err);
    } finally {
      logout();
    }
  };

  return (
    <div className={styles.container}>
      <aside className={styles.sidebar}>
        <div className={styles.brand}>
          <Mail className={styles.brandIcon} />
          <span className={styles.brandName}>NexusMail</span>
        </div>
        <div className={styles.searchContainer}>
          <form onSubmit={handleSearch} className={styles.searchForm}>
            <Search size={16} className={styles.searchIcon} />
            <input 
              type="text" 
              placeholder="Search..." 
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
              className={styles.searchInput}
            />
          </form>
        </div>
        <nav className={styles.nav}>
          <NavLink 
            to="/" 
            className={({ isActive }) => isActive ? `${styles.navItem} ${styles.active}` : styles.navItem}
          >
            <Inbox size={20} />
            <span>Inbox</span>
          </NavLink>
          <NavLink 
            to="/search" 
            className={({ isActive }) => isActive ? `${styles.navItem} ${styles.active}` : styles.navItem}
          >
            <Search size={20} />
            <span>Search</span>
          </NavLink>
          <NavLink 
            to="/rules" 
            className={({ isActive }) => isActive ? `${styles.navItem} ${styles.active}` : styles.navItem}
          >
            <Settings size={20} />
            <span>Automation</span>
          </NavLink>
        </nav>

        <div className={styles.sidebarFooter}>
          <div style={{ marginBottom: '16px', display: 'flex', justifyContent: 'center' }}>
            <NotificationBell />
          </div>
          <button onClick={handleLogout} className={styles.logoutBtn}>
            <LogOut size={20} />
            <span>Sign out</span>
          </button>
        </div>
      </aside>
      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  );
};
