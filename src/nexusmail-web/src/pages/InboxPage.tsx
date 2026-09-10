import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { InboxMessage } from '../types/email';
import { apiClient } from '../api/client';
import { Sparkles, Archive, Zap, AlertCircle } from 'lucide-react';
import styles from './InboxPage.module.css';

export const InboxPage: React.FC = () => {
  const navigate = useNavigate();
  const [emails, setEmails] = useState<InboxMessage[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchEmails = async () => {
      try {
        setIsLoading(true);
        setError(null);
        // Using real endpoint
        const response = await apiClient.get<InboxMessage[]>('/emails/inbox');
        setEmails(response.data);
      } catch (err: any) {
        console.error('Failed to fetch inbox', err);
        setError(err.response?.data?.message || 'Failed to load inbox. Please try again later.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchEmails();
  }, []);

  const getPriorityBadgeClass = (priority: string | null) => {
    if (!priority) return styles.badgeDefault || styles.badgeLow;
    const lower = priority.toLowerCase();
    if (lower === 'high') return styles.badgeHigh;
    if (lower === 'medium') return styles.badgeMedium;
    return styles.badgeLow;
  };

  const getPriorityLabel = (priority: string | null) => {
    if (!priority) return 'LOW';
    return priority.toUpperCase();
  };

  if (isLoading) {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.spinner}></div>
        <p>Loading your action center...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.centerContainer}>
        <AlertCircle size={48} className={styles.errorIcon} />
        <h2 className={styles.errorTitle}>Error Loading Inbox</h2>
        <p className={styles.errorMessage}>{error}</p>
        <button className={styles.retryBtn} onClick={() => window.location.reload()}>Retry</button>
      </div>
    );
  }

  if (emails.length === 0) {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.emptyIcon}>Inbox Zero</div>
        <p className={styles.emptyMessage}>You have no new emails to process.</p>
      </div>
    );
  }

  return (
    <div className={styles.inboxContainer}>
      <header className={styles.header}>
        <h1 className={styles.title}>Inbox</h1>
        <div className={styles.filters}>
          <span className={`${styles.filterTab} ${styles.active}`}>All</span>
          <span className={styles.filterTab}>Important</span>
          <span className={styles.filterTab}>Needs Action</span>
        </div>
      </header>

      <div className={styles.emailList}>
        {emails.map(email => (
          <div 
            key={email.id} 
            className={styles.emailCard} 
            onClick={() => navigate(`/emails/${email.id}`)}
            style={{ cursor: 'pointer' }}
          >
            <div className={styles.cardHeader}>
              <span className={styles.sender}>{email.sender}</span>
              {email.priorityScore > 0 && (
                <span className={`${styles.badge} ${getPriorityBadgeClass(email.priority)}`}>
                  {getPriorityLabel(email.priority)}
                </span>
              )}
            </div>
            
            <h3 className={styles.subject}>{email.subject}</h3>
            
            {/* AI Summary Section - First Class Information */}
            <div className={styles.aiSection}>
              <div className={styles.aiHeader}>
                <Sparkles size={14} className={styles.aiIcon} />
                <span>AI SUMMARY</span>
              </div>
              {email.summary ? (
                <p className={styles.aiSummary}>{email.summary}</p>
              ) : (
                <p className={styles.aiProcessing}>AI Analysis is currently processing or unavailable.</p>
              )}
              
              {(email.category || email.priority) && (
                <div className={styles.aiTags}>
                  {email.category && <span className={styles.tag}>{email.category}</span>}
                </div>
              )}
            </div>

            <div className={styles.cardFooter}>
              <div className={styles.actions}>
                <button 
                  className={`${styles.actionBtn} ${styles.primary}`}
                  onClick={(e) => e.stopPropagation()}
                >
                  Reply <Sparkles size={14} />
                </button>
                {/* Disabled as endpoint isn't wired yet */}
                <button 
                  className={styles.actionBtn} 
                  disabled 
                  title="Archive feature not yet implemented"
                  onClick={(e) => e.stopPropagation()}
                >
                  <Archive size={14} /> Archive
                </button>
                <button 
                  className={styles.actionBtn} 
                  disabled 
                  title="Automation not yet configured"
                  onClick={(e) => e.stopPropagation()}
                >
                  <Zap size={14} /> Automate
                </button>
              </div>
              <span className={styles.time}>
                {new Date(email.receivedAt).toLocaleDateString()}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
