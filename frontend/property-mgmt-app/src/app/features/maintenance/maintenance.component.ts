import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { MaintenanceRequest } from '../../core/models/models';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-maintenance',
  imports: [FormsModule, MatTableModule, MatButtonModule, MatInputModule, MatFormFieldModule,
    MatSelectModule, MatCardModule, MatChipsModule, MatSnackBarModule],
  template: `
    <h2>Maintenance Requests</h2>
    <mat-card class="form-card">
      <mat-card-content>
        <mat-form-field><mat-label>Lease ID</mat-label><input matInput [(ngModel)]="form.leaseId" /></mat-form-field>
        <mat-form-field><mat-label>Title</mat-label><input matInput [(ngModel)]="form.title" /></mat-form-field>
        <mat-form-field><mat-label>Description</mat-label><input matInput [(ngModel)]="form.description" /></mat-form-field>
        <mat-form-field>
          <mat-label>Priority</mat-label>
          <mat-select [(ngModel)]="form.priority">
            <mat-option value="Routine">Routine</mat-option>
            <mat-option value="Urgent">Urgent</mat-option>
            <mat-option value="Emergency">Emergency</mat-option>
          </mat-select>
        </mat-form-field>
        <button mat-raised-button color="primary" (click)="add()">Submit Request</button>
      </mat-card-content>
    </mat-card>

    <table mat-table [dataSource]="requests()" class="mat-elevation-z2 full-width">
      <ng-container matColumnDef="title"><th mat-header-cell *matHeaderCellDef>Title</th><td mat-cell *matCellDef="let r">{{r.title}}</td></ng-container>
      <ng-container matColumnDef="priority"><th mat-header-cell *matHeaderCellDef>Priority</th><td mat-cell *matCellDef="let r"><mat-chip [color]="r.priority === 'Emergency' ? 'warn' : 'primary'">{{r.priority}}</mat-chip></td></ng-container>
      <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let r">{{r.status}}</td></ng-container>
      <ng-container matColumnDef="assignedTo"><th mat-header-cell *matHeaderCellDef>Assigned To</th><td mat-cell *matCellDef="let r">{{r.assignedTo ?? '—'}}</td></ng-container>
      <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Actions</th>
        <td mat-cell *matCellDef="let r">
          @if (r.status !== 'Resolved' && r.status !== 'Closed') {
            <button mat-button color="accent" (click)="resolve(r.id)">Resolve</button>
          }
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols;"></tr>
    </table>
  `,
  styles: [`.form-card{margin-bottom:16px}mat-form-field{margin-right:8px}.full-width{width:100%}`]
})
export class MaintenanceComponent implements OnInit {
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);
  requests = signal<MaintenanceRequest[]>([]);
  cols = ['title', 'priority', 'status', 'assignedTo', 'actions'];
  form = { leaseId: '', title: '', description: '', priority: 'Routine', assignedTo: '' };

  ngOnInit() { this.load(); }
  load() { this.api.getMaintenance().subscribe(r => this.requests.set(r)); }

  add() {
    this.api.createMaintenance(this.form).subscribe(() => {
      this.snack.open('Request submitted — triage workflow triggered', 'OK', { duration: 3000 });
      this.form = { leaseId: '', title: '', description: '', priority: 'Routine', assignedTo: '' };
      this.load();
    });
  }

  resolve(id: string) {
    this.api.resolveMaintenance(id).subscribe(() => {
      this.snack.open('Marked resolved', 'OK', { duration: 2000 });
      this.load();
    });
  }
}
