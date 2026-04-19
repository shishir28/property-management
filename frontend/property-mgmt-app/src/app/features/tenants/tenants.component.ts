import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Tenant } from '../../core/models/models';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-tenants',
  imports: [FormsModule, MatTableModule, MatButtonModule, MatInputModule, MatFormFieldModule, MatCardModule, MatChipsModule, MatSnackBarModule],
  template: `
    <h2>Tenants</h2>
    <mat-card class="form-card">
      <mat-card-content>
        <mat-form-field><mat-label>First Name</mat-label><input matInput [(ngModel)]="form.firstName" /></mat-form-field>
        <mat-form-field><mat-label>Last Name</mat-label><input matInput [(ngModel)]="form.lastName" /></mat-form-field>
        <mat-form-field><mat-label>Email</mat-label><input matInput [(ngModel)]="form.email" /></mat-form-field>
        <mat-form-field><mat-label>Phone</mat-label><input matInput [(ngModel)]="form.phone" /></mat-form-field>
        <button mat-raised-button color="primary" (click)="add()">Add Tenant</button>
      </mat-card-content>
    </mat-card>

    <table mat-table [dataSource]="tenants()" class="mat-elevation-z2 full-width">
      <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let t">{{t.fullName}}</td></ng-container>
      <ng-container matColumnDef="email"><th mat-header-cell *matHeaderCellDef>Email</th><td mat-cell *matCellDef="let t">{{t.email}}</td></ng-container>
      <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let t"><mat-chip>{{t.status}}</mat-chip></td></ng-container>
      <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Actions</th>
        <td mat-cell *matCellDef="let t">
          @if (t.status === 'Applicant') {
            <button mat-button color="accent" (click)="activate(t.id)">Activate</button>
          }
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols;"></tr>
    </table>
  `,
  styles: [`.form-card{margin-bottom:16px}mat-form-field{margin-right:8px}.full-width{width:100%}`]
})
export class TenantsComponent implements OnInit {
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);
  tenants = signal<Tenant[]>([]);
  cols = ['name', 'email', 'status', 'actions'];
  form = { firstName: '', lastName: '', email: '', phone: '' };

  ngOnInit() { this.load(); }
  load() { this.api.getTenants().subscribe(t => this.tenants.set(t)); }

  add() {
    this.api.createTenant(this.form).subscribe(() => {
      this.snack.open('Tenant added', 'OK', { duration: 2000 });
      this.form = { firstName: '', lastName: '', email: '', phone: '' };
      this.load();
    });
  }

  activate(id: string) {
    this.api.activateTenant(id).subscribe(() => {
      this.snack.open('Tenant activated', 'OK', { duration: 2000 });
      this.load();
    });
  }
}
