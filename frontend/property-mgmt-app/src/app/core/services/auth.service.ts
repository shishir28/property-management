import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { LoginResponse } from '../models/models';

const TOKEN_KEY = 'pm.auth.token';
const USER_KEY = 'pm.auth.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenState = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly displayNameState = signal<string | null>(localStorage.getItem(USER_KEY));

  readonly token = computed(() => this.tokenState());
  readonly displayName = computed(() => this.displayNameState());
  readonly isAuthenticated = computed(() => !!this.tokenState());

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  login(username: string, password: string) {
    return this.http.post<LoginResponse>('/api/auth/login', { username, password }).pipe(
      tap(response => {
        localStorage.setItem(TOKEN_KEY, response.accessToken);
        localStorage.setItem(USER_KEY, response.displayName);
        this.tokenState.set(response.accessToken);
        this.displayNameState.set(response.displayName);
      })
    );
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.tokenState.set(null);
    this.displayNameState.set(null);
    this.router.navigate(['/login']);
  }
}
