import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

const API_BASE_URL = 'http://localhost:5038/api/v1'; // Adjust to actual backend URL when known

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
    // X-Workspace-Id will be injected by the interceptor
  }
});

// Interceptor #1: Attach Authorization: Bearer <accessToken>
apiClient.interceptors.request.use(
  (config) => {
    const { accessToken, workspaceId } = useAuthStore.getState();
    if (config.headers) {
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`;
      }
      if (workspaceId) {
        config.headers['X-Workspace-Id'] = workspaceId;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor #2: 401 -> Logout
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Clear session
      useAuthStore.getState().logout();
      // Redirect to login handled by ProtectedRoute or router, 
      // but we can also force it here if strictly needed. 
      // It's usually better to let React Router handle it via auth state changes.
    }
    return Promise.reject(error);
  }
);
