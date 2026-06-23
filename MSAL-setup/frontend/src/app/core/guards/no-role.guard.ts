import { Injectable, inject } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, map, of, filter, switchMap, take } from 'rxjs';
import { MsalBroadcastService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';

import { AuthService } from '../services/auth.service';

/**
 * Guard for the /no-role page.
 * Redirects users WHO HAVE roles to their appropriate dashboard.
 * Only allows access to users with NO roles assigned.
 */
@Injectable({ providedIn: 'root' })
export class NoRoleGuard implements CanActivate {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly msalBroadcastService = inject(MsalBroadcastService);

  canActivate(): Observable<boolean> {
    // Wait for MSAL to complete any interaction and be ready
    return this.msalBroadcastService.inProgress$.pipe(
      filter((status: InteractionStatus) => status === InteractionStatus.None),
      take(1),
      switchMap(() => {
        // Fetch roles from access token
        return this.authService.fetchRolesFromAccessToken().pipe(
          map(userRoles => {
            // If user HAS roles, redirect them to their dashboard
            if (userRoles.length > 0) {
              const dashboardUrl = this.authService.getDashboardUrl();
              this.router.navigate([dashboardUrl]);
              return false;
            }

            // User has no roles - allow access to /no-role page
            return true;
          })
        );
      })
    );
  }
}
