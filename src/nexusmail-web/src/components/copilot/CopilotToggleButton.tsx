import React from 'react';
import { Bot } from 'lucide-react';
import { useCopilotStore } from '../../stores/copilotStore';
import styles from './CopilotToggleButton.module.css';

export const CopilotToggleButton: React.FC = () => {
  const { toggleDrawer, isOpen } = useCopilotStore();

  return (
    <button 
      className={`${styles.toggleBtn} ${isOpen ? styles.active : ''}`}
      onClick={toggleDrawer}
      title="Ask Copilot"
    >
      <Bot size={22} />
    </button>
  );
};
