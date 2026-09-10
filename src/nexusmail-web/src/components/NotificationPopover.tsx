import React from 'react';
import { useNotificationStore } from '../stores/notificationStore';
import { NotificationItem } from './NotificationItem';
import styles from './Notification.module.css';

interface NotificationPopoverProps {
  onClose: () => void;
}

export const NotificationPopover: React.FC<NotificationPopoverProps> = () => {
  const { 
    notifications, 
    isLoading, 
    error, 
    fetchNotifications, 
    markAsRead, 
    markAllAsRead 
  } = useNotificationStore();

  const handleMarkAsRead = (id: string) => {
    markAsRead(id);
  };

  return (
    <div className={styles.popover}>
      <div className={styles.header}>
        <h3 className={styles.headerTitle}>Notifications</h3>
        {notifications.some(n => !n.isRead) && (
          <button className={styles.markAllBtn} onClick={markAllAsRead}>
            Mark all as read
          </button>
        )}
      </div>

      <div className={styles.content}>
        {isLoading && notifications.length === 0 ? (
          <div className={styles.loadingState}>
            Loading notifications...
          </div>
        ) : error ? (
          <div className={styles.errorState}>
            <p>{error}</p>
            <button className={styles.retryBtn} onClick={fetchNotifications}>
              Retry
            </button>
          </div>
        ) : notifications.length === 0 ? (
          <div className={styles.emptyState}>
            <p>You have no notifications</p>
          </div>
        ) : (
          notifications.map((notification) => (
            <NotificationItem 
              key={notification.id} 
              notification={notification} 
              onClick={handleMarkAsRead} 
            />
          ))
        )}
      </div>
    </div>
  );
};
