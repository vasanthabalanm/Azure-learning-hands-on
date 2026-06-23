import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '@app/core/services/auth.service';
import { ApiService, AuditLog, UserListItem } from '@app/core/services/api.service';

/**
 * Admin Dashboard - Landing page for Admin role users.
 * Shows system-wide metrics, user management, and admin controls.
 */
@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard-header admin-header">
      <div class="header-content">
        <div class="welcome-section">
          <span class="role-indicator">ADMIN</span>
          <h1>Welcome back, {{ displayName }}</h1>
          <p class="subtitle">System Administrator Dashboard</p>
        </div>
        <div class="header-stats">
          <div class="quick-stat">
            <span class="stat-number">{{ totalUsers }}</span>
            <span class="stat-label">Total Users</span>
          </div>
          <div class="quick-stat">
            <span class="stat-number">{{ activeToday }}</span>
            <span class="stat-label">Active Today</span>
          </div>
          <div class="quick-stat">
            <span class="stat-number">{{ pendingActions }}</span>
            <span class="stat-label">Pending Actions</span>
          </div>
        </div>
      </div>
    </div>

    <div class="dashboard-content">
      <div class="dashboard-grid">
        <!-- System Health Card -->
        <div class="card system-health">
          <div class="card-header">
            <h2>🔧 System Health</h2>
            <span class="status-badge healthy">All Systems Operational</span>
          </div>
          <div class="health-metrics">
            <div class="metric">
              <span class="metric-label">API Response Time</span>
              <div class="metric-bar">
                <div class="bar-fill" style="width: 15%"></div>
              </div>
              <span class="metric-value">45ms</span>
            </div>
            <div class="metric">
              <span class="metric-label">Database Load</span>
              <div class="metric-bar">
                <div class="bar-fill" style="width: 32%"></div>
              </div>
              <span class="metric-value">32%</span>
            </div>
            <div class="metric">
              <span class="metric-label">Memory Usage</span>
              <div class="metric-bar">
                <div class="bar-fill" style="width: 58%"></div>
              </div>
              <span class="metric-value">58%</span>
            </div>
          </div>
        </div>

        <!-- Quick Actions Card -->
        <div class="card quick-actions">
          <h2>⚡ Quick Actions</h2>
          <div class="action-buttons">
            <a routerLink="/admin" class="action-btn">
              <span class="icon">👥</span>
              <span>Manage Users</span>
            </a>
            <button class="action-btn" (click)="exportLogs()">
              <span class="icon">📊</span>
              <span>Export Audit Logs</span>
            </button>
            <button class="action-btn">
              <span class="icon">🔐</span>
              <span>Security Settings</span>
            </button>
            <button class="action-btn">
              <span class="icon">📧</span>
              <span>Send Announcement</span>
            </button>
          </div>
        </div>

        <!-- Recent Activity Card -->
        <div class="card recent-activity">
          <h2>📋 Recent System Activity</h2>
          @if (logsLoading) {
            <div class="loading-spinner">Loading...</div>
          } @else {
            <ul class="activity-list">
              @for (log of recentLogs; track log.id) {
                <li class="activity-item">
                  <div class="activity-icon" [ngClass]="getActivityIcon(log.action)">
                    {{ getActivityEmoji(log.action) }}
                  </div>
                  <div class="activity-details">
                    <span class="activity-action">{{ log.action }}</span>
                    <span class="activity-user">by {{ log.performedBy }}</span>
                  </div>
                  <span class="activity-time">{{ log.timestamp | date:'short' }}</span>
                </li>
              } @empty {
                <li class="no-activity">No recent activity</li>
              }
            </ul>
          }
        </div>

        <!-- User Overview Card -->
        <div class="card user-overview">
          <h2>👥 User Overview</h2>
          <div class="user-stats">
            <div class="user-stat-item">
              <div class="stat-circle admin">{{ adminCount }}</div>
              <span>Admins</span>
            </div>
            <div class="user-stat-item">
              <div class="stat-circle manager">{{ managerCount }}</div>
              <span>Managers</span>
            </div>
            <div class="user-stat-item">
              <div class="stat-circle user">{{ userCount }}</div>
              <span>Users</span>
            </div>
          </div>
          <a routerLink="/admin" class="view-all-link">View All Users →</a>
        </div>
      </div>

      <!-- Navigation Cards -->
      <div class="nav-cards">
        <a routerLink="/manager" class="nav-card manager-nav">
          <span class="nav-icon">📈</span>
          <div class="nav-info">
            <h3>Manager Dashboard</h3>
            <p>View team reports and analytics</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
        <a routerLink="/user" class="nav-card user-nav">
          <span class="nav-icon">📝</span>
          <div class="nav-info">
            <h3>User Dashboard</h3>
            <p>Access tasks and notifications</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
        <a routerLink="/profile" class="nav-card profile-nav">
          <span class="nav-icon">👤</span>
          <div class="nav-info">
            <h3>My Profile</h3>
            <p>View your account details</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-header {
      background: linear-gradient(135deg, #dc2626 0%, #991b1b 100%);
      color: white;
      padding: 2rem;
      border-radius: 12px;
      margin-bottom: 2rem;
    }

    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1rem;
    }

    .role-indicator {
      background: rgba(255,255,255,0.2);
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
      font-weight: 700;
      letter-spacing: 1px;
    }

    .welcome-section h1 {
      margin: 0.5rem 0 0.25rem;
      font-size: 1.75rem;
    }

    .subtitle {
      opacity: 0.9;
      margin: 0;
    }

    .header-stats {
      display: flex;
      gap: 2rem;
    }

    .quick-stat {
      text-align: center;
      background: rgba(255,255,255,0.1);
      padding: 1rem 1.5rem;
      border-radius: 8px;
    }

    .stat-number {
      display: block;
      font-size: 2rem;
      font-weight: 700;
    }

    .stat-label {
      font-size: 0.875rem;
      opacity: 0.9;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 1.5rem;
      margin-bottom: 2rem;
    }

    .card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }

    .card h2 {
      margin: 0 0 1rem;
      font-size: 1.125rem;
      color: #374151;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .card-header h2 {
      margin: 0;
    }

    .status-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .status-badge.healthy {
      background: #dcfce7;
      color: #166534;
    }

    .health-metrics {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .metric {
      display: grid;
      grid-template-columns: 120px 1fr 60px;
      align-items: center;
      gap: 1rem;
    }

    .metric-label {
      font-size: 0.875rem;
      color: #6b7280;
    }

    .metric-bar {
      height: 8px;
      background: #e5e7eb;
      border-radius: 4px;
      overflow: hidden;
    }

    .bar-fill {
      height: 100%;
      background: linear-gradient(90deg, #22c55e, #16a34a);
      border-radius: 4px;
    }

    .metric-value {
      font-weight: 600;
      color: #374151;
      text-align: right;
    }

    .action-buttons {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 0.75rem;
    }

    .action-btn {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.75rem 1rem;
      background: #f3f4f6;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      text-decoration: none;
      color: #374151;
      font-size: 0.875rem;
      transition: all 0.2s;
    }

    .action-btn:hover {
      background: #e5e7eb;
      transform: translateY(-1px);
    }

    .action-btn .icon {
      font-size: 1.25rem;
    }

    .activity-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .activity-item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid #f3f4f6;
    }

    .activity-item:last-child {
      border-bottom: none;
    }

    .activity-icon {
      width: 36px;
      height: 36px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1rem;
    }

    .activity-details {
      flex: 1;
    }

    .activity-action {
      display: block;
      font-weight: 500;
      color: #374151;
    }

    .activity-user {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .activity-time {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .user-stats {
      display: flex;
      justify-content: space-around;
      margin-bottom: 1rem;
    }

    .user-stat-item {
      text-align: center;
    }

    .stat-circle {
      width: 48px;
      height: 48px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 1.25rem;
      margin: 0 auto 0.5rem;
    }

    .stat-circle.admin {
      background: #fecaca;
      color: #dc2626;
    }

    .stat-circle.manager {
      background: #fef3c7;
      color: #d97706;
    }

    .stat-circle.user {
      background: #dbeafe;
      color: #2563eb;
    }

    .view-all-link {
      display: block;
      text-align: center;
      color: #dc2626;
      font-weight: 500;
      text-decoration: none;
    }

    .view-all-link:hover {
      text-decoration: underline;
    }

    .nav-cards {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
    }

    .nav-card {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1.25rem;
      background: white;
      border-radius: 12px;
      text-decoration: none;
      color: inherit;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      transition: all 0.2s;
      border-left: 4px solid transparent;
    }

    .nav-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0,0,0,0.12);
    }

    .nav-card.manager-nav {
      border-left-color: #d97706;
    }

    .nav-card.user-nav {
      border-left-color: #2563eb;
    }

    .nav-card.profile-nav {
      border-left-color: #7c3aed;
    }

    .nav-icon {
      font-size: 2rem;
    }

    .nav-info {
      flex: 1;
    }

    .nav-info h3 {
      margin: 0 0 0.25rem;
      font-size: 1rem;
    }

    .nav-info p {
      margin: 0;
      font-size: 0.875rem;
      color: #6b7280;
    }

    .nav-arrow {
      font-size: 1.25rem;
      color: #9ca3af;
    }

    .loading-spinner {
      text-align: center;
      padding: 2rem;
      color: #6b7280;
    }

    .no-activity {
      text-align: center;
      padding: 1rem;
      color: #9ca3af;
    }

    @media (max-width: 768px) {
      .header-content {
        flex-direction: column;
        text-align: center;
      }

      .header-stats {
        justify-content: center;
      }

      .dashboard-grid {
        grid-template-columns: 1fr;
      }

      .nav-cards {
        grid-template-columns: 1fr;
      }

      .action-buttons {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly apiService = inject(ApiService);

  displayName = '';
  totalUsers = 0;
  activeToday = 0;
  pendingActions = 0;
  adminCount = 0;
  managerCount = 0;
  userCount = 0;
  recentLogs: AuditLog[] = [];
  logsLoading = true;

  ngOnInit(): void {
    const account = this.authService.getActiveAccount();
    this.displayName = account?.name ?? 'Admin';

    // Simulated stats - in production, fetch from API
    this.totalUsers = 24;
    this.activeToday = 18;
    this.pendingActions = 3;
    this.adminCount = 2;
    this.managerCount = 5;
    this.userCount = 17;

    this.loadRecentLogs();
  }

  private loadRecentLogs(): void {
    this.apiService.getAuditLogs().subscribe({
      next: (logs) => {
        this.recentLogs = logs.slice(0, 5);
        this.logsLoading = false;
      },
      error: () => {
        // Fallback demo data when API is unavailable
        this.recentLogs = [
          { id: 1, action: 'User Login', performedBy: 'admin@company.com', details: 'Successful login', timestamp: new Date().toISOString() },
          { id: 2, action: 'Create User', performedBy: 'admin@company.com', details: 'Added new user', timestamp: new Date(Date.now() - 3600000).toISOString() },
          { id: 3, action: 'Update Settings', performedBy: 'admin@company.com', details: 'Changed security policy', timestamp: new Date(Date.now() - 7200000).toISOString() },
          { id: 4, action: 'User Login', performedBy: 'manager@company.com', details: 'Successful login', timestamp: new Date(Date.now() - 86400000).toISOString() }
        ];
        this.logsLoading = false;
      }
    });
  }

  getActivityIcon(action: string): string {
    if (action.includes('Login')) return 'login';
    if (action.includes('Create')) return 'create';
    if (action.includes('Update')) return 'update';
    return 'default';
  }

  getActivityEmoji(action: string): string {
    if (action.includes('Login')) return '🔓';
    if (action.includes('Create')) return '➕';
    if (action.includes('Update')) return '✏️';
    if (action.includes('Delete')) return '🗑️';
    return '📌';
  }

  exportLogs(): void {
    alert('Exporting audit logs...');
  }
}
