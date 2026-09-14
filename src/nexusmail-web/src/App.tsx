import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { Layout } from './components/Layout';
import { InboxPage } from './pages/InboxPage';
import { EmailDetailPage } from './pages/EmailDetailPage';
import { SearchPage } from './pages/SearchPage';
import { LoginPage } from './pages/LoginPage';
import { AutomationPage } from './pages/AutomationPage';
import { AnalyticsPage } from './pages/AnalyticsPage';
import { useAuthStore, getRefreshToken } from './stores/authStore';
import { apiClient } from './api/client';
import type { AuthResponse, RefreshTokenRequest } from './types/auth';

const ProtectedRoute: React.FC = () => {
  const { isAuthenticated } = useAuthStore();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  return <Outlet />;
};

export const App: React.FC = () => {
  const { login, logout } = useAuthStore();
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  useEffect(() => {
    const bootstrapSession = async () => {
      const refreshToken = getRefreshToken();
      if (!refreshToken) {
        setIsBootstrapping(false);
        return;
      }

      try {
        const payload: RefreshTokenRequest = { refreshToken };
        const response = await apiClient.post<AuthResponse>('/identity/refresh-token', payload);
        login(response.data);
      } catch (error) {
        console.error('Session restoration failed:', error);
        logout(); // Clear invalid token
      } finally {
        setIsBootstrapping(false);
      }
    };

    bootstrapSession();
  }, [login, logout]);

  if (isBootstrapping) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', backgroundColor: '#f9fafb' }}>
        <p style={{ color: '#6b7280' }}>Loading session...</p>
      </div>
    );
  }

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<Layout />}>
            <Route index element={<InboxPage />} />
            <Route path="emails/:id" element={<EmailDetailPage />} />
            <Route path="search" element={<SearchPage />} />
            <Route path="rules" element={<AutomationPage />} />
            <Route path="analytics" element={<AnalyticsPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
};

export default App;
