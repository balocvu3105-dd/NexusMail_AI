import React, { useState, useEffect } from 'react';
import { apiClient } from '../api/client';
import { RuleEditor } from '../components/automation/RuleEditor';
import { ExecutionHistory } from '../components/automation/ExecutionHistory';
import styles from './AutomationPage.module.css';

export interface AutomationRule {
  id: string;
  name: string;
  description: string;
  isEnabled: boolean;
  triggerType: string;
  conditionsJson: string;
  actionsJson: string;
}

export const AutomationPage: React.FC = () => {
  const [rules, setRules] = useState<AutomationRule[]>([]);
  const [isEditorOpen, setIsEditorOpen] = useState(false);
  const [editingRule, setEditingRule] = useState<AutomationRule | null>(null);

  const fetchRules = async () => {
    try {
      const res = await apiClient.get<AutomationRule[]>('/automation/rules');
      setRules(res.data);
    } catch (err) {
      console.error('Failed to fetch rules', err);
    }
  };

  useEffect(() => {
    fetchRules();
  }, []);

  const handleCreateNew = () => {
    setEditingRule(null);
    setIsEditorOpen(true);
  };

  const handleEdit = (rule: AutomationRule) => {
    setEditingRule(rule);
    setIsEditorOpen(true);
  };

  const handleToggle = async (rule: AutomationRule) => {
    try {
      if (rule.isEnabled) {
        await apiClient.post(`/automation/rules/${rule.id}/disable`);
      } else {
        await apiClient.post(`/automation/rules/${rule.id}/enable`);
      }
      fetchRules();
    } catch (err) {
      console.error('Failed to toggle rule', err);
    }
  };

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <h1 className={styles.title}>Automation Rules</h1>
          <p className={styles.subtitle}>Manage your email automation rules</p>
        </div>
        <button onClick={handleCreateNew} className={styles.createButton}>
          + Create Rule
        </button>
      </header>

      <div className={styles.content}>
        <div className={styles.rulesList}>
          {rules.length === 0 ? (
            <div className={styles.emptyState}>No automation rules found.</div>
          ) : (
            rules.map((rule) => (
              <div key={rule.id} className={styles.ruleCard}>
                <div className={styles.ruleHeader}>
                  <h3 className={styles.ruleName}>{rule.name}</h3>
                  <div className={styles.ruleActions}>
                    <button onClick={() => handleToggle(rule)} className={rule.isEnabled ? styles.enabled : styles.disabled}>
                      {rule.isEnabled ? 'Enabled' : 'Disabled'}
                    </button>
                    <button onClick={() => handleEdit(rule)} className={styles.editButton}>Edit</button>
                  </div>
                </div>
                <p className={styles.ruleDescription}>{rule.description || 'No description'}</p>
              </div>
            ))
          )}
        </div>

        <div className={styles.historySection}>
          <h2 className={styles.sectionTitle}>Recent Executions</h2>
          <ExecutionHistory />
        </div>
      </div>

      {isEditorOpen && (
        <RuleEditor
          rule={editingRule}
          onClose={() => setIsEditorOpen(false)}
          onSave={() => {
            setIsEditorOpen(false);
            fetchRules();
          }}
        />
      )}
    </div>
  );
};
