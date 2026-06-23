import { Injectable, inject } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable, map, of, filter, switchMap, take } from 'rxjs';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';

import { AuthService } from '../services/auth.service';

/**
 * Route guard that checks if the user has required roles.
 * Waits for MSAL to initialize before checking roles.
 * 
 * Usage in routes:
 * ```
 * {
 *   path: 'admin',
 *   component: AdminComponent,
 *   canActivate: [MsalGuard, RoleGuard],
 *   data: { roles: ['Admin'] }
 * }
 * ```
 */
@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly msalBroadcastService = inject(MsalBroadcastService);
  private readonly msalService = inject(MsalService);

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    const requiredRoles = route.data['roles'] as string[] | undefined;

    if (!requiredRoles || requiredRoles.length === 0) {
      // No roles required, allow access
      return of(true);
    }

    // Wait for MSAL to complete any interaction and be ready
    return this.msalBroadcastService.inProgress$.pipe(
      filter((status: InteractionStatus) => status === InteractionStatus.None),
      take(1),
      switchMap(() => {
        // Now MSAL is ready, fetch roles from access token
        return this.authService.fetchRolesFromAccessToken().pipe(
          map(userRoles => {
            const hasRequiredRole = requiredRoles.some(role => userRoles.includes(role));
            
            if (!hasRequiredRole) {
              this.router.navigate(['/unauthorized']);
              return false;
            }

            return true;
          })
        );
      })
    );
  }
}
