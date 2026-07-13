import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export interface SseEvent {
  eventType: string;
  data: any;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class SseService {
  private eventSource: EventSource | null = null;
  private eventSubject = new Subject<SseEvent>();
  private connectionStatusSubject = new Subject<boolean>();

  public events$: Observable<SseEvent> = this.eventSubject.asObservable();
  public connectionStatus$: Observable<boolean> = this.connectionStatusSubject.asObservable();

  connect(url: string = 'http://localhost:5000/api/sse/stream'): void {
    if (this.eventSource) {
      console.log('SSE already connected');
      return;
    }

    console.log('🔌 Connecting to SSE endpoint:', url);
    this.eventSource = new EventSource(url);

    this.eventSource.onopen = () => {
      console.log('✅ SSE connection established');
      this.connectionStatusSubject.next(true);
    };

    // The server sends data in format: data: {json}\n\n
    this.eventSource.onmessage = (event) => {
      try {
        const parsed = JSON.parse(event.data);
        console.log('📩 SSE Event received:', parsed);
        this.eventSubject.next(parsed);
      } catch (error) {
        console.error('Failed to parse SSE event:', error);
      }
    };

    this.eventSource.onerror = (error) => {
      console.error('❌ SSE connection error:', error);
      this.connectionStatusSubject.next(false);
      this.disconnect();
    };
  }

  disconnect(): void {
    if (this.eventSource) {
      console.log('🔌 Disconnecting SSE');
      this.eventSource.close();
      this.eventSource = null;
      this.connectionStatusSubject.next(false);
    }
  }

  isConnected(): boolean {
    return this.eventSource !== null && this.eventSource.readyState === EventSource.OPEN;
  }
}
