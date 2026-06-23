import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { RoleGuard } from './core/guards/role.guard';
import { NoRoleGuard } from './core/guards/no-role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent)
  },
  // Role-specific dashboards (landing pages after login)
  {
    path: 'dashboard/admin',
    loadComponent: () => import('./pages/dashboard/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [MsalGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'dashboard/manager',
    loadComponent: () => import('./pages/dashboard/manager-dashboard/manager-dashboard.component').then(m => m.ManagerDashboardComponent),
    canActivate: [MsalGuard, RoleGuard],
    data: { roles: ['Admin', 'Manager'] }
  },
  {
    path: 'dashboard/user',
    loadComponent: () => import('./pages/dashboard/user-dashboard/user-dashboard.component').then(m => m.UserDashboardComponent),
    canActivate: [MsalGuard, RoleGuard],
    data: { roles: ['Admin', 'Manager', 'User'] }
  },
  // No role assigned page - only accessible to users WITHOUT roles
  {
    path: 'no-role',
    loadComponent: () => import('./pages/no-role/no-role.component').then(m => m.NoRoleComponent),
    canActivate: [MsalGuard, NoRoleGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [MsalGuard]
  },
  {
    path: 'user',
    loadComponent: () => import('./pages/user/user.component').then(m => m.UserComponent),
    canActivate: [MsalGuard, RoleGuard],
    data: { roles: ['Admin', 'Manager', 'User'] }
  },
  {
    path: 'manager',
    loadComponent: () => import('./pages/manager/manager.component').then(m => m.ManagerComponent),
    canActivate: [MsalGuard, RoleGuard],
    data: { roles: ['Admin', 'Manager'] }
  },
  {
    path: 'admin',
    loadComponent: () => import('./pages/admin/admin.component').then(m => m.AdminComponent),
    canActivate: [MsalGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./pages/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
