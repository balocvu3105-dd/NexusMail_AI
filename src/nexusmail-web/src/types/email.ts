export interface InboxMessage {
  id: string;
  messageId: string;
  sender: string;
  subject: string;
  content: string;
  receivedAt: string;
  summary: string | null;
  priorityScore: number;
  priority: string | null;
  category: string | null;
}

export interface EmailDetail {
  id: string;
  messageId: string;
  sender: string;
  subject: string;
  content: string;
  receivedAt: string;
  category: string | null;
  language: string | null;
  confidence: number | null;
  summary: string | null;
  priorityScore: number | null;
  priority: string | null;
  tags: string[] | null;
}
