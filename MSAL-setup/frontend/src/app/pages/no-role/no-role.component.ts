import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '@app/core/services/auth.service';

/**
 * Page shown to authenticated users who have no role assigned.
 * Provides guidance on how to get a role assigned.
 */
@Component({
  selector: 'app-no-role',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="no-role-container">
      <div class="no-role-card">
        <div class="icon">⚠️</div>
        <h1>No Role Assigned</h1>
        
        <div class="user-info">
          <p>Logged in as:</p>
          <strong>{{ displayName }}</strong>
          <span class="email">{{ email }}</span>
        </div>

        <div class="message">
          <p>Your account doesn't have any application role assigned.</p>
          <p>Please contact your IT administrator to request access.</p>
        </div>

        <div class="info-box">
          <h3>📋 For Administrators</h3>
          <p>To assign a role to this user:</p>
          <ol>
            <li>Go to <strong>Azure Portal</strong> → <strong>Enterprise Applications</strong></li>
            <li>Select <strong>MsalDemo-API</strong></li>
            <li>Click <strong>Users and groups</strong></li>
            <li>Click <strong>Add user/group</strong></li>
            <li>Select the user and assign a role (Admin, Manager, or User)</li>
          </ol>
        </div>

        <div class="actions">
          <a routerLink="/profile" class="btn btn-primary">View My Profile</a>
          <button class="btn btn-secondary" (click)="refreshRoles()">🔄 Refresh Roles</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .no-role-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 70vh;
      padding: 2rem;
    }

    .no-role-card {
      background: white;
      border-radius: 16px;
      padding: 3rem;
      max-width: 600px;
      text-align: center;
      box-shadow: 0 4px 24px rgba(0,0,0,0.1);
    }

    .icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    h1 {
      color: #d97706;
      margin: 0 0 1.5rem;
    }

    .user-info {
      background: #f3f4f6;
      padding: 1rem;
      border-radius: 8px;
      margin-bottom: 1.5rem;
    }

    .user-info p {
      margin: 0;
      color: #6b7280;
      font-size: 0.875rem;
    }

    .user-info strong {
      display: block;
      font-size: 1.25rem;
      color: #1f2937;
      margin-top: 0.25rem;
    }

    .email {
      display: block;
      color: #6b7280;
      font-size: 0.875rem;
    }

    .message {
      margin-bottom: 1.5rem;
    }

    .message p {
      margin: 0.5rem 0;
      color: #4b5563;
    }

    .info-box {
      background: #eff6ff;
      border: 1px solid #bfdbfe;
      border-radius: 8px;
      padding: 1.5rem;
      text-align: left;
      margin-bottom: 2rem;
    }

    .info-box h3 {
      margin: 0 0 0.75rem;
      color: #1e40af;
    }

    .info-box p {
      margin: 0 0 0.5rem;
      color: #1e40af;
    }

    .info-box ol {
      margin: 0;
      padding-left: 1.25rem;
      color: #374151;
    }

    .info-box li {
      margin-bottom: 0.5rem;
    }

    .actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
    }

    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
      text-decoration: none;
    }

    .btn-primary {
      background: #2563eb;
      color: white;
    }

    .btn-secondary {
      background: #e5e7eb;
      color: #374151;
    }

    .btn:hover {
      opacity: 0.9;
    }
  `]
})
export class NoRoleComponent {
  private readonly authService = inject(AuthService);

  displayName = '';
  email = '';

  constructor() {
    const account = this.authService.getActiveAccount();
    this.displayName = account?.name ?? 'Unknown';
    this.email = account?.username ?? '';
  }

  refreshRoles(): void {
    // Force a page reload to get fresh token with updated roles
    window.location.reload();
  }
}
