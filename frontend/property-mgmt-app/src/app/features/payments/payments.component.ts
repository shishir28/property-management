import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Payment } from '../../core/models/models';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { SlicePipe } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-payments',
  imports: [FormsModule, SlicePipe, MatTableModule, MatButtonModule, MatInputModule,
    MatFormFieldModule, MatCardModule, MatChipsModule, MatSnackBarModule],
  template: `
    <h2>Payments</h2>
    <div style="margin-bottom:16px">
      <button mat-raised-button color="warn" (click)="runCollection()">
        Run Rent Collection Workflow
      </button>
    </div>

    <mat-card class="form-card">
      <mat-card-title>Record Payment</mat-card-title>
      <mat-card-content>
        <mat-form-field><mat-label>Lease ID</mat-label><input matInput [(ngModel)]="form.leaseId" /></mat-form-field>
        <mat-form-field><mat-label>Amount</mat-label><input matInput type="number" [(ngModel)]="form.amount" /></mat-form-field>
        <mat-form-field><mat-label>Due Date</mat-label><input matInput type="date" [(ngModel)]="form.dueDate" /></mat-form-field>
        <button mat-raised-button color="primary" (click)="add()">Create Payment Record</button>
      </mat-card-content>
    </mat-card>

    <h3>Overdue Payments</h3>
    <table mat-table [dataSource]="overdue()" class="mat-elevation-z2 full-width">
      <ng-container matColumnDef="leaseId"><th mat-header-cell *matHeaderCellDef>Lease ID</th><td mat-cell *matCellDef="let p">{{p.leaseId | slice:0:8}}...</td></ng-container>
      <ng-container matColumnDef="amount"><th mat-header-cell *matHeaderCellDef>Amount</th><td mat-cell *matCellDef="let p">\${{p.amount}}</td></ng-container>
      <ng-container matColumnDef="dueDate"><th mat-header-cell *matHeaderCellDef>Due Date</th><td mat-cell *matCellDef="let p">{{p.dueDate}}</td></ng-container>
      <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let p"><mat-chip color="warn">{{p.status}}</mat-chip></td></ng-container>
      <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef>Actions</th>
        <td mat-cell *matCellDef="let p">
          <button mat-button color="primary" (click)="pay(p.id)">Mark Paid</button>
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols;"></tr>
    </table>
  `,
  styles: [`.form-card{margin-bottom:16px}mat-form-field{margin-right:8px}.full-width{width:100%}`]
})
export class PaymentsComponent implements OnInit {
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);
  overdue = signal<Payment[]>([]);
  cols = ['leaseId', 'amount', 'dueDate', 'status', 'actions'];
  form = { leaseId: '', amount: 0, dueDate: '' };

  ngOnInit() { this.load(); }
  load() { this.api.getOverduePayments().subscribe(p => this.overdue.set(p)); }

  add() {
    this.api.createMaintenance(this.form as any).subscribe(() => {
      this.snack.open('Payment record created', 'OK', { duration: 2000 });
      this.load();
    });
  }

  pay(id: string) {
    this.api.markPaid(id, 'manual').subscribe(() => {
      this.snack.open('Marked as paid', 'OK', { duration: 2000 });
      this.load();
    });
  }

  runCollection() {
    this.api.runRentCollection().subscribe(() =>
      this.snack.open('Collection workflow triggered', 'OK', { duration: 3000 }));
  }
}
