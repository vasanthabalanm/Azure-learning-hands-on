import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { filter, takeUntil, take } from 'rxjs/operators';
import { MsalService, MsalBroadcastService } from '@azure/msal-angular';
import { EventMessage, EventType, AuthenticationResult, InteractionStatus } from '@azure/msal-browser';

import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  template: `
    <nav>
      <a routerLink="/">Home</a>
      
      @if (isLoggedIn) {
        <!-- Dashboard link based on role -->
        <a [routerLink]="dashboardUrl" class="dashboard-link">
          🏠 My Dashboard
        </a>
        <a routerLink="/profile">Profile</a>
        <a routerLink="/user">User Area</a>
        
        @if (hasRole('Manager') || hasRole('Admin')) {
          <a routerLink="/manager">Manager Area</a>
        }
        
        @if (hasRole('Admin')) {
          <a routerLink="/admin">Admin Area</a>
        }
        
        <span class="nav-right">
          <div class="user-info">
            <span class="user-name">{{ displayName }}</span>
            <div class="user-roles">
              @if (loadingRoles) {
                <span class="loading-roles">Loading...</span>
              } @else if (userRoles.length > 0) {
                @for (role of userRoles; track role) {
                  <span class="role-badge role-{{ role.toLowerCase() }}">{{ role }}</span>
                }
              } @else {
                <span class="no-role">No role assigned</span>
              }
            </div>
          </div>
          <button class="btn btn-danger" (click)="logout()">Sign Out</button>
        </span>
      } @else {
        <span class="nav-right">
          <button class="btn btn-primary" (click)="login()">Sign In with Microsoft</button>
        </span>
      }
    </nav>
    
    <main class="container">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    :host {
      display: block;
    }
    nav {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
    }
    .nav-right {
      margin-left: auto;
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .user-info {
      text-align: right;
    }
    .user-name {
      display: block;
      font-weight: 600;
      color: #1f2937;
    }
    .user-roles {
      display: flex;
      gap: 0.25rem;
      justify-content: flex-end;
      margin-top: 0.125rem;
    }
    .no-role {
      font-size: 0.75rem;
      color: #dc2626;
      font-style: italic;
    }
    .loading-roles {
      font-size: 0.75rem;
      color: #6b7280;
      font-style: italic;
    }
    .dashboard-link {
      background: linear-gradient(90deg, #3b82f6, #8b5cf6);
      color: white !important;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      font-weight: 600;
    }
    .dashboard-link:hover {
      opacity: 0.9;
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  isLoggedIn = false;
  displayName = '';
  userRoles: string[] = [];
  dashboardUrl = '/profile';
  loadingRoles = false;
  
  private readonly destroy$ = new Subject<void>();

  constructor(
    private msalService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Handle redirect callback after login
    this.msalService.handleRedirectObservable().subscribe();

    // Listen for login success events
    this.msalBroadcastService.msalSubject$
      .pipe(
        filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS),
        takeUntil(this.destroy$)
      )
      .subscribe((result: EventMessage) => {
        const payload = result.payload as AuthenticationResult;
        this.msalService.instance.setActiveAccount(payload.account);
        this.displayName = payload.account?.name ?? payload.account?.username ?? 'User';
        this.isLoggedIn = true;
        this.loadingRoles = true;
        
        // Fetch roles from access token FIRST, then navigate
        this.authService.fetchRolesFromAccessToken().subscribe({
          next: (roles) => {
            this.userRoles = roles;
            this.dashboardUrl = this.authService.getDashboardUrl();
            this.loadingRoles = false;
            
            // Now navigate to the correct dashboard
            this.authService.navigateToDashboard();
          },
          error: () => {
            this.userRoles = [];
            this.dashboardUrl = '/no-role';
            this.loadingRoles = false;
            this.router.navigate(['/no-role']);
          }
        });
      });

    // Wait for MSAL to be fully initialized before updating login state
    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        take(1),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateLoginState();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  login(): void {
    this.msalService.loginRedirect();
  }

  logout(): void {
    this.msalService.logoutRedirect();
  }

  hasRole(role: string): boolean {
    return this.userRoles.includes(role);
  }

  private updateLoginState(): void {
    const accounts = this.msalService.instance.getAllAccounts();
    this.isLoggedIn = accounts.length > 0;
    
    if (this.isLoggedIn) {
      const account = accounts[0];
      this.displayName = account.name ?? account.username ?? 'User';
      
      // First try ID token roles, then fetch from access token
      const idTokenRoles = this.authService.getRolesFromAccount(account);
      
      if (idTokenRoles.length > 0) {
        this.userRoles = idTokenRoles;
        this.dashboardUrl = this.authService.getDashboardUrl();
        this.loadingRoles = false;
      } else {
        // Show loading while fetching roles from access token
        this.loadingRoles = true;
        
        // Fetch roles from access token (for API-assigned roles)
        this.authService.fetchRolesFromAccessToken().subscribe({
          next: (roles) => {
            this.userRoles = roles;
            this.dashboardUrl = this.authService.getDashboardUrl();
            this.loadingRoles = false;
          },
          error: () => {
            this.userRoles = [];
            this.dashboardUrl = '/no-role';
            this.loadingRoles = false;
          }
        });
      }
    } else {
      this.displayName = '';
      this.userRoles = [];
      this.dashboardUrl = '/profile';
      this.loadingRoles = false;
    }
  }
}
