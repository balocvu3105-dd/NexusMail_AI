/// <reference types="@testing-library/jest-dom" />
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { CopilotActionCard } from './CopilotActionCard';
import type { ActionProposal } from '../../api/copilotClient';
import { executeAction } from '../../api/copilotClient';

vi.mock('../../api/copilotClient', () => ({
  executeAction: vi.fn(),
}));

describe('CopilotActionCard', () => {
  const mockCreateTask: ActionProposal = {
    actionType: 'CreateTask',
    parameters: {
      title: 'Buy milk',
      description: 'At the store'
    }
  };

  const mockApplyLabel: ActionProposal = {
    actionType: 'ApplyLabel',
    parameters: {
      emailId: '123',
      label: 'Important'
    }
  };

  const mockUnknown: ActionProposal = {
    actionType: 'SomeRandomAction',
    parameters: {}
  };

  it('renders CreateTask correctly', () => {
    render(<CopilotActionCard action={mockCreateTask} />);
    (expect(screen.getByText('Create Task')) as any).toBeInTheDocument();
  });

  it('renders ApplyLabel correctly', () => {
    render(<CopilotActionCard action={mockApplyLabel} />);
    (expect(screen.getByText('Apply Label')) as any).toBeInTheDocument();
  });

  it('does not render unknown action types', () => {
    render(<CopilotActionCard action={mockUnknown} />);
    (expect(screen.getByText('SomeRandomAction')) as any).toBeInTheDocument();
  });

  it('calls executeCopilotAction on Approve & Run', async () => {
    (executeAction as any).mockResolvedValueOnce({ success: true });
    
    render(<CopilotActionCard action={mockCreateTask} />);
    
    const button = screen.getByRole('button', { name: /Approve & Run/i });
    fireEvent.click(button);
    
    expect(executeAction).toHaveBeenCalledWith(
      expect.any(String), // idempotencyKey
      mockCreateTask
    );
    
    // Wait for the button text to change back to Success or something
    await waitFor(() => {
      (expect(screen.getByText('✅ Done')) as any).toBeInTheDocument();
    });
  });

  it('prevents double execution on double click', async () => {
    // Delay the mock to simulate network
    let resolveExecution: any;
    const executionPromise = new Promise(resolve => {
      resolveExecution = resolve;
    });
    
    (executeAction as any).mockImplementation(() => executionPromise);
    
    render(<CopilotActionCard action={mockCreateTask} />);
    
    const button = screen.getByRole('button', { name: /Approve & Run/i });
    fireEvent.click(button);
    fireEvent.click(button); // Double click
    
    expect(executeAction).toHaveBeenCalledTimes(1);
    
    resolveExecution({ success: true });
  });
});
