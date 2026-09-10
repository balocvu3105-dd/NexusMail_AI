export interface SearchHit {
  emailId: string;
  subject: string;
  sender: string;
  receivedAt: string;
  snippet: string | null;
  score: number;
}

export interface SearchResult {
  hits: SearchHit[];
  totalCount: number;
  page: number;
  pageSize: number;
  searchMode: string;
  searchDurationMs: number;
}
