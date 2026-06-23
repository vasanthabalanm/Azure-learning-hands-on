import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '@app/core/services/auth.service';
import { ApiService, Task, Notification } from '@app/core/services/api.service';

/**
 * User Dashboard - Landing page for User role users.
 * Shows personal tasks, notifications, and quick actions.
 */
@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard-header user-header">
      <div class="header-content">
        <div class="welcome-section">
          <span class="role-indicator">USER</span>
          <h1>Welcome back, {{ displayName }}</h1>
          <p class="subtitle">Here's your overview for today</p>
        </div>
        <div class="header-date">
          <span class="date-day">{{ today | date:'EEEE' }}</span>
          <span class="date-full">{{ today | date:'MMMM d, yyyy' }}</span>
        </div>
      </div>
    </div>

    <div class="dashboard-content">
      <!-- Quick Stats Row -->
      <div class="stats-row">
        <div class="stat-card">
          <div class="stat-icon pending">📋</div>
          <div class="stat-info">
            <span class="stat-value">{{ pendingTasks }}</span>
            <span class="stat-label">Pending Tasks</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon completed">✅</div>
          <div class="stat-info">
            <span class="stat-value">{{ completedTasks }}</span>
            <span class="stat-label">Completed This Week</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon notifications">🔔</div>
          <div class="stat-info">
            <span class="stat-value">{{ unreadNotifications }}</span>
            <span class="stat-label">New Notifications</span>
          </div>
        </div>
      </div>

      <div class="dashboard-grid">
        <!-- Today's Tasks Card -->
        <div class="card tasks-card">
          <div class="card-header">
            <h2>📝 Today's Tasks</h2>
            <button class="add-btn" (click)="addTask()">+ Add Task</button>
          </div>
          @if (tasksLoading) {
            <div class="loading">Loading tasks...</div>
          } @else {
            <ul class="tasks-list">
              @for (task of tasks; track task.id) {
                <li class="task-item" [class.completed]="task.status === 'Completed'">
                  <label class="task-checkbox">
                    <input 
                      type="checkbox" 
                      [checked]="task.status === 'Completed'"
                      (change)="toggleTask(task)"
                    >
                    <span class="checkmark"></span>
                  </label>
                  <div class="task-content">
                    <span class="task-title">{{ task.title }}</span>
                    <span class="task-due">
                      <span class="due-icon">📅</span>
                      {{ task.dueDate | date:'MMM d' }}
                    </span>
                  </div>
                  <span class="task-priority" [class]="'priority-' + getPriority(task)">
                    {{ getPriority(task) }}
                  </span>
                </li>
              } @empty {
                <li class="no-tasks">
                  <span class="empty-icon">🎉</span>
                  <span>No tasks for today! Enjoy your day.</span>
                </li>
              }
            </ul>
          }
          <a routerLink="/user" class="view-all-link">View All Tasks →</a>
        </div>

        <!-- Notifications Card -->
        <div class="card notifications-card">
          <div class="card-header">
            <h2>🔔 Notifications</h2>
            @if (unreadNotifications > 0) {
              <button class="mark-read-btn" (click)="markAllRead()">Mark all read</button>
            }
          </div>
          @if (notificationsLoading) {
            <div class="loading">Loading notifications...</div>
          } @else {
            <ul class="notifications-list">
              @for (notification of notifications; track notification.id) {
                <li class="notification-item" [class.unread]="!notification.read">
                  <div class="notification-icon" [ngClass]="getNotificationIcon(notification.message)">
                    {{ getNotificationEmoji(notification.message) }}
                  </div>
                  <div class="notification-content">
                    <span class="notification-message">{{ notification.message }}</span>
                    <span class="notification-time">{{ notification.createdAt | date:'short' }}</span>
                  </div>
                </li>
              } @empty {
                <li class="no-notifications">No new notifications</li>
              }
            </ul>
          }
        </div>

        <!-- Quick Actions Card -->
        <div class="card quick-actions">
          <h2>⚡ Quick Actions</h2>
          <div class="actions-grid">
            <button class="action-item" (click)="requestLeave()">
              <span class="action-icon">🏖️</span>
              <span class="action-label">Request Leave</span>
            </button>
            <button class="action-item" (click)="submitExpense()">
              <span class="action-icon">💰</span>
              <span class="action-label">Submit Expense</span>
            </button>
            <button class="action-item" (click)="bookRoom()">
              <span class="action-icon">🏢</span>
              <span class="action-label">Book Room</span>
            </button>
            <button class="action-item" (click)="askHelp()">
              <span class="action-icon">❓</span>
              <span class="action-label">Get Help</span>
            </button>
          </div>
        </div>

        <!-- Progress Card -->
        <div class="card progress-card">
          <h2>📈 Your Progress</h2>
          <div class="progress-content">
            <div class="progress-circle">
              <svg viewBox="0 0 36 36">
                <path
                  class="progress-bg"
                  d="M18 2.0845
                    a 15.9155 15.9155 0 0 1 0 31.831
                    a 15.9155 15.9155 0 0 1 0 -31.831"
                />
                <path
                  class="progress-fill"
                  [style.stroke-dasharray]="progressPercent + ', 100'"
                  d="M18 2.0845
                    a 15.9155 15.9155 0 0 1 0 31.831
                    a 15.9155 15.9155 0 0 1 0 -31.831"
                />
                <text x="18" y="20.35" class="progress-text">{{ progressPercent }}%</text>
              </svg>
            </div>
            <div class="progress-details">
              <div class="progress-item">
                <span class="label">Weekly Goal</span>
                <span class="value">{{ completedTasks }}/{{ weeklyGoal }} tasks</span>
              </div>
              <div class="progress-item">
                <span class="label">Streak</span>
                <span class="value">🔥 {{ streak }} days</span>
              </div>
              <div class="progress-item">
                <span class="label">Points Earned</span>
                <span class="value">⭐ {{ points }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Navigation Card -->
      <div class="nav-cards">
        <a routerLink="/user" class="nav-card">
          <span class="nav-icon">📋</span>
          <div class="nav-info">
            <h3>All Tasks</h3>
            <p>View and manage all your tasks</p>
          </div>
          <span class="nav-arrow">→</span>
        </a>
        <a routerLink="/profile" class="nav-card">
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
      background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
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

    .header-date {
      text-align: right;
    }

    .date-day {
      display: block;
      font-size: 1.5rem;
      font-weight: 700;
    }

    .date-full {
      opacity: 0.9;
    }

    .stats-row {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .stat-card {
      background: white;
      padding: 1.25rem;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .stat-icon {
      width: 50px;
      height: 50px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
    }

    .stat-icon.pending { background: #fef3c7; }
    .stat-icon.completed { background: #dcfce7; }
    .stat-icon.notifications { background: #dbeafe; }

    .stat-info {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: #1f2937;
    }

    .stat-label {
      font-size: 0.875rem;
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

    .add-btn {
      background: #2563eb;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.875rem;
    }

    .mark-read-btn {
      background: none;
      color: #2563eb;
      border: none;
      cursor: pointer;
      font-size: 0.875rem;
    }

    .tasks-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .task-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid #f3f4f6;
    }

    .task-item.completed .task-title {
      text-decoration: line-through;
      color: #9ca3af;
    }

    .task-checkbox {
      position: relative;
      cursor: pointer;
    }

    .task-checkbox input {
      opacity: 0;
      position: absolute;
    }

    .checkmark {
      width: 20px;
      height: 20px;
      border: 2px solid #d1d5db;
      border-radius: 4px;
      display: block;
    }

    .task-checkbox input:checked + .checkmark {
      background: #2563eb;
      border-color: #2563eb;
    }

    .task-checkbox input:checked + .checkmark::after {
      content: '✓';
      color: white;
      position: absolute;
      top: -1px;
      left: 3px;
      font-size: 0.875rem;
    }

    .task-content {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .task-title {
      font-weight: 500;
      color: #374151;
    }

    .task-due {
      font-size: 0.75rem;
      color: #9ca3af;
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .task-priority {
      font-size: 0.75rem;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-weight: 500;
    }

    .priority-high {
      background: #fecaca;
      color: #dc2626;
    }

    .priority-medium {
      background: #fef3c7;
      color: #d97706;
    }

    .priority-low {
      background: #dbeafe;
      color: #2563eb;
    }

    .no-tasks {
      text-align: center;
      padding: 2rem;
      color: #9ca3af;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
    }

    .empty-icon {
      font-size: 2rem;
    }

    .view-all-link {
      display: block;
      text-align: center;
      color: #2563eb;
      font-weight: 500;
      text-decoration: none;
      margin-top: 1rem;
    }

    .notifications-list {
      list-style: none;
      padding: 0;
      margin: 0;
      max-height: 300px;
      overflow-y: auto;
    }

    .notification-item {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid #f3f4f6;
    }

    .notification-item.unread {
      background: #eff6ff;
      margin: 0 -1.5rem;
      padding: 0.75rem 1.5rem;
    }

    .notification-icon {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: #f3f4f6;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .notification-content {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .notification-message {
      color: #374151;
    }

    .notification-time {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .actions-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 0.75rem;
    }

    .action-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem;
      background: #f3f4f6;
      border: none;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .action-item:hover {
      background: #e5e7eb;
      transform: translateY(-2px);
    }

    .action-icon {
      font-size: 1.5rem;
    }

    .action-label {
      font-size: 0.875rem;
      color: #374151;
    }

    .progress-content {
      display: flex;
      align-items: center;
      gap: 2rem;
    }

    .progress-circle {
      width: 120px;
      height: 120px;
    }

    .progress-circle svg {
      width: 100%;
      height: 100%;
    }

    .progress-bg {
      fill: none;
      stroke: #e5e7eb;
      stroke-width: 3;
    }

    .progress-fill {
      fill: none;
      stroke: #2563eb;
      stroke-width: 3;
      stroke-linecap: round;
      transform: rotate(-90deg);
      transform-origin: 50% 50%;
    }

    .progress-text {
      fill: #374151;
      font-size: 0.5rem;
      font-weight: 700;
      text-anchor: middle;
    }

    .progress-details {
      flex: 1;
    }

    .progress-item {
      display: flex;
      justify-content: space-between;
      padding: 0.5rem 0;
      border-bottom: 1px solid #f3f4f6;
    }

    .progress-item:last-child {
      border-bottom: none;
    }

    .progress-item .label {
      color: #6b7280;
    }

    .progress-item .value {
      font-weight: 600;
      color: #374151;
    }

    .nav-cards {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
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
      border-left: 4px solid #2563eb;
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

    .loading, .no-notifications {
      text-align: center;
      padding: 1rem;
      color: #9ca3af;
    }

    @media (max-width: 768px) {
      .header-content {
        flex-direction: column;
        text-align: center;
      }

      .header-date {
        text-align: center;
      }

      .stats-row {
        grid-template-columns: 1fr;
      }

      .dashboard-grid {
        grid-template-columns: 1fr;
      }

      .nav-cards {
        grid-template-columns: 1fr;
      }

      .progress-content {
        flex-direction: column;
      }
    }
  `]
})
export class UserDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly apiService = inject(ApiService);

  displayName = '';
  today = new Date();
  tasks: Task[] = [];
  notifications: Notification[] = [];
  tasksLoading = true;
  notificationsLoading = true;

  pendingTasks = 0;
  completedTasks = 0;
  unreadNotifications = 0;
  weeklyGoal = 10;
  streak = 5;
  points = 250;
  progressPercent = 0;

  ngOnInit(): void {
    const account = this.authService.getActiveAccount();
    this.displayName = account?.name ?? 'User';

    this.loadTasks();
    this.loadNotifications();
  }

  private loadTasks(): void {
    this.apiService.getMyTasks().subscribe({
      next: (tasks) => {
        this.tasks = tasks.slice(0, 5);
        this.pendingTasks = tasks.filter(t => t.status !== 'Completed').length;
        this.completedTasks = tasks.filter(t => t.status === 'Completed').length;
        this.progressPercent = Math.round((this.completedTasks / this.weeklyGoal) * 100);
        this.tasksLoading = false;
      },
      error: () => {
        // Fallback demo data when API is unavailable
        this.tasks = [
          { id: 1, title: 'Review project proposal', status: 'Pending', dueDate: new Date().toISOString() },
          { id: 2, title: 'Update documentation', status: 'InProgress', dueDate: new Date(Date.now() + 86400000).toISOString() },
          { id: 3, title: 'Team standup meeting', status: 'Completed', dueDate: new Date().toISOString() },
          { id: 4, title: 'Code review for PR #42', status: 'Pending', dueDate: new Date(Date.now() + 172800000).toISOString() }
        ];
        this.pendingTasks = this.tasks.filter(t => t.status !== 'Completed').length;
        this.completedTasks = this.tasks.filter(t => t.status === 'Completed').length;
        this.progressPercent = Math.round((this.completedTasks / this.weeklyGoal) * 100);
        this.tasksLoading = false;
      }
    });
  }

  private loadNotifications(): void {
    this.apiService.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications.slice(0, 5);
        this.unreadNotifications = notifications.filter(n => !n.read).length;
        this.notificationsLoading = false;
      },
      error: () => {
        // Fallback demo data when API is unavailable
        this.notifications = [
          { id: 1, message: 'New task assigned to you', read: false, createdAt: new Date().toISOString() },
          { id: 2, message: 'Your leave request was approved', read: false, createdAt: new Date(Date.now() - 3600000).toISOString() },
          { id: 3, message: 'Comment on your PR #38', read: true, createdAt: new Date(Date.now() - 86400000).toISOString() }
        ];
        this.unreadNotifications = this.notifications.filter(n => !n.read).length;
        this.notificationsLoading = false;
      }
    });
  }

  getPriority(task: Task): string {
    // Simple priority based on due date
    const due = new Date(task.dueDate);
    const today = new Date();
    const diff = Math.ceil((due.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    
    if (diff <= 1) return 'high';
    if (diff <= 3) return 'medium';
    return 'low';
  }

  toggleTask(task: Task): void {
    task.status = task.status === 'Completed' ? 'Pending' : 'Completed';
    if (task.status === 'Completed') {
      this.completedTasks++;
      this.pendingTasks--;
    } else {
      this.completedTasks--;
      this.pendingTasks++;
    }
    this.progressPercent = Math.round((this.completedTasks / this.weeklyGoal) * 100);
  }

  getNotificationIcon(message: string): string {
    if (message.includes('assigned')) return 'task';
    if (message.includes('comment')) return 'comment';
    return 'default';
  }

  getNotificationEmoji(message: string): string {
    if (message.includes('assigned')) return '📋';
    if (message.includes('comment')) return '💬';
    if (message.includes('approved')) return '✅';
    if (message.includes('deadline')) return '⏰';
    return '📌';
  }

  markAllRead(): void {
    this.notifications.forEach(n => n.read = true);
    this.unreadNotifications = 0;
  }

  addTask(): void {
    alert('Add task dialog would open here');
  }

  requestLeave(): void {
    alert('Leave request form would open here');
  }

  submitExpense(): void {
    alert('Expense submission form would open here');
  }

  bookRoom(): void {
    alert('Room booking would open here');
  }

  askHelp(): void {
    alert('Help center would open here');
  }
}
