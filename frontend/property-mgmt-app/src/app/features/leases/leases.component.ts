import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Lease } from '../../core/models/models';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-leases',
  imports: [FormsModule, MatTableModule, MatButtonModule, MatInputModule, MatFormFieldModule,
    MatCardModule, MatTabsModule, MatChipsModule, MatSnackBarModule, MatIconModule],
  template: `
    <h2>Leases</h2>
    <mat-tab-group>
      <mat-tab label="All Leases">
        <mat-card class="form-card">
          <mat-card-content>
            <mat-form-field><mat-label>Property ID</mat-label><input matInput [(ngModel)]="form.propertyId" /></mat-form-field>
            <mat-form-field><mat-label>Tenant ID</mat-label><input matInput [(ngModel)]="form.tenantId" /></mat-form-field>
            <mat-form-field><mat-label>Unit #</mat-label><input matInput [(ngModel)]="form.unitNumber" /></mat-form-field>
            <mat-form-field><mat-label>Start Date</mat-label><input matInput type="date" [(ngModel)]="form.startDate" /></mat-form-field>
            <mat-form-field><mat-label>End Date</mat-label><input matInput type="date" [(ngModel)]="form.endDate" /></mat-form-field>
            <mat-form-field><mat-label>Monthly Rent</mat-label><input matInput type="number" [(ngModel)]="form.monthlyRent" /></mat-form-field>
            <mat-form-field><mat-label>Security Deposit</mat-label><input matInput type="number" [(ngModel)]="form.securityDeposit" /></mat-form-field>
            <button mat-raised-button color="primary" (click)="add()">Create Lease</button>
          </mat-card-content>
        </mat-card>
        <table mat-table [dataSource]="leases()" class="mat-elevation-z2 full-width">
          <ng-container matColumnDef="unit"><th mat-header-cell *matHeaderCellDef>Unit</th><td mat-cell *matCellDef="let l">{{l.unitNumber}}</td></ng-container>
          <ng-container matColumnDef="rent"><th mat-header-cell *matHeaderCellDef>Rent</th><td mat-cell *matCellDef="let l">\${{l.monthlyRent}}</td></ng-container>
          <ng-container matColumnDef="end"><th mat-header-cell *matHeaderCellDef>End Date</th><td mat-cell *matCellDef="let l">{{l.endDate}}</td></ng-container>
          <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let l"><mat-chip>{{l.status}}</mat-chip></td></ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let l">
              @if (l.status === 'Draft') { <button mat-icon-button color="primary" title="Activate" (click)="activate(l.id)"><mat-icon>check_circle</mat-icon></button> }
              @if (l.status === 'Active') { <button mat-icon-button color="accent" title="Trigger Renewal" (click)="triggerRenewal(l.id)"><mat-icon>autorenew</mat-icon></button> }
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols;"></tr>
        </table>
        <div class="pager">
          <span>Page {{ leasePage() }} · {{ leaseTotalCount() }} total</span>
          <div class="pager-actions">
            <button mat-stroked-button type="button" [disabled]="leasePage() === 1" (click)="previousLeasePage()">Previous</button>
            <button mat-stroked-button type="button" [disabled]="leasePage() * pageSize >= leaseTotalCount()" (click)="nextLeasePage()">Next</button>
          </div>
        </div>
      </mat-tab>

      <mat-tab label="Expiring Soon">
        <table mat-table [dataSource]="expiring()" class="mat-elevation-z2 full-width" style="margin-top:16px">
          <ng-container matColumnDef="unit"><th mat-header-cell *matHeaderCellDef>Unit</th><td mat-cell *matCellDef="let l">{{l.unitNumber}}</td></ng-container>
          <ng-container matColumnDef="rent"><th mat-header-cell *matHeaderCellDef>Rent</th><td mat-cell *matCellDef="let l">\${{l.monthlyRent}}</td></ng-container>
          <ng-container matColumnDef="end"><th mat-header-cell *matHeaderCellDef>End Date</th><td mat-cell *matCellDef="let l">{{l.endDate}}</td></ng-container>
          <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let l"><mat-chip color="warn">{{l.status}}</mat-chip></td></ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let l">
              <button mat-button color="warn" (click)="triggerRenewal(l.id)">Trigger Renewal</button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols;"></tr>
        </table>
        <div class="pager">
          <span>Page {{ expiringPage() }} · {{ expiringTotalCount() }} total</span>
          <div class="pager-actions">
            <button mat-stroked-button type="button" [disabled]="expiringPage() === 1" (click)="previousExpiringPage()">Previous</button>
            <button mat-stroked-button type="button" [disabled]="expiringPage() * pageSize >= expiringTotalCount()" (click)="nextExpiringPage()">Next</button>
          </div>
        </div>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`.form-card{margin:16px 0}mat-form-field{margin-right:8px}.full-width{width:100%}.pager{display:flex;justify-content:space-between;align-items:center;margin-top:12px}.pager-actions{display:flex;gap:8px}`]
})
export class LeasesComponent implements OnInit {
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);
  leases = signal<Lease[]>([]);
  expiring = signal<Lease[]>([]);
  leaseTotalCount = signal(0);
  expiringTotalCount = signal(0);
  leasePage = signal(1);
  expiringPage = signal(1);
  readonly pageSize = 10;
  cols = ['unit', 'rent', 'end', 'status', 'actions'];
  form = { propertyId: '', tenantId: '', unitNumber: '', startDate: '', endDate: '', monthlyRent: 0, securityDeposit: 0 };

  ngOnInit() {
    this.loadLeases();
    this.loadExpiring();
  }

  add() {
    this.api.createLease(this.form).subscribe(() => {
      this.snack.open('Lease created', 'OK', { duration: 2000 });
      this.leasePage.set(1);
      this.loadLeases();
      this.loadExpiring();
    });
  }

  activate(id: string) {
    this.api.activateLease(id).subscribe(() => {
      this.snack.open('Lease activated — onboarding workflow triggered', 'OK', { duration: 3000 });
      this.loadLeases();
      this.loadExpiring();
    });
  }

  triggerRenewal(id: string) {
    this.api.triggerRenewal(id).subscribe(() =>
      this.snack.open('Renewal workflow triggered', 'OK', { duration: 2000 }));
  }

  nextLeasePage() {
    if (this.leasePage() * this.pageSize >= this.leaseTotalCount()) {
      return;
    }

    this.leasePage.update(page => page + 1);
    this.loadLeases();
  }

  previousLeasePage() {
    if (this.leasePage() === 1) {
      return;
    }

    this.leasePage.update(page => page - 1);
    this.loadLeases();
  }

  nextExpiringPage() {
    if (this.expiringPage() * this.pageSize >= this.expiringTotalCount()) {
      return;
    }

    this.expiringPage.update(page => page + 1);
    this.loadExpiring();
  }

  previousExpiringPage() {
    if (this.expiringPage() === 1) {
      return;
    }

    this.expiringPage.update(page => page - 1);
    this.loadExpiring();
  }

  private loadLeases() {
    this.api.getLeasesPage(this.leasePage(), this.pageSize).subscribe(response => {
      this.leases.set(response.items);
      this.leaseTotalCount.set(response.totalCount);
    });
  }

  private loadExpiring() {
    this.api.getExpiringLeasesPage(this.expiringPage(), this.pageSize).subscribe(response => {
      this.expiring.set(response.items);
      this.expiringTotalCount.set(response.totalCount);
    });
  }
}
