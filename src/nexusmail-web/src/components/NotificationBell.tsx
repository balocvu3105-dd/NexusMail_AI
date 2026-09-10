import React, { useState, useRef, useEffect } from 'react';
import { Bell } from 'lucide-react';
import { useNotificationStore } from '../stores/notificationStore';
import { NotificationPopover } from './NotificationPopover';
import styles from './Notification.module.css';

export const NotificationBell: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const { unreadCount, isConnected } = useNotificationStore();

  const toggleOpen = () => {
    setIsOpen(!isOpen);
  };

  const closePopover = () => {
    setIsOpen(false);
  };

  // Close when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  return (
    <div className={styles.bellContainer} ref={containerRef}>
      <button 
        className={`${styles.bellButton} ${isOpen ? styles.active : ''}`} 
        onClick={toggleOpen}
        title={isConnected ? "Notifications connected" : "Notifications reconnecting..."}
      >
        <Bell size={20} />
        {unreadCount > 0 && (
          <div className={styles.badge}>
            {unreadCount > 99 ? '99+' : unreadCount}
          </div>
        )}
      </button>

      {isOpen && <NotificationPopover onClose={closePopover} />}
    </div>
  );
};
