import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule, MatIconModule, MatButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  navItems = [
    { label: 'Dashboard',   icon: 'dashboard',     route: '/dashboard' },
    { label: 'Properties',  icon: 'apartment',     route: '/properties' },
    { label: 'Tenants',     icon: 'people',        route: '/tenants' },
    { label: 'Leases',      icon: 'description',   route: '/leases' },
    { label: 'Maintenance', icon: 'build',         route: '/maintenance' },
    { label: 'Payments',    icon: 'payments',      route: '/payments' },
  ];
}
