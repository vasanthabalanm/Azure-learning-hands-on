import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ApiService, Task, Notification } from '@app/core/services/api.service';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <h1>User Area</h1>
      <p>This area is accessible by all authenticated users with any role.</p>
    </div>

    <div class="card">
      <h2>My Tasks</h2>
      @if (tasksLoading) {
        <p>Loading tasks...</p>
      } @else if (tasksError) {
        <div class="alert alert-error">{{ tasksError }}</div>
      } @else {
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Title</th>
              <th>Status</th>
              <th>Due Date</th>
            </tr>
          </thead>
          <tbody>
            @for (task of tasks; track task.id) {
              <tr>
                <td>{{ task.id }}</td>
                <td>{{ task.title }}</td>
                <td>
                  <span [class]="'status-' + task.status.toLowerCase()">{{ task.status }}</span>
                </td>
                <td>{{ task.dueDate | date:'mediumDate' }}</td>
              </tr>
            } @empty {
              <tr>
                <td colspan="4">No tasks found.</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </div>

    <div class="card">
      <h2>Notifications</h2>
      @if (notificationsLoading) {
        <p>Loading notifications...</p>
      } @else if (notificationsError) {
        <div class="alert alert-error">{{ notificationsError }}</div>
      } @else {
        <ul class="notifications-list">
          @for (notification of notifications; track notification.id) {
            <li [class.unread]="!notification.read">
              <span class="message">{{ notification.message }}</span>
              <span class="time">{{ notification.createdAt | date:'short' }}</span>
            </li>
          } @empty {
            <li>No notifications.</li>
          }
        </ul>
      }
    </div>
  `,
  styles: [`
    .status-completed { color: var(--success-color); }
    .status-inprogress { color: var(--warning-color); }
    .status-pending { color: var(--primary-color); }
    
    .notifications-list {
      list-style: none;
      padding: 0;
    }
    .notifications-list li {
      padding: 0.75rem;
      border-bottom: 1px solid var(--border-color);
      display: flex;
      justify-content: space-between;
    }
    .notifications-list li.unread {
      background-color: #f0f8ff;
      font-weight: 500;
    }
    .time {
      color: #666;
      font-size: 0.875rem;
    }
  `]
})
export class UserComponent implements OnInit {
  private readonly apiService = inject(ApiService);

  tasks: Task[] = [];
  tasksLoading = true;
  tasksError: string | null = null;

  notifications: Notification[] = [];
  notificationsLoading = true;
  notificationsError: string | null = null;

  ngOnInit(): void {
    this.loadTasks();
    this.loadNotifications();
  }

  private loadTasks(): void {
    this.apiService.getTasks().subscribe({
      next: (tasks) => {
        this.tasks = tasks;
        this.tasksLoading = false;
      },
      error: (err) => {
        this.tasksError = err.message ?? 'Failed to load tasks';
        this.tasksLoading = false;
      }
    });
  }

  private loadNotifications(): void {
    this.apiService.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.notificationsLoading = false;
      },
      error: (err) => {
        this.notificationsError = err.message ?? 'Failed to load notifications';
        this.notificationsLoading = false;
      }
    });
  }
}
