import React, { useState } from 'react';
import { v4 as uuidv4 } from 'uuid';
import styles from './CopilotActionCard.module.css';
import { executeAction } from '../../api/copilotClient';
import type { ActionProposal } from '../../api/copilotClient';

interface Props {
  action: ActionProposal;
}

export const CopilotActionCard: React.FC<Props> = ({ action }) => {
  const [status, setStatus] = useState<'pending' | 'executing' | 'success' | 'error'>('pending');
  const [errorMsg, setErrorMsg] = useState<string>('');

  const handleApprove = async () => {
    setStatus('executing');
    setErrorMsg('');
    try {
      const idempotencyKey = uuidv4();
      await executeAction(idempotencyKey, action);
      setStatus('success');
    } catch (err: any) {
      setStatus('error');
      setErrorMsg(err.message || 'Execution failed');
    }
  };

  const isCreateTask = action.actionType === 'CreateTask';
  const isApplyLabel = action.actionType === 'ApplyLabel';

  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <span className={styles.icon}>
          {isCreateTask ? '📝' : isApplyLabel ? '🏷️' : '⚡'}
        </span>
        <span className={styles.title}>
          {isCreateTask ? 'Create Task' : isApplyLabel ? 'Apply Label' : action.actionType}
        </span>
      </div>
      
      <div className={styles.body}>
        {isCreateTask && (
          <div className={styles.details}>
            <div><strong>Title:</strong> {action.parameters?.title}</div>
            {action.parameters?.description && <div><strong>Desc:</strong> {action.parameters.description}</div>}
            {action.parameters?.provider && <div><strong>To:</strong> {action.parameters.provider}</div>}
          </div>
        )}
        
        {isApplyLabel && (
          <div className={styles.details}>
            <div><strong>Label:</strong> {action.parameters?.label}</div>
            <div className={styles.muted}>Applies to the current context email.</div>
          </div>
        )}
        
        {!isCreateTask && !isApplyLabel && (
          <pre className={styles.rawParams}>
            {JSON.stringify(action.parameters, null, 2)}
          </pre>
        )}
      </div>

      <div className={styles.footer}>
        {status === 'pending' && (
          <button className={styles.approveBtn} onClick={handleApprove}>
            Approve & Run
          </button>
        )}
        {status === 'executing' && <span className={styles.statusExecuting}>Executing...</span>}
        {status === 'success' && <span className={styles.statusSuccess}>✅ Done</span>}
        {status === 'error' && (
          <div className={styles.errorContainer}>
            <span className={styles.statusError}>❌ {errorMsg}</span>
            <button className={styles.retryBtn} onClick={handleApprove}>Retry</button>
          </div>
        )}
      </div>
    </div>
  );
};
