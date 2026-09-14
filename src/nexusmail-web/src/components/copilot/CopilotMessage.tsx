import React from 'react';
import DOMPurify from 'dompurify';
import styles from './CopilotMessage.module.css';
import type { CopilotMessage as CopilotMessageType } from '../../stores/copilotStore';
import { CopilotActionCard } from './CopilotActionCard';

interface Props {
  message: CopilotMessageType;
}

export const CopilotMessage: React.FC<Props> = ({ message }) => {
  const isAssistant = message.role === 'assistant';

  // Basic markdown to HTML (just line breaks for now since we don't have a full md parser)
  // Dompurify keeps it safe.
  const createMarkup = (html: string) => {
    const formatted = html.replace(/\n/g, '<br/>');
    return { __html: DOMPurify.sanitize(formatted) };
  };

  return (
    <div className={`${styles.messageWrapper} ${isAssistant ? styles.assistant : styles.user}`}>
      <div className={styles.messageBubble}>
        <div className={styles.text} dangerouslySetInnerHTML={createMarkup(message.text)} />
        {message.isStreaming && <span className={styles.cursor}></span>}
      </div>

      {isAssistant && !message.isStreaming && message.state && (
        <div className={styles.footer}>
          {message.state === 'InsufficientEvidence' && (
            <div className={styles.warning}>
              ⚠️ Không tìm thấy đủ dữ liệu để trả lời chắc chắn.
            </div>
          )}

          {message.state === 'Grounded' && message.evidence && message.evidence.length > 0 && (
            <div className={styles.evidenceContainer}>
              <span className={styles.evidenceTitle}>Sources:</span>
              <div className={styles.evidenceList}>
                {message.evidence
                  .filter((ev) => !!ev.subject)
                  .map((ev, i) => (
                    <span key={i} className={styles.evidenceTag}>
                      {ev.subject}
                    </span>
                  ))}
              </div>
            </div>
          )}

          {message.suggestedActions && message.suggestedActions.length > 0 && (
            <div className={styles.actionsContainer}>
              <span className={styles.actionsTitle}>Suggested Actions:</span>
              <div className={styles.actionsList}>
                {message.suggestedActions.map((action, i) => (
                  <CopilotActionCard key={i} action={action} />
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
