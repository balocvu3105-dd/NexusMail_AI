import { create } from 'zustand';
import { streamAskCopilot } from '../api/copilotClient';
import type { ActionProposal } from '../api/copilotClient';

export interface CopilotMessage {
  id: string;
  role: 'user' | 'assistant';
  text: string;
  isStreaming?: boolean;
  state?: string;
  evidence?: any[];
  suggestedActions?: ActionProposal[];
}

interface CopilotStore {
  isOpen: boolean;
  messages: CopilotMessage[];
  isStreaming: boolean;
  abortController: AbortController | null;

  toggleDrawer: () => void;
  openDrawer: () => void;
  closeDrawer: () => void;
  clearChat: () => void;
  ask: (query: string) => Promise<void>;
  abortCurrent: () => void;
}

const generateId = () => Math.random().toString(36).substr(2, 9);

export const useCopilotStore = create<CopilotStore>((set, get) => ({
  isOpen: false,
  messages: [],
  isStreaming: false,
  abortController: null,

  toggleDrawer: () => {
    const state = get();
    if (state.isOpen) {
      state.closeDrawer();
    } else {
      state.openDrawer();
    }
  },

  openDrawer: () => set({ isOpen: true }),

  closeDrawer: () => {
    // When drawer closes, abort any ongoing stream
    get().abortCurrent();
    set({ isOpen: false });
  },

  clearChat: () => {
    get().abortCurrent();
    set({ messages: [] });
  },

  abortCurrent: () => {
    const { abortController } = get();
    if (abortController) {
      abortController.abort();
      set({ abortController: null, isStreaming: false });
    }
  },

  ask: async (query: string) => {
    // Cancel any previous request
    get().abortCurrent();

    const userMessage: CopilotMessage = {
      id: generateId(),
      role: 'user',
      text: query,
    };

    const assistantMessageId = generateId();
    const assistantMessage: CopilotMessage = {
      id: assistantMessageId,
      role: 'assistant',
      text: '',
      isStreaming: true,
    };

    set((state) => ({
      messages: [...state.messages, userMessage, assistantMessage],
      isStreaming: true,
    }));

    const abortController = new AbortController();
    set({ abortController });

    try {
      await streamAskCopilot(query, abortController.signal, {
        onChunk: (textChunk) => {
          set((state) => {
            const msgs = [...state.messages];
            const idx = msgs.findIndex((m) => m.id === assistantMessageId);
            if (idx > -1) {
              msgs[idx] = { ...msgs[idx], text: msgs[idx].text + textChunk };
            }
            return { messages: msgs };
          });
        },
        onComplete: (completionState, evidence, suggestedActions) => {
          set((state) => {
            const msgs = [...state.messages];
            const idx = msgs.findIndex((m) => m.id === assistantMessageId);
            if (idx > -1) {
              msgs[idx] = {
                ...msgs[idx],
                isStreaming: false,
                state: completionState,
                evidence: evidence,
                suggestedActions: suggestedActions,
              };
            }
            return { messages: msgs, isStreaming: false, abortController: null };
          });
        },
        onError: (err) => {
          set((state) => {
            const msgs = [...state.messages];
            const idx = msgs.findIndex((m) => m.id === assistantMessageId);
            if (idx > -1) {
              msgs[idx] = {
                ...msgs[idx],
                isStreaming: false,
                text: msgs[idx].text + `\n\n[Error: ${err.message}]`,
              };
            }
            return { messages: msgs, isStreaming: false, abortController: null };
          });
        },
      });
    } catch (err: any) {
      if (err.name !== 'AbortError') {
        // Handled by onError usually, but just in case it throws directly
        set(() => ({ isStreaming: false, abortController: null }));
      }
    }
  },
}));
