import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Property } from '../../core/models/models';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-properties',
  imports: [FormsModule, MatTableModule, MatButtonModule, MatInputModule, MatFormFieldModule, MatCardModule, MatSnackBarModule],
  template: `
    <h2>Properties</h2>
    <mat-card class="form-card">
      <mat-card-content>
        <mat-form-field><mat-label>Address</mat-label><input matInput [(ngModel)]="form.address" /></mat-form-field>
        <mat-form-field><mat-label>City</mat-label><input matInput [(ngModel)]="form.city" /></mat-form-field>
        <mat-form-field><mat-label>State</mat-label><input matInput [(ngModel)]="form.state" maxlength="2" /></mat-form-field>
        <mat-form-field><mat-label>Zip</mat-label><input matInput [(ngModel)]="form.zip" /></mat-form-field>
        <mat-form-field><mat-label>Units</mat-label><input matInput type="number" [(ngModel)]="form.unitCount" /></mat-form-field>
        <button mat-raised-button color="primary" (click)="add()">Add Property</button>
      </mat-card-content>
    </mat-card>

    <table mat-table [dataSource]="properties()" class="mat-elevation-z2 full-width">
      <ng-container matColumnDef="address"><th mat-header-cell *matHeaderCellDef>Address</th><td mat-cell *matCellDef="let p">{{p.address}}</td></ng-container>
      <ng-container matColumnDef="city"><th mat-header-cell *matHeaderCellDef>City</th><td mat-cell *matCellDef="let p">{{p.city}}</td></ng-container>
      <ng-container matColumnDef="state"><th mat-header-cell *matHeaderCellDef>State</th><td mat-cell *matCellDef="let p">{{p.state}}</td></ng-container>
      <ng-container matColumnDef="units"><th mat-header-cell *matHeaderCellDef>Units</th><td mat-cell *matCellDef="let p">{{p.unitCount}}</td></ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols;"></tr>
    </table>
  `,
  styles: [`.form-card{margin-bottom:16px}mat-form-field{margin-right:8px}.full-width{width:100%}`]
})
export class PropertiesComponent implements OnInit {
  private api = inject(ApiService);
  private snack = inject(MatSnackBar);
  properties = signal<Property[]>([]);
  cols = ['address', 'city', 'state', 'units'];
  form = { address: '', city: '', state: '', zip: '', unitCount: 1 };

  ngOnInit() { this.load(); }
  load() { this.api.getProperties().subscribe(p => this.properties.set(p)); }

  add() {
    this.api.createProperty(this.form).subscribe(() => {
      this.snack.open('Property added', 'OK', { duration: 2000 });
      this.form = { address: '', city: '', state: '', zip: '', unitCount: 1 };
      this.load();
    });
  }
}
