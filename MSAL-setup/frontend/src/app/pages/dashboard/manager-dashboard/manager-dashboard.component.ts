import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '@app/core/services/auth.service';
import { ApiService, DashboardData, Report } from '@app/core/services/api.service';

/**
 * Manager Dashboard - Landing page for Manager role users.
 * Shows team metrics, reports, and management tools.
 */
@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard-header manager-header">
      <div class="header-content">
        <div class="welcome-section">
          <span class="role-indicator">MANAGER</span>
          <h1>Welcome back, {{ displayName }}</h1>
          <p class="subtitle">Team Management Dashboard</p>
        </div>
        <div class="header-actions">
          <button class="header-btn" (click)="generateReport()">
            📊 Generate Report
          </button>
          <button class="header-btn secondary">
            📅 Schedule Meeting
          </button>
        </div>
      </div>
    </div>

    <div class="dashboard-content">
      <!-- Key Metrics Row -->
      <div class="metrics-row">
        <div class="metric-card">
          <div class="metric-icon team">👥</div>
          <div class="metric-info">
            <span class="metric-value">{{ dashboard?.totalTeamMembers ?? 0 }}</span>
            <span class="metric-label">Team Members</span>
          </div>
          <span class="metric-trend positive">+2 this month</span>
        </div>
        <div class="metric-card">
          <div class="metric-icon projects">📁</div>
          <div class="metric-info">
            <span class="metric-value">{{ dashboard?.activeProjects ?? 0 }}</span>
            <span class="metric-label">Active Projects</span>
          </div>
          <span class="metric-trend neutral">No change</span>
        </div>
        <div class="metric-card">
          <div class="metric-icon approvals">✅</div>
          <div class="metric-info">
            <span class="metric-value">{{ dashboard?.pendingApprovals ?? 0 }}</span>
            <span class="metric-label">Pending Approvals</span>
          </div>
          <span class="metric-trend" [class.negative]="(dashboard?.pendingApprovals ?? 0) > 5">
            {{ (dashboard?.pendingApprovals ?? 0) > 0 ? 'Action needed' : 'All clear' }}
          </span>
        </div>
        <div class="metric-card">
          <div class="metric-icon productivity">📈</div>
          <div class="metric-info">
            <span class="metric-value">92%</span>
            <span class="metric-label">Team Productivity</span>
          </div>
          <span class="metric-trend positive">+5% from last week</span>
        </div>
      </div>

      <div class="dashboard-grid">
        <!-- Team Performance Card -->
        <div class="card team-performance">
          <h2>📊 Team Performance</h2>
          <div class="performance-chart">
            <div class="chart-bar">
              <span class="bar-label">Tasks Completed</span>
              <div class="bar-container">
                <div class="bar-fill" style="width: 85%"></div>
              </div>
              <span class="bar-value">85%</span>
            </div>
            <div class="chart-bar">
              <span class="bar-label">On-Time Delivery</span>
              <div class="bar-container">
                <div class="bar-fill" style="width: 78%"></div>
              </div>
              <span class="bar-value">78%</span>
            </div>
            <div class="chart-bar">
              <span class="bar-label">Quality Score</span>
              <div class="bar-container">
                <div class="bar-fill" style="width: 92%"></div>
              </div>
              <span class="bar-value">92%</span>
            </div>
            <div class="chart-bar">
              <span class="bar-label">Team Satisfaction</span>
              <div class="bar-container">
                <div class="bar-fill" style="width: 88%"></div>
              </div>
              <span class="bar-value">88%</span>
            </div>
          </div>
        </div>

        <!-- Pending Approvals Card -->
        <div class="card pending-approvals">
          <div class="card-header">
            <h2>⏳ Pending Approvals</h2>
            <span class="badge">{{ pendingItems.length }}</span>
          </div>
          <ul class="approval-list">
            @for (item of pendingItems; track item.id) {
              <li class="approval-item">
                <div class="approval-info">
                  <span class="approval-title">{{ item.title }}</span>
                  <span class="approval-requester">Requested by {{ item.requester }}</span>
                </div>
                <div class="approval-actions">
                  <button class="btn-approve" (click)="approve(item.id)">✓</button>
                  <button class="btn-reject" (click)="reject(item.id)">✕</button>
                </div>
              </li>
            } @empty {
              <li class="no-items">No pending approvals 🎉</li>
            }
          </ul>
        </div>

        <!-- Team Members Card -->
        <div class="card team-members">
          <h2>👥 Team Members</h2>
          <div class="members-list">
            @for (member of teamMembers; track member.name) {
              <div class="member-item">
                <div class="member-avatar" [style.background]="member.color">
                  {{ member.initials }}
                </div>
                <div class="member-info">
                  <span class="member-name">{{ member.name }}</span>
                  <span class="member-role">{{ member.role }}</span>
                </div>
                <span class="member-status" [class]="member.status">
                  {{ member.status }}
                </span>
              </div>
            }
          </div>
          <a routerLink="/manager" class="view-all-link">Manage Team →</a>
        </div>

        <!-- Recent Reports Card -->
        <div class="card recent-reports">
          <h2>📄 Recent Reports</h2>
          @if (reportsLoading) {
            <div class="loading">Loading reports...</div>
          } @else {
            <ul class="reports-list">
              @for (report of reports; track report.id) {
                <li class="report-item">
                  <div class="report-icon">📑</div>
                  <div class="report-info">
                    <span class="report-name">{{ report.name }}</span>
                    <span class="report-date">{{ report.generatedAt | date:'mediumDate' }}</span>
                  </div>
                  <button class="btn-download">⬇️</button>
                </li>
              } @empty {
                <li class="no-items">No reports available</li>
              }
            </ul>
          }
        </div>
      </div>

      <!-- Quick Navigation -->
      <div class="nav-cards">
        <a routerLink="/manager" class="nav-card">
          <span class="nav-icon">📈</span>
          <div class="nav-info">
            <h3>Reports & Analytics</h3>
            <p>View detailed team reports</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
        <a routerLink="/user" class="nav-card">
          <span class="nav-icon">📝</span>
          <div class="nav-info">
            <h3>My Tasks</h3>
            <p>View your personal tasks</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
        <a routerLink="/profile" class="nav-card">
          <span class="nav-icon">👤</span>
          <div class="nav-info">
            <h3>My Profile</h3>
            <p>View account details</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-header {
      background: linear-gradient(135deg, #d97706 0%, #b45309 100%);
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

    .header-actions {
      display: flex;
      gap: 0.75rem;
    }

    .header-btn {
      padding: 0.75rem 1.25rem;
      background: white;
      color: #d97706;
      border: none;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .header-btn.secondary {
      background: rgba(255,255,255,0.2);
      color: white;
    }

    .header-btn:hover {
      transform: translateY(-2px);
    }

    .metrics-row {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .metric-card {
      background: white;
      padding: 1.25rem;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .metric-icon {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.25rem;
    }

    .metric-icon.team { background: #dbeafe; }
    .metric-icon.projects { background: #fef3c7; }
    .metric-icon.approvals { background: #dcfce7; }
    .metric-icon.productivity { background: #f3e8ff; }

    .metric-info {
      display: flex;
      flex-direction: column;
    }

    .metric-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: #1f2937;
    }

    .metric-label {
      font-size: 0.875rem;
      color: #6b7280;
    }

    .metric-trend {
      font-size: 0.75rem;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      width: fit-content;
    }

    .metric-trend.positive {
      background: #dcfce7;
      color: #166534;
    }

    .metric-trend.negative {
      background: #fecaca;
      color: #dc2626;
    }

    .metric-trend.neutral {
      background: #f3f4f6;
      color: #6b7280;
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

    .badge {
      background: #d97706;
      color: white;
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.875rem;
      font-weight: 600;
    }

    .performance-chart {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .chart-bar {
      display: grid;
      grid-template-columns: 120px 1fr 50px;
      align-items: center;
      gap: 1rem;
    }

    .bar-label {
      font-size: 0.875rem;
      color: #6b7280;
    }

    .bar-container {
      height: 10px;
      background: #e5e7eb;
      border-radius: 5px;
      overflow: hidden;
    }

    .bar-fill {
      height: 100%;
      background: linear-gradient(90deg, #d97706, #f59e0b);
      border-radius: 5px;
    }

    .bar-value {
      font-weight: 600;
      color: #374151;
      text-align: right;
    }

    .approval-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .approval-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 0;
      border-bottom: 1px solid #f3f4f6;
    }

    .approval-item:last-child {
      border-bottom: none;
    }

    .approval-info {
      display: flex;
      flex-direction: column;
    }

    .approval-title {
      font-weight: 500;
      color: #374151;
    }

    .approval-requester {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .approval-actions {
      display: flex;
      gap: 0.5rem;
    }

    .btn-approve, .btn-reject {
      width: 32px;
      height: 32px;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-size: 1rem;
    }

    .btn-approve {
      background: #dcfce7;
      color: #166534;
    }

    .btn-reject {
      background: #fecaca;
      color: #dc2626;
    }

    .members-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .member-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .member-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: 600;
      font-size: 0.875rem;
    }

    .member-info {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .member-name {
      font-weight: 500;
      color: #374151;
    }

    .member-role {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .member-status {
      font-size: 0.75rem;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
    }

    .member-status.online {
      background: #dcfce7;
      color: #166534;
    }

    .member-status.away {
      background: #fef3c7;
      color: #d97706;
    }

    .member-status.offline {
      background: #f3f4f6;
      color: #6b7280;
    }

    .view-all-link {
      display: block;
      text-align: center;
      color: #d97706;
      font-weight: 500;
      text-decoration: none;
      margin-top: 1rem;
    }

    .reports-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .report-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid #f3f4f6;
    }

    .report-icon {
      font-size: 1.5rem;
    }

    .report-info {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .report-name {
      font-weight: 500;
      color: #374151;
    }

    .report-date {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .btn-download {
      background: none;
      border: none;
      cursor: pointer;
      font-size: 1.25rem;
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
      border-left: 4px solid #d97706;
    }

    .nav-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0,0,0,0.12);
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

    .loading, .no-items {
      text-align: center;
      padding: 1rem;
      color: #9ca3af;
    }

    @media (max-width: 1024px) {
      .metrics-row {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    @media (max-width: 768px) {
      .header-content {
        flex-direction: column;
        text-align: center;
      }

      .metrics-row {
        grid-template-columns: 1fr;
      }

      .dashboard-grid {
        grid-template-columns: 1fr;
      }

      .nav-cards {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ManagerDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly apiService = inject(ApiService);

  displayName = '';
  dashboard: DashboardData | null = null;
  reports: Report[] = [];
  reportsLoading = true;

  pendingItems = [
    { id: 1, title: 'Leave Request - John Doe', requester: 'John Doe' },
    { id: 2, title: 'Expense Report - Q2', requester: 'Sarah Smith' },
    { id: 3, title: 'Project Budget Increase', requester: 'Mike Johnson' }
  ];

  teamMembers = [
    { name: 'John Doe', role: 'Senior Developer', initials: 'JD', color: '#3b82f6', status: 'online' },
    { name: 'Sarah Smith', role: 'Designer', initials: 'SS', color: '#ec4899', status: 'online' },
    { name: 'Mike Johnson', role: 'Developer', initials: 'MJ', color: '#10b981', status: 'away' },
    { name: 'Emily Brown', role: 'QA Engineer', initials: 'EB', color: '#8b5cf6', status: 'offline' }
  ];

  ngOnInit(): void {
    const account = this.authService.getActiveAccount();
    this.displayName = account?.name ?? 'Manager';

    this.loadDashboard();
    this.loadReports();
  }

  private loadDashboard(): void {
    this.apiService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
      },
      error: () => {
        // Use fallback data
        this.dashboard = {
          totalTeamMembers: 8,
          activeProjects: 5,
          pendingApprovals: 3,
          lastUpdated: new Date().toISOString()
        };
      }
    });
  }

  private loadReports(): void {
    this.apiService.getReports().subscribe({
      next: (reports) => {
        this.reports = reports;
        this.reportsLoading = false;
      },
      error: () => {
        // Fallback demo data when API is unavailable
        this.reports = [
          { id: 1, name: 'Weekly Team Performance', generatedAt: new Date().toISOString() },
          { id: 2, name: 'Monthly Budget Analysis', generatedAt: new Date(Date.now() - 604800000).toISOString() },
          { id: 3, name: 'Q2 Project Status Report', generatedAt: new Date(Date.now() - 1209600000).toISOString() }
        ];
        this.reportsLoading = false;
      }
    });
  }

  generateReport(): void {
    alert('Generating new report...');
  }

  approve(id: number): void {
    this.pendingItems = this.pendingItems.filter(item => item.id !== id);
  }

  reject(id: number): void {
    this.pendingItems = this.pendingItems.filter(item => item.id !== id);
  }
}
