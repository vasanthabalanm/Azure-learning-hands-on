import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, from, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { MsalService } from '@azure/msal-angular';
import { AccountInfo, SilentRequest } from '@azure/msal-browser';

import { environment } from '@env/environment';

export interface UserProfile {
  objectId: string;
  tenantId: string;
  email: string;
  displayName: string;
  roles: string[];
  localProfileId: number;
  firstLogin: string;
  lastLogin: string;
}

/**
 * Authentication service for MSAL operations and user info.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly msalService = inject(MsalService);
  private readonly router = inject(Router);

  // Cache for roles extracted from access token
  private cachedRoles: string[] | null = null;

  /**
   * Get the currently active account.
   */
  getActiveAccount(): AccountInfo | null {
    return this.msalService.instance.getActiveAccount();
  }

  /**
   * Extract roles from the account's ID token claims.
   * Roles are defined as App Roles in Azure AD.
   */
  getRolesFromAccount(account: AccountInfo | null): string[] {
    // First check cached roles (from access token)
    if (this.cachedRoles && this.cachedRoles.length > 0) {
      return this.cachedRoles;
    }

    if (!account?.idTokenClaims) {
      return [];
    }

    const claims = account.idTokenClaims as Record<string, unknown>;
    const roles = claims['roles'];

    if (Array.isArray(roles)) {
      return roles as string[];
    }

    return [];
  }

  /**
   * Fetch roles from the access token (for API-assigned roles).
   * Call this after login to populate the role cache.
   */
  fetchRolesFromAccessToken(): Observable<string[]> {
    const account = this.getActiveAccount();
    if (!account) {
      return of([]);
    }

    const request: SilentRequest = {
      account: account,
      scopes: environment.apiConfig.scopes
    };

    return from(this.msalService.instance.acquireTokenSilent(request)).pipe(
      map(response => {
        const roles = this.decodeAccessTokenRoles(response.accessToken);
        this.cachedRoles = roles;
        return roles;
      }),
      catchError(() => {
        // Fall back to ID token roles
        return of(this.getRolesFromAccount(account));
      })
    );
  }

  /**
   * Decode JWT access token and extract roles claim.
   */
  private decodeAccessTokenRoles(accessToken: string): string[] {
    try {
      const parts = accessToken.split('.');
      if (parts.length !== 3) {
        return [];
      }

      // Decode the payload (second part)
      const payload = JSON.parse(atob(parts[1]));
      const roles = payload.roles;

      if (Array.isArray(roles)) {
        return roles as string[];
      }
      return [];
    } catch {
      return [];
    }
  }

  /**
   * Check if the current user has a specific role.
   */
  hasRole(role: string): boolean {
    const account = this.getActiveAccount();
    const roles = this.getRolesFromAccount(account);
    return roles.includes(role);
  }

  /**
   * Check if user has any of the specified roles.
   */
  hasAnyRole(requiredRoles: string[]): boolean {
    const account = this.getActiveAccount();
    const userRoles = this.getRolesFromAccount(account);
    return requiredRoles.some(role => userRoles.includes(role));
  }

  /**
   * Get the highest role of the current user.
   * Priority: Admin > Manager > User
   */
  getHighestRole(): string | null {
    const account = this.getActiveAccount();
    const roles = this.getRolesFromAccount(account);

    if (roles.includes('Admin')) return 'Admin';
    if (roles.includes('Manager')) return 'Manager';
    if (roles.includes('User')) return 'User';
    
    return null;
  }

  /**
   * Get the dashboard URL based on the user's highest role.
   */
  getDashboardUrl(): string {
    const role = this.getHighestRole();
    
    switch (role) {
      case 'Admin':
        return '/dashboard/admin';
      case 'Manager':
        return '/dashboard/manager';
      case 'User':
        return '/dashboard/user';
      default:
        return '/no-role';
    }
  }

  /**
   * Navigate to the appropriate dashboard based on user role.
   */
  navigateToDashboard(): void {
    const url = this.getDashboardUrl();
    this.router.navigate([url]);
  }

  /**
   * Fetch current user profile from the backend API.
   */
  getCurrentUserProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${environment.apiConfig.uri}/me`);
  }

  /**
   * Check if user is authenticated.
   */
  isAuthenticated(): boolean {
    return this.msalService.instance.getAllAccounts().length > 0;
  }
}
