import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/services/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent) },
  { path: 'dashboard', canActivate: [authGuard], loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'workflows', canActivate: [authGuard], loadComponent: () => import('./features/workflows/workflows.component').then(m => m.WorkflowsComponent) },
  { path: 'properties', canActivate: [authGuard], loadComponent: () => import('./features/properties/properties.component').then(m => m.PropertiesComponent) },
  { path: 'tenants', canActivate: [authGuard], loadComponent: () => import('./features/tenants/tenants.component').then(m => m.TenantsComponent) },
  { path: 'leases', canActivate: [authGuard], loadComponent: () => import('./features/leases/leases.component').then(m => m.LeasesComponent) },
  { path: 'maintenance', canActivate: [authGuard], loadComponent: () => import('./features/maintenance/maintenance.component').then(m => m.MaintenanceComponent) },
  { path: 'payments', canActivate: [authGuard], loadComponent: () => import('./features/payments/payments.component').then(m => m.PaymentsComponent) },
  { path: '**', redirectTo: 'dashboard' }
];
