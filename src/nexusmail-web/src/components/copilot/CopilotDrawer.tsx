import React, { useState, useRef, useEffect } from 'react';
import { X, Send, Bot } from 'lucide-react';
import { useCopilotStore } from '../../stores/copilotStore';
import { CopilotMessage } from './CopilotMessage';
import styles from './CopilotDrawer.module.css';

export const CopilotDrawer: React.FC = () => {
  const { isOpen, closeDrawer, messages, ask, isStreaming, clearChat } = useCopilotStore();
  const [inputValue, setInputValue] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages, isStreaming]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputValue.trim()) return;

    ask(inputValue.trim());
    setInputValue('');
  };

  return (
    <div className={styles.overlay}>
      <div className={styles.backdrop} onClick={closeDrawer} />
      <div className={styles.drawer}>
        <div className={styles.header}>
          <div className={styles.title}>
            <Bot size={20} className={styles.botIcon} />
            <h2>Nexus Copilot</h2>
          </div>
          <div className={styles.actions}>
            <button onClick={clearChat} className={styles.clearBtn} title="Clear Chat">
              Clear
            </button>
            <button onClick={closeDrawer} className={styles.closeBtn}>
              <X size={20} />
            </button>
          </div>
        </div>

        <div className={styles.messagesContainer}>
          {messages.length === 0 ? (
            <div className={styles.emptyState}>
              <Bot size={48} className={styles.emptyIcon} />
              <p>How can I help you today?</p>
              <p className={styles.subtitle}>Ask questions about your emails, extract information, or summarize threads.</p>
            </div>
          ) : (
            <div className={styles.messageList}>
              {messages.map((msg) => (
                <CopilotMessage key={msg.id} message={msg} />
              ))}
              <div ref={messagesEndRef} />
            </div>
          )}
        </div>

        <div className={styles.inputContainer}>
          <form onSubmit={handleSubmit} className={styles.inputForm}>
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="Ask anything..."
              className={styles.input}
              disabled={isStreaming}
            />
            <button
              type="submit"
              className={styles.sendBtn}
              disabled={!inputValue.trim() || isStreaming}
            >
              <Send size={18} />
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
