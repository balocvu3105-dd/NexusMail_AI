import React from 'react';
import type { NotificationDto } from '../types/notification';
import styles from './Notification.module.css';

interface NotificationItemProps {
  notification: NotificationDto;
  onClick: (id: string) => void;
}

export const NotificationItem: React.FC<NotificationItemProps> = ({ notification, onClick }) => {
  const handleClick = () => {
    if (!notification.isRead) {
      onClick(notification.id);
    }
  };

  // Format date simply
  const date = new Date(notification.createdAtUtc);
  const timeString = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  const dateString = date.toLocaleDateString();

  return (
    <div 
      className={`${styles.item} ${!notification.isRead ? styles.unread : ''}`}
      onClick={handleClick}
    >
      {!notification.isRead ? (
        <div className={styles.itemDot} />
      ) : (
        <div style={{ width: 8, flexShrink: 0 }} /> // Placeholder for alignment
      )}
      <div className={styles.itemContent}>
        <h4 className={styles.itemTitle}>{notification.title}</h4>
        <p className={styles.itemMessage}>{notification.message}</p>
        <p className={styles.itemTime}>{dateString} at {timeString}</p>
      </div>
    </div>
  );
};
