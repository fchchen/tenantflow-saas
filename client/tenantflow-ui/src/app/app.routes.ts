import { Routes } from '@angular/router';
import { HomePage } from './pages/home/home.page';
import { LoginPage } from './pages/login/login.page';
import { AdminDashboardPage } from './pages/admin/admin-dashboard.page';
import { TenantDashboardPage } from './pages/tenant/tenant-dashboard.page';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', component: HomePage },
  { path: 'login', component: LoginPage },
  {
    path: 'admin',
    component: AdminDashboardPage,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['PlatformAdmin'] }
  },
  {
    path: 'app',
    component: TenantDashboardPage,
    canActivate: [authGuard, roleGuard],
    data: { roles: ['TenantAdmin', 'TenantUser', 'PlatformAdmin'] }
  },
  { path: '**', redirectTo: '' }
];
