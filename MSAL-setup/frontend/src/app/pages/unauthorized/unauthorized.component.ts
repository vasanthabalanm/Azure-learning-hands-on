import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="card unauthorized">
      <h1>🚫 Access Denied</h1>
      <p>You do not have permission to access this page.</p>
      <p>If you believe this is an error, contact your administrator to request the appropriate role.</p>
      <a routerLink="/" class="btn btn-primary">Return to Home</a>
    </div>
  `,
  styles: [`
    .unauthorized {
      text-align: center;
      padding: 3rem;
    }
    h1 {
      color: var(--danger-color);
    }
    p {
      margin-bottom: 1rem;
      color: #666;
    }
  `]
})
export class UnauthorizedComponent {}
