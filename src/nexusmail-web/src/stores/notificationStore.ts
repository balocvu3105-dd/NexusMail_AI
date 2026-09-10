import { create } from 'zustand';
import type { NotificationDto } from '../types/notification';
import { notificationApi } from '../api/notificationClient';
import { notificationHub } from '../api/notificationHub';

interface NotificationState {
  notifications: NotificationDto[];
  unreadCount: number;
  isConnected: boolean;
  isConnecting: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  fetchNotifications: () => Promise<void>;
  markAsRead: (id: string) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  handleRealtimeNotification: (notification: NotificationDto) => void;
  connectSignalR: (accessToken: string, workspaceId: string) => Promise<void>;
  disconnectSignalR: () => Promise<void>;
}

export const useNotificationStore = create<NotificationState>((set, get) => ({
  notifications: [],
  unreadCount: 0,
  isConnected: false,
  isConnecting: false,
  isLoading: false,
  error: null,

  fetchNotifications: async () => {
    set({ isLoading: true, error: null });
    try {
      const data = await notificationApi.getNotifications();
      const unreadCount = data.filter((n) => !n.isRead).length;
      set({ notifications: data, unreadCount, isLoading: false });
    } catch (err: any) {
      set({ error: err.message || 'Failed to fetch notifications', isLoading: false });
    }
  },

  markAsRead: async (id: string) => {
    const { notifications } = get();
    const target = notifications.find(n => n.id === id);
    if (!target || target.isRead) return;

    // Optimistic UI update
    set((state) => ({
      notifications: state.notifications.map((n) =>
        n.id === id ? { ...n, isRead: true } : n
      ),
      unreadCount: Math.max(0, state.unreadCount - 1),
    }));

    try {
      await notificationApi.markAsRead(id);
    } catch (err) {
      // Revert on failure
      console.error('Failed to mark notification as read:', err);
      set((state) => ({
        notifications: state.notifications.map((n) =>
          n.id === id ? { ...n, isRead: false } : n
        ),
        unreadCount: state.unreadCount + 1,
      }));
    }
  },

  markAllAsRead: async () => {
    const currentNotifications = get().notifications;
    const unreadCount = currentNotifications.filter(n => !n.isRead).length;
    if (unreadCount === 0) return;

    // Optimistic
    set({
      notifications: currentNotifications.map(n => ({ ...n, isRead: true })),
      unreadCount: 0
    });

    try {
      await notificationApi.markAllAsRead();
    } catch (err) {
      // Revert (fetch from server to be safe)
      console.error('Failed to mark all as read:', err);
      get().fetchNotifications();
    }
  },

  handleRealtimeNotification: (notification: NotificationDto) => {
    set((state) => {
      // Deduplicate by sourceEventId
      if (state.notifications.some((n) => n.sourceEventId === notification.sourceEventId)) {
        return state;
      }
      
      const newNotifications = [notification, ...state.notifications];
      const newUnreadCount = state.unreadCount + (notification.isRead ? 0 : 1);
      
      return {
        notifications: newNotifications,
        unreadCount: newUnreadCount,
      };
    });
  },

  connectSignalR: async (accessToken: string, workspaceId: string) => {
    const state = get();
    if (state.isConnected || state.isConnecting) return;

    set({ isConnecting: true });
    try {
      await notificationHub.connect(accessToken, workspaceId, {
        onReceiveNotification: (notification: NotificationDto) => {
          get().handleRealtimeNotification(notification);
        },
        onReconnecting: () => {
          set({ isConnected: false });
        },
        onReconnected: () => {
          set({ isConnected: true });
          // Fetch catch-up
          get().fetchNotifications();
        },
        onClose: () => {
          set({ isConnected: false });
        },
      });
      set({ isConnected: true, isConnecting: false });
    } catch (err) {
      console.error('SignalR connect error:', err);
      set({ isConnected: false, isConnecting: false });
    }
  },

  disconnectSignalR: async () => {
    set({ isConnecting: false });
    await notificationHub.disconnect();
    set({ isConnected: false, notifications: [], unreadCount: 0 });
  },
}));
