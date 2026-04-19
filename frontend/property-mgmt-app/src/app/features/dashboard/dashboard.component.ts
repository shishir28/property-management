import { Component, inject, signal, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-dashboard',
  imports: [MatCardModule, MatIconModule, MatButtonModule, MatSnackBarModule],
  template: `
    <h2>Dashboard</h2>
    <div class="cards">
      <mat-card>
        <mat-card-header><mat-icon>apartment</mat-icon><mat-card-title>Properties</mat-card-title></mat-card-header>
        <mat-card-content><p class="stat">{{ stats().properties }}</p></mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-header><mat-icon>people</mat-icon><mat-card-title>Tenants</mat-card-title></mat-card-header>
        <mat-card-content><p class="stat">{{ stats().tenants }}</p></mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-header><mat-icon>description</mat-icon><mat-card-title>Active Leases</mat-card-title></mat-card-header>
        <mat-card-content><p class="stat">{{ stats().leases }}</p></mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-header><mat-icon>warning</mat-icon><mat-card-title>Expiring Soon</mat-card-title></mat-card-header>
        <mat-card-content><p class="stat warn">{{ stats().expiring }}</p></mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-header><mat-icon>build</mat-icon><mat-card-title>Open Maintenance</mat-card-title></mat-card-header>
        <mat-card-content><p class="stat">{{ stats().maintenance }}</p></mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-header><mat-icon>payments</mat-icon><mat-card-title>Overdue Payments</mat-card-title></mat-card-header>
        <mat-card-content><p class="stat warn">{{ stats().overdue }}</p></mat-card-content>
        <mat-card-actions>
          <button mat-button color="primary" (click)="runCollection()">Run Collection Workflow</button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`.cards{display:grid;grid-template-columns:repeat(3,1fr);gap:16px}.stat{font-size:2.5rem;font-weight:700;margin:8px 0}.warn{color:#e53935}`]
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);

  stats = signal({ properties: 0, tenants: 0, leases: 0, expiring: 0, maintenance: 0, overdue: 0 });

  ngOnInit() {
    this.api.getProperties().subscribe(p => this.stats.update(s => ({ ...s, properties: p.length })));
    this.api.getTenants().subscribe(t => this.stats.update(s => ({ ...s, tenants: t.length })));
    this.api.getLeases().subscribe(l => this.stats.update(s => ({ ...s, leases: l.length })));
    this.api.getExpiringLeases().subscribe(l => this.stats.update(s => ({ ...s, expiring: l.length })));
    this.api.getMaintenance().subscribe(m => this.stats.update(s => ({ ...s, maintenance: m.length })));
    this.api.getOverduePayments().subscribe(p => this.stats.update(s => ({ ...s, overdue: p.length })));
  }

  runCollection() {
    this.api.runRentCollection().subscribe(() =>
      this.snack.open('Rent collection workflow triggered', 'OK', { duration: 3000 }));
  }
}
