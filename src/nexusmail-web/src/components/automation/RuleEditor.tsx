import React, { useState } from 'react';
import type { AutomationRule } from '../../pages/AutomationPage';
import { apiClient } from '../../api/client';
import styles from './RuleEditor.module.css';

interface RuleEditorProps {
  rule: AutomationRule | null;
  onClose: () => void;
  onSave: () => void;
}

export const RuleEditor: React.FC<RuleEditorProps> = ({ rule, onClose, onSave }) => {
  const [name, setName] = useState(rule?.name || '');
  const [description, setDescription] = useState(rule?.description || '');
  const [triggerType, setTriggerType] = useState(rule?.triggerType || 'EmailReceived');
  const [conditionsJson, setConditionsJson] = useState(rule?.conditionsJson || '[]');
  const [actionsJson, setActionsJson] = useState(rule?.actionsJson || '[]');
  const [error, setError] = useState<string | null>(null);

  const handleSave = async () => {
    try {
      setError(null);
      // Basic JSON validation
      JSON.parse(conditionsJson);
      JSON.parse(actionsJson);

      const payload = {
        name,
        description,
        triggerType,
        conditionsJson,
        actionsJson,
        dryRun: false
      };

      if (rule) {
        await apiClient.put(`/automation/rules/${rule.id}`, payload);
      } else {
        await apiClient.post('/automation/rules', payload);
      }
      onSave();
    } catch (err: any) {
      setError(err.message || 'Invalid JSON format or server error');
    }
  };

  return (
    <div className={styles.overlay}>
      <div className={styles.modal}>
        <div className={styles.header}>
          <h2>{rule ? 'Edit Rule' : 'Create New Rule'}</h2>
          <button onClick={onClose} className={styles.closeButton}>&times;</button>
        </div>
        
        <div className={styles.body}>
          {error && <div className={styles.error}>{error}</div>}
          
          <div className={styles.formGroup}>
            <label>Name</label>
            <input 
              value={name} 
              onChange={e => setName(e.target.value)} 
              placeholder="e.g. Forward Invoices"
              className={styles.input}
            />
          </div>

          <div className={styles.formGroup}>
            <label>Description</label>
            <input 
              value={description} 
              onChange={e => setDescription(e.target.value)} 
              placeholder="Optional description"
              className={styles.input}
            />
          </div>

          {!rule && (
            <div className={styles.formGroup}>
              <label>Trigger Type</label>
              <select 
                value={triggerType} 
                onChange={e => setTriggerType(e.target.value)}
                className={styles.select}
              >
                <option value="EmailReceived">EmailReceived</option>
                <option value="Manual">Manual</option>
              </select>
            </div>
          )}

          <div className={styles.formGroup}>
            <label>Conditions (JSON)</label>
            <textarea 
              value={conditionsJson} 
              onChange={e => setConditionsJson(e.target.value)} 
              className={styles.textarea}
              rows={4}
            />
          </div>

          <div className={styles.formGroup}>
            <label>Actions (JSON)</label>
            <textarea 
              value={actionsJson} 
              onChange={e => setActionsJson(e.target.value)} 
              className={styles.textarea}
              rows={6}
            />
          </div>
        </div>

        <div className={styles.footer}>
          <button onClick={onClose} className={styles.cancelButton}>Cancel</button>
          <button onClick={handleSave} className={styles.saveButton}>Save</button>
        </div>
      </div>
    </div>
  );
};
