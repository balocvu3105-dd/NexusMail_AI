import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { apiClient } from '../api/client';
import type { SearchResult } from '../types/search';
import { Search, AlertCircle, Clock, Hash, Sparkles } from 'lucide-react';
import styles from './SearchPage.module.css';

export const SearchPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  
  const queryParams = new URLSearchParams(location.search);
  const query = queryParams.get('q') || '';

  const [result, setResult] = useState<SearchResult | null>(null);
  const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'empty' | 'error'>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    // If no query string, don't hit the API. Show landing state.
    if (!query.trim()) {
      setStatus('idle');
      setResult(null);
      return;
    }

    const fetchSearch = async () => {
      try {
        setStatus('loading');
        setErrorMessage(null);

        const response = await apiClient.get<SearchResult>('/emails/search', {
          params: { query: query.trim() }
        });
        
        const data = response.data;
        if (data.hits && data.hits.length > 0) {
          setResult(data);
          setStatus('success');
        } else {
          setResult(null);
          setStatus('empty');
        }
      } catch (err: any) {
        console.error('Search failed', err);
        setStatus('error');
        setErrorMessage(err.response?.data?.message || 'Failed to execute search. Please try again.');
      }
    };

    fetchSearch();
  }, [query]);

  // Landing state when query is empty
  if (status === 'idle') {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.idleIconWrapper}>
          <Search size={48} className={styles.idleIcon} />
        </div>
        <h2 className={styles.idleTitle}>Search Intelligence</h2>
        <p className={styles.idleMessage}>
          Type in the search bar to find emails across your workspace using Hybrid Search.
        </p>
      </div>
    );
  }

  // Loading state
  if (status === 'loading') {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.spinner}></div>
        <p>Searching for "{query}"...</p>
      </div>
    );
  }

  // Error state
  if (status === 'error') {
    return (
      <div className={styles.centerContainer}>
        <AlertCircle size={48} className={styles.errorIcon} />
        <h2 className={styles.errorTitle}>Search Failed</h2>
        <p className={styles.errorMessage}>{errorMessage}</p>
        <button className={styles.retryBtn} onClick={() => window.location.reload()}>Retry Search</button>
      </div>
    );
  }

  // Empty state (no results found)
  if (status === 'empty') {
    return (
      <div className={styles.centerContainer}>
        <div className={styles.idleIconWrapper}>
          <Search size={48} className={styles.idleIcon} />
        </div>
        <h2 className={styles.errorTitle}>No Results Found</h2>
        <p className={styles.errorMessage}>
          We couldn't find any emails matching "{query}". Try adjusting your keywords.
        </p>
      </div>
    );
  }

  if (!result) return null;

  return (
    <div className={styles.searchContainer}>
      <header className={styles.header}>
        <h1 className={styles.title}>Search Results</h1>
        <div className={styles.metaInfo}>
          <span className={styles.metaBadge}>
            <Search size={14} /> Found {result.totalCount} results
          </span>
          <span className={styles.metaBadge}>
            <Sparkles size={14} /> Mode: {result.searchMode}
          </span>
          <span className={styles.metaBadge}>
            <Clock size={14} /> {result.searchDurationMs.toFixed(1)}ms
          </span>
        </div>
      </header>

      <div className={styles.resultsList}>
        {result.hits.map(hit => (
            <div 
              key={hit.emailId} 
              className={styles.resultCard}
              onClick={() => navigate(`/emails/${hit.emailId}`)}
            >
              <div className={styles.cardHeader}>
                <div className={styles.senderInfo}>
                  <div className={styles.avatar}>{hit.sender.charAt(0).toUpperCase()}</div>
                  <span className={styles.sender}>{hit.sender}</span>
                </div>
                
                <div className={styles.scoreContainer} title="Relevance Score">
                  <Hash size={14} />
                  <span>Score: {hit.score.toFixed(3)}</span>
                </div>
              </div>

              <h3 className={styles.subject}>{hit.subject}</h3>
              
              <div className={styles.snippetContainer}>
                {hit.snippet ? (
                  <div className={styles.snippet}>{hit.snippet}</div>
                ) : (
                  <div className={styles.emptySnippet}>No preview available</div>
                )}
              </div>

              <div className={styles.cardFooter}>
                <span className={styles.time}>
                  {new Date(hit.receivedAt).toLocaleString()}
                </span>
              </div>
            </div>
          ))}
      </div>
    </div>
  );
};
