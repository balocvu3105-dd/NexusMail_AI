import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import DOMPurify from 'dompurify';
import { apiClient } from '../api/client';
import type { EmailDetail } from '../types/email';
import { ArrowLeft, Sparkles, AlertCircle, Clock, Tag } from 'lucide-react';
import styles from './EmailDetailPage.module.css';

export const EmailDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [email, setEmail] = useState<EmailDetail | null>(null);
  const [status, setStatus] = useState<'loading' | 'success' | 'error' | 'not-found'>('loading');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    const fetchEmailDetail = async () => {
      try {
        setStatus('loading');
        setErrorMessage(null);
        
        const response = await apiClient.get<EmailDetail>(`/emails/${id}`);
        setEmail(response.data);
        setStatus('success');
      } catch (err: any) {
        console.error('Failed to fetch email details', err);
        
        if (err.response?.status === 404) {
          setStatus('not-found');
        } else {
          setStatus('error');
          setErrorMessage(err.response?.data?.message || 'An unexpected error occurred while loading the email.');
        }
      }
    };

    if (id) {
      fetchEmailDetail();
    }
  }, [id]);

  if (status === 'loading') {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.spinner}></div>
        <p>Loading email details...</p>
      </div>
    );
  }

  if (status === 'not-found') {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.emptyIcon}>404</div>
        <h2 className={styles.errorTitle}>Email Not Found</h2>
        <p className={styles.errorMessage}>The email you are looking for does not exist or has been removed.</p>
        <button className={styles.backBtn} onClick={() => navigate('/')}>
          <ArrowLeft size={16} /> Back to Inbox
        </button>
      </div>
    );
  }

  if (status === 'error') {
    return (
      <div className={styles.centerContainer}>
        <AlertCircle size={48} className={styles.errorIcon} />
        <h2 className={styles.errorTitle}>Error Loading Email</h2>
        <p className={styles.errorMessage}>{errorMessage}</p>
        <div className={styles.buttonGroup}>
          <button className={styles.retryBtn} onClick={() => window.location.reload()}>Retry</button>
          <button className={styles.backBtn} onClick={() => navigate('/')}>
            <ArrowLeft size={16} /> Back to Inbox
          </button>
        </div>
      </div>
    );
  }

  if (!email) return null;

  const getPriorityBadgeClass = (priority: string | null) => {
    if (!priority) return styles.badgeDefault;
    const lower = priority.toLowerCase();
    if (lower === 'high') return styles.badgeHigh;
    if (lower === 'medium') return styles.badgeMedium;
    return styles.badgeLow;
  };

  const hasAIAnalysis = email.summary !== null || email.category !== null || email.priority !== null || email.tags !== null;

  // Render HTML safely or fallback to text
  const cleanHtml = DOMPurify.sanitize(email.content, { USE_PROFILES: { html: true } });

  return (
    <div className={styles.detailContainer}>
      <header className={styles.header}>
        <button className={styles.backNavBtn} onClick={() => navigate('/')}>
          <ArrowLeft size={18} /> Back to Inbox
        </button>
      </header>

      <div className={styles.contentLayout}>
        <div className={styles.mainContent}>
          <div className={styles.emailHeader}>
            <h1 className={styles.subject}>{email.subject}</h1>
            <div className={styles.metadata}>
              <div className={styles.senderInfo}>
                <div className={styles.avatar}>{email.sender.charAt(0).toUpperCase()}</div>
                <span className={styles.sender}>{email.sender}</span>
              </div>
              <div className={styles.timeInfo}>
                <Clock size={14} />
                <span>{new Date(email.receivedAt).toLocaleString()}</span>
              </div>
            </div>
          </div>
          
          <div className={styles.emailBody}>
            {/* Using safely sanitized HTML if it looks like HTML, otherwise just text */}
            {cleanHtml !== email.content && email.content.includes('<') && email.content.includes('>') ? (
              <div dangerouslySetInnerHTML={{ __html: cleanHtml }} />
            ) : (
              <div style={{ whiteSpace: 'pre-wrap' }}>{email.content}</div>
            )}
          </div>
        </div>

        <aside className={styles.aiPanel}>
          <div className={styles.aiHeader}>
            <Sparkles size={18} className={styles.aiIcon} />
            <h2>AI Intelligence Layer</h2>
          </div>
          
          <div className={styles.aiContent}>
            {hasAIAnalysis ? (
              <>
                {email.priority && (
                  <div className={styles.aiSection}>
                    <h3>Priority</h3>
                    <div className={`${styles.priorityBadge} ${getPriorityBadgeClass(email.priority)}`}>
                      {email.priority} {email.priorityScore !== null ? `(${email.priorityScore}/10)` : ''}
                    </div>
                  </div>
                )}
                
                {email.summary && (
                  <div className={styles.aiSection}>
                    <h3>Summary</h3>
                    <p className={styles.summaryText}>{email.summary}</p>
                  </div>
                )}

                {email.category && (
                  <div className={styles.aiSection}>
                    <h3>Category</h3>
                    <span className={styles.categoryBadge}>{email.category}</span>
                  </div>
                )}

                {email.tags && email.tags.length > 0 && (
                  <div className={styles.aiSection}>
                    <h3>Tags</h3>
                    <div className={styles.tagsContainer}>
                      {email.tags.map(tag => (
                        <span key={tag} className={styles.tagBadge}>
                          <Tag size={12} /> {tag}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
                
                {email.language && (
                  <div className={styles.aiSection}>
                    <h3>Language</h3>
                    <span className={styles.languageText}>{email.language}</span>
                  </div>
                )}
              </>
            ) : (
              <div className={styles.aiEmptyState}>
                <div className={styles.aiEmptyIcon}>
                  <Sparkles size={32} />
                </div>
                <p>Analysis is not available yet.</p>
                <span className={styles.aiEmptySubtext}>
                  The AI processing might be queued or unavailable for this email.
                </span>
              </div>
            )}
          </div>
        </aside>
      </div>
    </div>
  );
};
