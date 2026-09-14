import { describe, it, expect } from 'vitest';
import { SseLineParser } from './copilotClient.ts';

describe('SseLineParser', () => {
  it('1. Event bị split giữa 2 stream chunks', () => {
    const events: any[] = [];
    const parser = new SseLineParser((e, d) => events.push({ e, d }));
    parser.feed('event: chu');
    parser.feed('nk\ndata: {"a":1}\n\n');
    
    expect(events.length).toBe(1);
    expect(events[0].e).toBe('chunk');
    expect(events[0].d).toBe('{"a":1}');
  });

  it('2. Hỗ trợ \\r\\n và \\n', () => {
    const events: any[] = [];
    const parser = new SseLineParser((e, d) => events.push({ e, d }));
    parser.feed('event: msg1\r\ndata: A\r\n\r\nevent: msg2\ndata: B\n\n');
    
    expect(events.length).toBe(2);
    expect(events[0].e).toBe('msg1');
    expect(events[0].d).toBe('A');
    expect(events[1].e).toBe('msg2');
    expect(events[1].d).toBe('B');
  });

  it('3. data: bị split giữa chunks', () => {
    const events: any[] = [];
    const parser = new SseLineParser((e, d) => events.push({ e, d }));
    parser.feed('event: chunk\ndata: {"text":"He');
    parser.feed('llo"}\n\n');
    
    expect(events.length).toBe(1);
    expect(events[0].e).toBe('chunk');
    expect(events[0].d).toBe('{"text":"Hello"}');
  });

  it('4. chunk events nối đúng thứ tự', () => {
    const events: any[] = [];
    const parser = new SseLineParser((e, d) => events.push({ e, d }));
    parser.feed('event: chunk\ndata: 1\n\nevent: chunk\ndata: 2\n\n');
    
    expect(events.length).toBe(2);
    expect(events[0].d).toBe('1');
    expect(events[1].d).toBe('2');
  });

  it('5. complete event chỉ kết thúc stream (không có thêm logic rác)', () => {
    const events: any[] = [];
    const parser = new SseLineParser((e, d) => events.push({ e, d }));
    parser.feed('event: complete\ndata: {"state":"Grounded"}\n\n');
    
    expect(events.length).toBe(1);
    expect(events[0].e).toBe('complete');
    expect(events[0].d).toBe('{"state":"Grounded"}');
  });
});
