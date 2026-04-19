import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="login-page">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-icon>lock</mat-icon>
          <mat-card-title>Property Management Login</mat-card-title>
        </mat-card-header>

        <mat-card-content>
          <p class="helper">
            Sign in with the local development credentials configured in the API.
          </p>

          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline">
              <mat-label>Username</mat-label>
              <input matInput formControlName="username" autocomplete="username" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="current-password" />
            </mat-form-field>

            @if (error()) {
              <p class="error">{{ error() }}</p>
            }

            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || loading()">
              @if (loading()) {
                <mat-spinner diameter="18" />
              } @else {
                <span>Sign In</span>
              }
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-page {
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 24px;
      background:
        radial-gradient(circle at top left, rgba(16, 185, 129, 0.22), transparent 30%),
        radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.2), transparent 35%),
        linear-gradient(145deg, #f5f7fb, #eef3f8);
    }

    .login-card {
      width: min(100%, 420px);
      padding: 12px;
    }

    mat-card-header {
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .helper {
      margin-bottom: 16px;
      color: #475569;
    }

    form {
      display: grid;
      gap: 16px;
    }

    mat-form-field {
      width: 100%;
    }

    .error {
      margin: 0;
      color: #b91c1c;
    }

    button[mat-flat-button] {
      min-height: 44px;
    }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly form = this.fb.nonNullable.group({
    username: ['admin@property.local', [Validators.required]],
    password: ['Passw0rd!', [Validators.required]]
  });

  submit() {
    if (this.form.invalid || this.loading()) {
      return;
    }

    this.loading.set(true);
    this.error.set('');

    const { username, password } = this.form.getRawValue();
    this.auth.login(username, password).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Sign-in failed. Check the configured development credentials and try again.');
      }
    });
  }
}
