import React, { useEffect, useState } from 'react';
import { apiClient } from '../../api/client';
import { ExecutionDetailModal } from './ExecutionDetailModal';
import styles from './ExecutionHistory.module.css';

export interface AutomationExecution {
  id: string;
  ruleId: string;
  triggerType: string;
  wasSuccessful: boolean;
  executedAt: string;
  executionDuration: string;
  status: string;
}

export const ExecutionHistory: React.FC = () => {
  const [executions, setExecutions] = useState<AutomationExecution[]>([]);
  const [selectedExecution, setSelectedExecution] = useState<AutomationExecution | null>(null);

  useEffect(() => {
    const fetchExecutions = async () => {
      try {
        const res = await apiClient.get<AutomationExecution[]>('/automation/executions');
        setExecutions(res.data);
      } catch (err) {
        console.error('Failed to fetch executions', err);
      }
    };
    fetchExecutions();
  }, []);

  return (
    <div>
      {executions.length === 0 ? (
        <div className={styles.emptyState}>No executions yet.</div>
      ) : (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Date</th>
              <th>Trigger</th>
              <th>Status</th>
              <th>Duration</th>
            </tr>
          </thead>
          <tbody>
            {executions.map(exec => (
              <tr key={exec.id} onClick={() => setSelectedExecution(exec)} className={styles.row}>
                <td>{new Date(exec.executedAt).toLocaleString()}</td>
                <td>{exec.triggerType}</td>
                <td>
                  <span className={`${styles.status} ${styles[exec.status.toLowerCase()] || styles.unknown}`}>
                    {exec.status}
                  </span>
                </td>
                <td>{exec.executionDuration}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {selectedExecution && (
        <ExecutionDetailModal
          executionId={selectedExecution.id}
          onClose={() => setSelectedExecution(null)}
        />
      )}
    </div>
  );
};
