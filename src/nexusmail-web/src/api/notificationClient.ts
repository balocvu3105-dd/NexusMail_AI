import { apiClient } from './client';
import type { NotificationDto } from '../types/notification';

export const notificationApi = {
  getNotifications: async (): Promise<NotificationDto[]> => {
    const response = await apiClient.get<NotificationDto[]>('/notifications');
    return response.data;
  },

  markAsRead: async (id: string): Promise<void> => {
    await apiClient.patch(`/notifications/${id}/read`);
  },

  markAllAsRead: async (): Promise<void> => {
    await apiClient.post('/notifications/mark-all-read');
  },
};
