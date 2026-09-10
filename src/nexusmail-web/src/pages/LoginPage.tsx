import React, { useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { Mail, Lock, AlertCircle, Loader2 } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';
import { apiClient } from '../api/client';
import type { LoginRequest, AuthResponse } from '../types/auth';
import styles from './LoginPage.module.css';

export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated, login } = useAuthStore();
  
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // If already authenticated, redirect to home
  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      setError('Please enter both email and password.');
      return;
    }

    try {
      setIsLoading(true);
      setError(null);

      const payload: LoginRequest = {
        email,
        password,
        deviceName: navigator.userAgent,
      };

      const response = await apiClient.post<AuthResponse>('/identity/login', payload);
      
      // Store tokens and set authenticated state
      login(response.data);
      
      // Navigate to application
      navigate('/');
    } catch (err: any) {
      console.error('Login failed', err);
      setError(err.response?.data?.message || err.response?.data?.error?.message || 'Invalid email or password.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.loginBox}>
        <div className={styles.header}>
          <div className={styles.brandIconWrapper}>
            <Mail size={32} className={styles.brandIcon} />
          </div>
          <h1 className={styles.title}>NexusMail AI</h1>
          <p className={styles.subtitle}>Sign in to your intelligent workspace</p>
        </div>

        <form onSubmit={handleSubmit} className={styles.form}>
          {error && (
            <div className={styles.errorAlert}>
              <AlertCircle size={16} />
              <span>{error}</span>
            </div>
          )}

          <div className={styles.inputGroup}>
            <label htmlFor="email">Email address</label>
            <div className={styles.inputWrapper}>
              <Mail size={18} className={styles.inputIcon} />
              <input
                id="email"
                type="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>
          </div>

          <div className={styles.inputGroup}>
            <label htmlFor="password">Password</label>
            <div className={styles.inputWrapper}>
              <Lock size={18} className={styles.inputIcon} />
              <input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>
          </div>

          <button 
            type="submit" 
            className={styles.submitBtn}
            disabled={isLoading}
          >
            {isLoading ? (
              <>
                <Loader2 size={18} className={styles.spinner} />
                Signing in...
              </>
            ) : (
              'Sign in'
            )}
          </button>
        </form>
      </div>
    </div>
  );
};
