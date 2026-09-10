import { create } from 'zustand';
import type { AuthResponse } from '../types/auth';

interface AuthState {
  accessToken: string | null;
  workspaceId: string; // The selected workspace context
  isAuthenticated: boolean;
  login: (authData: AuthResponse) => void;
  logout: () => void;
  setAccessToken: (token: string) => void;
  setWorkspaceId: (workspaceId: string) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  workspaceId: '00000000-0000-0000-0000-000000000001', // Fallback/Default for MVP
  isAuthenticated: false,
  login: (authData: AuthResponse) => {
    // Store refresh token in sessionStorage
    sessionStorage.setItem('nexusmail_refresh_token', authData.refreshToken);
    
    // Store access token in memory (Zustand)
    set({
      accessToken: authData.accessToken,
      // In future, this should be set from a /workspaces API call after login
      workspaceId: '00000000-0000-0000-0000-000000000001', 
      isAuthenticated: true
    });
  },
  logout: () => {
    // Clear session storage
    sessionStorage.removeItem('nexusmail_refresh_token');
    
    // Clear memory state
    set({
      accessToken: null,
      isAuthenticated: false
    });
  },
  setAccessToken: (token: string) => {
    set({
      accessToken: token,
      isAuthenticated: true
    });
  },
  setWorkspaceId: (workspaceId: string) => {
    set({ workspaceId });
  }
}));

export const getRefreshToken = (): string | null => {
  return sessionStorage.getItem('nexusmail_refresh_token');
};

