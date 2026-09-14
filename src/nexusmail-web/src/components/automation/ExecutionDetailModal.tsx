import React, { useState, useEffect } from 'react';
import styles from './ExecutionDetailModal.module.css';
import { apiClient } from '../../api/client';

interface ActionExecutionDto {
  actionKey: string;
  actionType: string;
  status: string;
  createdAt: string;
  completedAt?: string;
  retryCount: number;
  errorMessage?: string;
}

interface AuditDto {
  executor: string;
  result: string;
  errorMessage?: string;
  executedAt: string;
}

export interface AutomationExecutionDetailDto {
  id: string;
  ruleId: string;
  emailId: string;
  status: string;
  createdAt: string;
  actionExecutions: ActionExecutionDto[];
  audits: AuditDto[];
}

interface ExecutionDetailModalProps {
  executionId: string;
  onClose: () => void;
}

export const ExecutionDetailModal: React.FC<ExecutionDetailModalProps> = ({ executionId, onClose }) => {
  const [detail, setDetail] = useState<AutomationExecutionDetailDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchDetail = async () => {
      setLoading(true);
      try {
        const response = await apiClient.get<AutomationExecutionDetailDto>(`/automation/executions/${executionId}`);
        setDetail(response.data);
      } catch (error) {
        console.error('Failed to fetch execution details', error);
      } finally {
        setLoading(false);
      }
    };
    fetchDetail();
  }, [executionId]);

  return (
    <div className={styles.overlay} onClick={onClose}>
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div className={styles.header}>
          <h2 className={styles.title}>Execution Details</h2>
          <button className={styles.closeBtn} onClick={onClose}>&times;</button>
        </div>

        {loading ? (
          <div className={styles.loading}>Loading details...</div>
        ) : detail ? (
          <div className={styles.content}>
            <div className={styles.section}>
              <h3 className={styles.sectionTitle}>Overview</h3>
              <div className={styles.infoGrid}>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Status</span>
                  <span className={`${styles.badge} ${styles[detail.status.toLowerCase()] || styles.default}`}>
                    {detail.status}
                  </span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Started At</span>
                  <span className={styles.infoValue}>{new Date(detail.createdAt).toLocaleString()}</span>
                </div>
              </div>
            </div>

            <div className={styles.section}>
              <h3 className={styles.sectionTitle}>Action Executions</h3>
              {detail.actionExecutions.length > 0 ? (
                <div className={styles.tableWrapper}>
                  <table className={styles.table}>
                    <thead>
                      <tr>
                        <th>Type</th>
                        <th>Status</th>
                        <th>Retries</th>
                        <th>Error</th>
                      </tr>
                    </thead>
                    <tbody>
                      {detail.actionExecutions.map((act, index) => (
                        <tr key={index}>
                          <td>{act.actionType}</td>
                          <td>
                            <span className={`${styles.badge} ${styles[act.status.toLowerCase()] || styles.default}`}>
                              {act.status}
                            </span>
                          </td>
                          <td>{act.retryCount}</td>
                          <td className={styles.errorText}>{act.errorMessage || '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className={styles.emptyState}>No actions found.</p>
              )}
            </div>

            <div className={styles.section}>
              <h3 className={styles.sectionTitle}>Audit Logs</h3>
              {detail.audits.length > 0 ? (
                <div className={styles.timeline}>
                  {detail.audits.map((audit, index) => (
                    <div key={index} className={styles.timelineItem}>
                      <div className={styles.timelineTime}>{new Date(audit.executedAt).toLocaleString()}</div>
                      <div className={styles.timelineContent}>
                        <strong>{audit.executor}</strong> returned <em>{audit.result}</em>
                        {audit.errorMessage && <div className={styles.errorDetail}>{audit.errorMessage}</div>}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className={styles.emptyState}>No audit logs found.</p>
              )}
            </div>
          </div>
        ) : (
          <div className={styles.errorState}>Failed to load details.</div>
        )}
      </div>
    </div>
  );
};
