import { useAuthStore } from '../stores/authStore.ts';

export class SseLineParser {
  private buffer: string = '';
  private currentEvent: string | null = null;
  private currentData: string = '';

  private onEvent: (event: string, data: string) => void;

  constructor(onEvent: (event: string, data: string) => void) {
    this.onEvent = onEvent;
  }

  public feed(chunk: string) {
    this.buffer += chunk;
    
    let newlineIndex: number;
    while ((newlineIndex = this.buffer.indexOf('\n')) !== -1) {
      let line = this.buffer.slice(0, newlineIndex);
      
      // Handle \r\n
      if (line.endsWith('\r')) {
        line = line.slice(0, -1);
      }
      
      this.buffer = this.buffer.slice(newlineIndex + 1);
      this.processLine(line);
    }
  }

  private processLine(line: string) {
    if (line === '') {
      if (this.currentEvent !== null || this.currentData !== '') {
        this.onEvent(this.currentEvent || 'message', this.currentData);
        this.currentEvent = null;
        this.currentData = '';
      }
      return;
    }

    if (line.startsWith('event:')) {
      this.currentEvent = line.substring(6).trim();
    } else if (line.startsWith('data:')) {
      // If multiple data lines, technically SSE joins with \n. But our backend sends one line per event.
      this.currentData += line.substring(5).trim();
    }
  }
}

export interface ActionProposal {
  actionType: string;
  parameters: any;
}

export interface CopilotStreamEventHandlers {
  onChunk: (text: string) => void;
  onComplete: (state: string, evidence: any[], suggestedActions: ActionProposal[]) => void;
  onError: (error: Error) => void;
}

export async function executeAction(idempotencyKey: string, proposal: ActionProposal) {
  const { accessToken, workspaceId } = useAuthStore.getState();
  const headers: HeadersInit = { 'Content-Type': 'application/json' };
  if (accessToken) headers['Authorization'] = `Bearer ${accessToken}`;
  if (workspaceId) headers['X-Workspace-Id'] = workspaceId;

  const response = await fetch('http://localhost:5038/api/v1/copilot/actions/execute', {
    method: 'POST',
    headers,
    body: JSON.stringify({ idempotencyKey, proposal }),
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Execution Failed: ${text}`);
  }
}

export async function streamAskCopilot(query: string, signal: AbortSignal, handlers: CopilotStreamEventHandlers) {
  const { accessToken, workspaceId } = useAuthStore.getState();

  const headers: HeadersInit = {
    'Content-Type': 'application/json'
  };

  if (accessToken) {
    headers['Authorization'] = `Bearer ${accessToken}`;
  }

  if (workspaceId) {
    headers['X-Workspace-Id'] = workspaceId;
  }

  try {
    const response = await fetch('http://localhost:5038/api/v1/copilot/stream', {
      method: 'POST',
      headers,
      body: JSON.stringify({ query }),
      signal
    });

    if (!response.ok) {
      const text = await response.text();
      throw new Error(`Copilot API Error: ${response.status} ${text}`);
    }

    if (!response.body) {
      throw new Error('ReadableStream not supported in this browser.');
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder('utf-8');

    const parser = new SseLineParser((event, data) => {
      try {
        if (!data) return;
        const parsed = JSON.parse(data);
        if (event === 'chunk') {
          handlers.onChunk(parsed.text || '');
        } else if (event === 'complete') {
          handlers.onComplete(parsed.state, parsed.evidence || [], parsed.suggestedActions || []);
        }
      } catch (err) {
        handlers.onError(new Error(`Failed to parse SSE data: ${err}`));
      }
    });

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      
      const chunk = decoder.decode(value, { stream: true });
      parser.feed(chunk);
    }
    
    // Flush decoder
    const finalChunk = decoder.decode();
    if (finalChunk) {
      parser.feed(finalChunk);
    }

  } catch (error: any) {
    if (error.name === 'AbortError') {
      // Aborted intentionally, do not fire onError if we want to swallow, 
      // but standard approach is to let caller handle AbortError.
      throw error; 
    }
    handlers.onError(error);
  }
}
