import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ApiService, DashboardData, Report } from '@app/core/services/api.service';

@Component({
  selector: 'app-manager',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <h1>Manager Area</h1>
      <p>This area is accessible by <span class="role-badge role-manager">Manager</span> and <span class="role-badge role-admin">Admin</span> roles.</p>
    </div>

    <div class="card">
      <h2>Team Dashboard</h2>
      @if (dashboardLoading) {
        <p>Loading dashboard...</p>
      } @else if (dashboardError) {
        <div class="alert alert-error">{{ dashboardError }}</div>
      } @else if (dashboard) {
        <div class="dashboard-grid">
          <div class="stat-card">
            <div class="stat-value">{{ dashboard.totalTeamMembers }}</div>
            <div class="stat-label">Team Members</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ dashboard.activeProjects }}</div>
            <div class="stat-label">Active Projects</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ dashboard.pendingApprovals }}</div>
            <div class="stat-label">Pending Approvals</div>
          </div>
        </div>
        <p class="last-updated">Last updated: {{ dashboard.lastUpdated | date:'medium' }}</p>
      }
    </div>

    <div class="card">
      <h2>Reports</h2>
      @if (reportsLoading) {
        <p>Loading reports...</p>
      } @else if (reportsError) {
        <div class="alert alert-error">{{ reportsError }}</div>
      } @else {
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Report Name</th>
              <th>Generated</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            @for (report of reports; track report.id) {
              <tr>
                <td>{{ report.id }}</td>
                <td>{{ report.name }}</td>
                <td>{{ report.generatedAt | date:'mediumDate' }}</td>
                <td><button class="btn btn-primary">View</button></td>
              </tr>
            } @empty {
              <tr>
                <td colspan="4">No reports available.</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>
  `,
  styles: [`
    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 1rem;
      margin-bottom: 1rem;
    }
    .stat-card {
      background: var(--bg-color);
      padding: 1.5rem;
      border-radius: 8px;
      text-align: center;
    }
    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      color: var(--primary-color);
    }
    .stat-label {
      color: #666;
      margin-top: 0.5rem;
    }
    .last-updated {
      color: #666;
      font-size: 0.875rem;
    }
  `]
})
export class ManagerComponent implements OnInit {
  private readonly apiService = inject(ApiService);

  dashboard: DashboardData | null = null;
  dashboardLoading = true;
  dashboardError: string | null = null;

  reports: Report[] = [];
  reportsLoading = true;
  reportsError: string | null = null;

  ngOnInit(): void {
    this.loadDashboard();
    this.loadReports();
  }

  private loadDashboard(): void {
    this.apiService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.dashboardLoading = false;
      },
      error: (err) => {
        this.dashboardError = err.message ?? 'Failed to load dashboard';
        this.dashboardLoading = false;
      }
    });
  }

  private loadReports(): void {
    this.apiService.getReports().subscribe({
      next: (reports) => {
        this.reports = reports;
        this.reportsLoading = false;
      },
      error: (err) => {
        this.reportsError = err.message ?? 'Failed to load reports';
        this.reportsLoading = false;
      }
    });
  }
}
