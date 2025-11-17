import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthState, LoginResponse } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly storageKey = 'githubFavorites.auth';
  private readonly apiBaseUrl = environment.apiBaseUrl;

  private readonly state = signal<AuthState | null>(this.restore());

  readonly isAuthenticated = computed(() => {
    const value = this.state();
    if (!value) {
      return false;
    }

    return new Date(value.expiresAtUtc).getTime() > Date.now();
  });

  readonly currentUserEmail = computed(() => this.state()?.email ?? '');

  constructor(private readonly http: HttpClient, private readonly router: Router) {}

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.apiBaseUrl}/auth/login`, { email, password }).pipe(
      tap(response => this.persistSession(response, email))
    );
  }

  logout(navigate = true): void {
    this.state.set(null);
    localStorage.removeItem(this.storageKey);
    if (navigate) {
      this.router.navigate(['/login']);
    }
  }

  get token(): string | null {
    if (!this.isAuthenticated()) {
      return null;
    }

    return this.state()?.token ?? null;
  }

  private persistSession(response: LoginResponse, email: string): void {
    const payload: AuthState = {
      email,
      token: response.token,
      expiresAtUtc: response.expiresAtUtc
    };

    this.state.set(payload);
    localStorage.setItem(this.storageKey, JSON.stringify(payload));
  }

  private restore(): AuthState | null {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) {
        return null;
      }
      const payload = JSON.parse(raw) as AuthState;
      return payload;
    } catch {
      return null;
    }
  }
}

