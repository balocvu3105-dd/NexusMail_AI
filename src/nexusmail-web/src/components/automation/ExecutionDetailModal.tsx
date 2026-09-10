import React, { useEffect, useState } from 'react';
import { apiClient } from '../../api/client';
import type { AutomationExecution } from './ExecutionHistory';
import styles from './ExecutionDetailModal.module.css';

interface ExecutionDetailModalProps {
  executionId: string;
  onClose: () => void;
}

export const ExecutionDetailModal: React.FC<ExecutionDetailModalProps> = ({ executionId, onClose }) => {
  const [execution, setExecution] = useState<AutomationExecution | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDetail = async () => {
      try {
        const res = await apiClient.get<AutomationExecution>(`/automation/executions/${executionId}`);
        setExecution(res.data);
      } catch (err) {
        console.error('Failed to fetch execution detail', err);
      } finally {
        setLoading(false);
      }
    };
    fetchDetail();
  }, [executionId]);

  return (
    <div className={styles.overlay}>
      <div className={styles.modal}>
        <div className={styles.header}>
          <h2>Execution Details</h2>
          <button onClick={onClose} className={styles.closeButton}>&times;</button>
        </div>
        
        <div className={styles.body}>
          {loading ? (
            <p>Loading...</p>
          ) : execution ? (
            <div className={styles.detailsList}>
              <div className={styles.detailRow}>
                <span className={styles.label}>Execution ID</span>
                <span className={styles.value}>{execution.id}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Rule ID</span>
                <span className={styles.value}>{execution.ruleId}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Date</span>
                <span className={styles.value}>{new Date(execution.executedAt).toLocaleString()}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Trigger Type</span>
                <span className={styles.value}>{execution.triggerType}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Duration</span>
                <span className={styles.value}>{execution.executionDuration}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Status</span>
                <span className={`${styles.status} ${styles[execution.status.toLowerCase()] || styles.unknown}`}>
                  {execution.status}
                </span>
              </div>

              {execution.status.toLowerCase() === 'unknown' && (
                <div className={styles.warningBox}>
                  ⚠️ Outcome Unknown — manual investigation required
                </div>
              )}
            </div>
          ) : (
            <p>Execution not found.</p>
          )}
        </div>

        <div className={styles.footer}>
          <button onClick={onClose} className={styles.closeBtn}>Close</button>
        </div>
      </div>
    </div>
  );
};
