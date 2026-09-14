import React, { useState, useEffect } from 'react';
import styles from './AnalyticsPage.module.css';
import { apiClient } from '../api/client';
import { ExecutionDetailModal } from '../components/automation/ExecutionDetailModal';

export interface OverviewMetrics {
  totalRules: number;
  activeRules: number;
  totalExecutions: number;
  succeededExecutions: number;
  failedExecutions: number;
  unknownExecutions: number;
  pendingExecutions: number;
  successRate: number;
}

export interface ExecutionAnalytics {
  id: string;
  ruleId: string;
  ruleName: string;
  emailId: string;
  status: string;
  createdAt: string;
}

interface PaginatedExecutions {
  items: ExecutionAnalytics[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const AnalyticsPage: React.FC = () => {
  const [metrics, setMetrics] = useState<OverviewMetrics | null>(null);
  const [executions, setExecutions] = useState<PaginatedExecutions | null>(null);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [selectedExecutionId, setSelectedExecutionId] = useState<string | null>(null);

  const fetchMetrics = async () => {
    try {
      const response = await apiClient.get<OverviewMetrics>('/automation/analytics/overview');
      setMetrics(response.data);
    } catch (error) {
      console.error('Failed to fetch analytics metrics', error);
    }
  };

  const fetchExecutions = async (pageIndex: number) => {
    try {
      const response = await apiClient.get<PaginatedExecutions>(`/automation/analytics/executions?page=${pageIndex}&pageSize=10`);
      setExecutions(response.data);
    } catch (error) {
      console.error('Failed to fetch executions', error);
    }
  };

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      await Promise.all([fetchMetrics(), fetchExecutions(page)]);
      setLoading(false);
    };
    loadData();
  }, [page]);

  if (loading && !metrics) {
    return <div className={styles.loading}>Loading analytics...</div>;
  }

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <h1 className={styles.title}>Automation Analytics</h1>
        <p className={styles.subtitle}>Monitor execution metrics and view history</p>
      </header>

      {metrics && (
        <div className={styles.metricsGrid}>
          <div className={styles.metricCard}>
            <div className={styles.metricValue}>{metrics.totalExecutions}</div>
            <div className={styles.metricLabel}>Total Executions</div>
          </div>
          <div className={styles.metricCard}>
            <div className={styles.metricValue}>{(metrics.successRate * 100).toFixed(1)}%</div>
            <div className={styles.metricLabel}>Success Rate</div>
          </div>
          <div className={styles.metricCard}>
            <div className={`${styles.metricValue} ${styles.succeeded}`}>{metrics.succeededExecutions}</div>
            <div className={styles.metricLabel}>Succeeded</div>
          </div>
          <div className={styles.metricCard}>
            <div className={`${styles.metricValue} ${styles.failed}`}>{metrics.failedExecutions}</div>
            <div className={styles.metricLabel}>Failed</div>
          </div>
        </div>
      )}

      <div className={styles.tableContainer}>
        <h2 className={styles.tableTitle}>Recent Executions</h2>
        {executions?.items && executions.items.length > 0 ? (
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Rule Name</th>
                <th>Status</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {executions.items.map((ex) => (
                <tr key={ex.id}>
                  <td>{ex.ruleName}</td>
                  <td>
                    <span className={`${styles.badge} ${styles[ex.status.toLowerCase()] || styles.default}`}>
                      {ex.status}
                    </span>
                  </td>
                  <td>{new Date(ex.createdAt).toLocaleString()}</td>
                  <td>
                    <button className={styles.btnSecondary} onClick={() => setSelectedExecutionId(ex.id)}>
                      View Details
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p className={styles.emptyState}>No executions found.</p>
        )}
        
        {executions && executions.totalPages > 1 && (
          <div className={styles.pagination}>
            <button 
              disabled={page === 1} 
              onClick={() => setPage(page - 1)}
              className={styles.pageBtn}
            >
              Previous
            </button>
            <span className={styles.pageInfo}>Page {page} of {executions.totalPages}</span>
            <button 
              disabled={page === executions.totalPages} 
              onClick={() => setPage(page + 1)}
              className={styles.pageBtn}
            >
              Next
            </button>
          </div>
        )}
      </div>

      {selectedExecutionId && (
        <ExecutionDetailModal 
          executionId={selectedExecutionId} 
          onClose={() => setSelectedExecutionId(null)} 
        />
      )}
    </div>
  );
};
