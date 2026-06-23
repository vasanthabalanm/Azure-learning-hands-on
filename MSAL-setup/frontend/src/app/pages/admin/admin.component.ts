import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ApiService, AuditLog, UserListItem } from '@app/core/services/api.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <h1>Admin Area</h1>
      <p>This area is accessible only by <span class="role-badge role-admin">Admin</span> role.</p>
    </div>

    <div class="card">
      <h2>All Users</h2>
      @if (usersLoading) {
        <p>Loading users...</p>
      } @else if (usersError) {
        <div class="alert alert-error">{{ usersError }}</div>
      } @else {
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Display Name</th>
              <th>Email</th>
              <th>Tenant ID</th>
              <th>Last Login</th>
            </tr>
          </thead>
          <tbody>
            @for (user of users; track user.id) {
              <tr>
                <td>{{ user.id }}</td>
                <td>{{ user.displayName }}</td>
                <td>{{ user.email }}</td>
                <td><code>{{ user.tenantId | slice:0:8 }}...</code></td>
                <td>{{ user.lastLoginAt | date:'short' }}</td>
              </tr>
            } @empty {
              <tr>
                <td colspan="5">No users found.</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    <div class="card">
      <h2>Audit Logs</h2>
      @if (logsLoading) {
        <p>Loading audit logs...</p>
      } @else if (logsError) {
        <div class="alert alert-error">{{ logsError }}</div>
      } @else {
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Action</th>
              <th>Performed By</th>
              <th>Details</th>
              <th>Timestamp</th>
            </tr>
          </thead>
          <tbody>
            @for (log of auditLogs; track log.id) {
              <tr>
                <td>{{ log.id }}</td>
                <td>{{ log.action }}</td>
                <td>{{ log.performedBy }}</td>
                <td>{{ log.details ?? '-' }}</td>
                <td>{{ log.timestamp | date:'medium' }}</td>
              </tr>
            } @empty {
              <tr>
                <td colspan="5">No audit logs found.</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
  styles: [`
    code {
      font-family: monospace;
      background: #f0f0f0;
      padding: 0.125rem 0.25rem;
      border-radius: 2px;
      font-size: 0.875rem;
    }
  `]
})
export class AdminComponent implements OnInit {
  private readonly apiService = inject(ApiService);

  users: UserListItem[] = [];
  usersLoading = true;
  usersError: string | null = null;

  auditLogs: AuditLog[] = [];
  logsLoading = true;
  logsError: string | null = null;

  ngOnInit(): void {
    this.loadUsers();
    this.loadAuditLogs();
  }

  private loadUsers(): void {
    this.apiService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.usersLoading = false;
      },
      error: (err) => {
        this.usersError = err.message ?? 'Failed to load users';
        this.usersLoading = false;
      }
    });
  }

  private loadAuditLogs(): void {
    this.apiService.getAuditLogs().subscribe({
      next: (logs) => {
        this.auditLogs = logs;
        this.logsLoading = false;
      },
      error: (err) => {
        this.logsError = err.message ?? 'Failed to load audit logs';
        this.logsLoading = false;
      }
    });
  }
}
