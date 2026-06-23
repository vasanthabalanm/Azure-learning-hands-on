import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '@env/environment';

export interface Task {
  id: number;
  title: string;
  status: string;
  dueDate: string;
}

export interface Notification {
  id: number;
  message: string;
  read: boolean;
  createdAt: string;
}

export interface DashboardData {
  totalTeamMembers: number;
  activeProjects: number;
  pendingApprovals: number;
  lastUpdated: string;
}

export interface Report {
  id: number;
  name: string;
  generatedAt: string;
}

export interface AuditLog {
  id: number;
  action: string;
  performedBy: string;
  details: string | null;
  timestamp: string;
}

export interface UserListItem {
  id: number;
  objectId: string;
  email: string;
  displayName: string;
  tenantId: string;
  firstLoginAt: string;
  lastLoginAt: string;
}

/**
 * API service for backend communication.
 * All requests automatically include bearer token via MSAL interceptor.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiConfig.uri;

  // ─────────────────────────────────────────────────────────────────────────────
  // User Endpoints (all authenticated users)
  // ─────────────────────────────────────────────────────────────────────────────

  getTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.baseUrl}/user/tasks`);
  }

  getMyTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.baseUrl}/user/tasks`);
  }

  getNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.baseUrl}/user/notifications`);
  }

  updatePreferences(preferences: { theme?: string; language?: string; emailNotifications: boolean }): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/user/preferences`, preferences);
  }

  // ─────────────────────────────────────────────────────────────────────────────
  // Manager Endpoints (Manager + Admin)
  // ─────────────────────────────────────────────────────────────────────────────

  getDashboard(): Observable<DashboardData> {
    return this.http.get<DashboardData>(`${this.baseUrl}/manager/dashboard`);
  }

  getReports(): Observable<Report[]> {
    return this.http.get<Report[]>(`${this.baseUrl}/manager/reports`);
  }

  approveRequest(requestId: number): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/manager/approve/${requestId}`, {});
  }

  // ─────────────────────────────────────────────────────────────────────────────
  // Admin Endpoints (Admin only)
  // ─────────────────────────────────────────────────────────────────────────────

  getAllUsers(): Observable<UserListItem[]> {
    return this.http.get<UserListItem[]>(`${this.baseUrl}/admin/users`);
  }

  getAuditLogs(take = 50): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.baseUrl}/admin/audit-logs`, {
      params: { take: take.toString() }
    });
  }

  createAuditLog(action: string, details?: string): Observable<AuditLog> {
    return this.http.post<AuditLog>(`${this.baseUrl}/admin/audit-logs`, { action, details });
  }
}
