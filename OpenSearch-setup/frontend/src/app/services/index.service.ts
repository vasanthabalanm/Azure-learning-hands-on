import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface IndexStatus {
  indexName: string;
  exists: boolean;
  documentCount: number;
  sizeInBytes: number;
  health: string;
}

@Injectable({
  providedIn: 'root'
})
export class IndexService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Get the status of the products index.
   */
  getIndexStatus(): Observable<IndexStatus> {
    return this.http.get<IndexStatus>(`${this.apiUrl}/index/status`);
  }

  /**
   * Create the products index with mappings.
   */
  createIndex(): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/index/create`, {});
  }

  /**
   * Seed the index with sample products.
   */
  seedIndex(): Observable<{ success: boolean; message: string; count: number }> {
    return this.http.post<{ success: boolean; message: string; count: number }>(`${this.apiUrl}/index/seed`, {});
  }

  /**
   * Reset the index (delete and recreate with sample data).
   */
  resetIndex(): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/index/reset`, {});
  }

  /**
   * Delete the products index.
   */
  deleteIndex(): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.apiUrl}/index`);
  }
}
