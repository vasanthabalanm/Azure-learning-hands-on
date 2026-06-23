import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthService, UserProfile } from '@app/core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <h1>User Profile</h1>
      
      @if (loading) {
        <p>Loading profile...</p>
      } @else {
        @if (apiError) {
          <div class="alert alert-warning">
            <strong>Note:</strong> Could not connect to the backend API. Showing token information only.
            <details>
              <summary>Details</summary>
              {{ apiError }}
            </details>
          </div>
        }
        
        @if (profile) {
          <table>
            <tbody>
              <tr>
                <th>Display Name</th>
                <td>{{ profile.displayName }}</td>
              </tr>
              <tr>
                <th>Email</th>
                <td>{{ profile.email }}</td>
              </tr>
              <tr>
                <th>Object ID</th>
                <td><code>{{ profile.objectId }}</code></td>
              </tr>
              <tr>
                <th>Tenant ID</th>
                <td><code>{{ profile.tenantId }}</code></td>
              </tr>
              <tr>
                <th>Roles</th>
                <td>
                  @for (role of profile.roles; track role) {
                    <span class="role-badge role-{{ role.toLowerCase() }}">{{ role }}</span>
                  } @empty {
                    <span class="no-roles">No roles assigned</span>
                  }
                </td>
              </tr>
              @if (profile.firstLogin) {
                <tr>
                  <th>First Login</th>
                  <td>{{ profile.firstLogin | date:'medium' }}</td>
                </tr>
              }
              @if (profile.lastLogin) {
                <tr>
                  <th>Last Login</th>
                  <td>{{ profile.lastLogin | date:'medium' }}</td>
                </tr>
              }
              @if (profile.localProfileId) {
                <tr>
                  <th>Local Profile ID</th>
                  <td>{{ profile.localProfileId }}</td>
                </tr>
              }
            </tbody>
          </table>
        }
        
        <!-- Debug section to verify token claims -->
        <details class="debug-section">
          <summary>🔍 Debug: Token Claims</summary>
          <div class="debug-content">
            <h4>ID Token Claims</h4>
            <pre>{{ idTokenClaims | json }}</pre>
            
            <h4>Access Token Roles</h4>
            <p>
              @if (accessTokenRoles.length > 0) {
                @for (role of accessTokenRoles; track role) {
                  <span class="role-badge role-{{ role.toLowerCase() }}">{{ role }}</span>
                }
              } @else {
                <span class="no-roles">No roles in access token</span>
              }
            </p>
            
            <button class="btn btn-secondary" (click)="refreshToken()">🔄 Fetch Fresh Token</button>
          </div>
        </details>
      }
    </div>
  `,
  styles: [`
    th {
      width: 150px;
      text-align: right;
      padding-right: 1rem;
    }
    code {
      font-family: monospace;
      background: #f0f0f0;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
    }
    .alert-warning {
      background: #fef3c7;
      border: 1px solid #d97706;
      color: #92400e;
      padding: 1rem;
      border-radius: 8px;
      margin-bottom: 1rem;
    }
    details {
      margin-top: 0.5rem;
    }
    summary {
      cursor: pointer;
    }
    .no-roles {
      color: #9ca3af;
      font-style: italic;
    }
    .debug-section {
      margin-top: 2rem;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      padding: 1rem;
    }
    .debug-section summary {
      font-weight: 600;
      color: #64748b;
    }
    .debug-content {
      margin-top: 1rem;
    }
    .debug-content h4 {
      margin: 1rem 0 0.5rem;
      color: #475569;
    }
    .debug-content pre {
      background: #1e293b;
      color: #e2e8f0;
      padding: 1rem;
      border-radius: 6px;
      overflow-x: auto;
      font-size: 0.75rem;
      max-height: 200px;
    }
    .btn-secondary {
      background: #64748b;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      cursor: pointer;
      margin-top: 1rem;
    }
  `]
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);

  profile: UserProfile | null = null;
  loading = true;
  apiError: string | null = null;
  idTokenClaims: Record<string, unknown> = {};
  accessTokenRoles: string[] = [];

  ngOnInit(): void {
    this.loadTokenClaims();
    
    this.authService.getCurrentUserProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.loading = false;
      },
      error: (err) => {
        // Fallback to token-based profile when API fails
        this.apiError = err.message ?? 'Failed to load profile from API';
        this.profile = this.getProfileFromToken();
        this.loading = false;
      }
    });
  }

  /**
   * Load token claims for debugging.
   */
  private loadTokenClaims(): void {
    const account = this.authService.getActiveAccount();
    this.idTokenClaims = (account?.idTokenClaims as Record<string, unknown>) ?? {};
    
    // Fetch access token roles
    this.authService.fetchRolesFromAccessToken().subscribe({
      next: (roles) => {
        this.accessTokenRoles = roles;
      }
    });
  }

  /**
   * Refresh token to get latest roles.
   */
  refreshToken(): void {
    this.authService.fetchRolesFromAccessToken().subscribe({
      next: (roles) => {
        this.accessTokenRoles = roles;
        // Reload the page to refresh all states
        window.location.reload();
      }
    });
  }

  /**
   * Creates a fallback profile from the MSAL token claims.
   */
  private getProfileFromToken(): UserProfile {
    const account = this.authService.getActiveAccount();
    const roles = this.authService.getRolesFromAccount(account);
    
    return {
      displayName: account?.name ?? 'Unknown User',
      email: account?.username ?? '',
      objectId: account?.localAccountId ?? '',
      tenantId: account?.tenantId ?? '',
      roles: roles,
      firstLogin: '',
      lastLogin: '',
      localProfileId: 0
    };
  }
}
