import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '@app/core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="card">
      <h1>Azure MSAL Multi-Tenant Demo</h1>
      <p>This application demonstrates Azure AD authentication with role-based access control.</p>
      
      @if (isLoggedIn) {
        <div class="logged-in-banner">
          <div class="banner-content">
            <span class="banner-icon">👋</span>
            <div class="banner-text">
              <strong>Welcome, {{ displayName }}!</strong>
              <p>You're logged in as 
                @for (role of userRoles; track role) {
                  <span class="role-badge role-{{ role.toLowerCase() }}">{{ role }}</span>
                }
              </p>
            </div>
          </div>
          <a [routerLink]="dashboardUrl" class="btn btn-primary go-to-dashboard">
            Go to My Dashboard →
          </a>
        </div>
      } @else {
        <div class="alert alert-info">
          <strong>Getting Started:</strong> Sign in with your Microsoft account to access protected features.
        </div>
      }
      
      <h2>Features</h2>
      <ul>
        <li>Multi-tenant Azure AD authentication via MSAL</li>
        <li>Role-based authorization (Admin, Manager, User)</li>
        <li>Protected API endpoints with JWT validation</li>
        <li>PostgreSQL data persistence via EF Core</li>
        <li><strong>Role-specific dashboards with unique views</strong></li>
      </ul>
      
      <h2>Role Access Matrix</h2>
      <table>
        <thead>
          <tr>
            <th>Feature</th>
            <th>User</th>
            <th>Manager</th>
            <th>Admin</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>View Profile</td>
            <td>✓</td>
            <td>✓</td>
            <td>✓</td>
          </tr>
          <tr>
            <td>User Tasks & Notifications</td>
            <td>✓</td>
            <td>✓</td>
            <td>✓</td>
          </tr>
          <tr>
            <td>Manager Dashboard</td>
            <td>✗</td>
            <td>✓</td>
            <td>✓</td>
          </tr>
          <tr>
            <td>Team Reports</td>
            <td>✗</td>
            <td>✓</td>
            <td>✓</td>
          </tr>
          <tr>
            <td>User Management</td>
            <td>✗</td>
            <td>✗</td>
            <td>✓</td>
          </tr>
          <tr>
            <td>Audit Logs</td>
            <td>✗</td>
            <td>✗</td>
            <td>✓</td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    ul {
      margin: 1rem 0;
      padding-left: 1.5rem;
    }
    li {
      margin-bottom: 0.5rem;
    }
    .logged-in-banner {
      background: linear-gradient(135deg, #dbeafe 0%, #e0e7ff 100%);
      border: 1px solid #93c5fd;
      border-radius: 12px;
      padding: 1.5rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .banner-content {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .banner-icon {
      font-size: 2rem;
    }
    .banner-text strong {
      font-size: 1.125rem;
      color: #1e40af;
    }
    .banner-text p {
      margin: 0.25rem 0 0;
      color: #1e40af;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .go-to-dashboard {
      background: linear-gradient(90deg, #3b82f6, #8b5cf6);
      padding: 0.75rem 1.5rem;
      font-weight: 600;
      white-space: nowrap;
    }
    .go-to-dashboard:hover {
      opacity: 0.9;
    }
  `]
})
export class HomeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  
  isLoggedIn = false;
  displayName = '';
  userRoles: string[] = [];
  dashboardUrl = '/profile';

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isAuthenticated();
    
    if (this.isLoggedIn) {
      const account = this.authService.getActiveAccount();
      this.displayName = account?.name ?? 'User';
      this.userRoles = this.authService.getRolesFromAccount(account);
      this.dashboardUrl = this.authService.getDashboardUrl();
    }
  }
}
