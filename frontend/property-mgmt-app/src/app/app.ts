import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  navItems = [
    { label: 'Dashboard',   icon: 'dashboard',     route: '/dashboard' },
    { label: 'Workflows',   icon: 'hub',           route: '/workflows' },
    { label: 'Properties',  icon: 'apartment',     route: '/properties' },
    { label: 'Tenants',     icon: 'people',        route: '/tenants' },
    { label: 'Leases',      icon: 'description',   route: '/leases' },
    { label: 'Maintenance', icon: 'build',         route: '/maintenance' },
    { label: 'Payments',    icon: 'payments',      route: '/payments' },
  ];

  readonly isAuthenticated = this.auth.isAuthenticated;
  readonly displayName = this.auth.displayName;
  readonly showShell = computed(() => this.isAuthenticated() && this.router.url !== '/login');

  logout() {
    this.auth.logout();
  }
}
