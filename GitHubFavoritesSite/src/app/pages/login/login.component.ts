import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  template: `
    <section class="login-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Sign in</mat-card-title>
          <mat-card-subtitle>Use the seeded demo account to explore the app.</mat-card-subtitle>
        </mat-card-header>

        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email" />
            <mat-error *ngIf="form.controls.email.hasError('required')">Email is required</mat-error>
            <mat-error *ngIf="form.controls.email.hasError('email')">Enter a valid email</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
            <mat-error *ngIf="form.controls.password.hasError('required')">Password is required</mat-error>
          </mat-form-field>

          <button mat-flat-button color="primary" type="submit" [disabled]="loading()">
            <span *ngIf="!loading()">Login</span>
            <span *ngIf="loading()">Signing in...</span>
          </button>

          <div class="divider">
            <span>or</span>
          </div>

          <button mat-stroked-button type="button" (click)="loginWithGitHub()" [disabled]="loading()" class="github-button">
            <svg class="github-icon" viewBox="0 0 24 24" width="20" height="20" fill="currentColor">
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
            </svg>
            <span>Continue with GitHub</span>
          </button>
        </form>
      </mat-card>
    </section>
  `,
  styles: [
    `
      .login-container {
        display: flex;
        justify-content: center;
        align-items: flex-start;
        padding: 3rem 1rem;
      }

      mat-card {
        max-width: 420px;
        width: 100%;
        padding: 2rem 2rem 1.5rem;
        box-sizing: border-box;
      }

      form {
        display: flex;
        flex-direction: column;
        gap: 1rem;
        margin-top: 1.5rem;
      }

      mat-form-field {
        width: 100%;
      }

      .divider {
        display: flex;
        align-items: center;
        text-align: center;
        margin: 1rem 0;
        color: rgba(0, 0, 0, 0.54);
      }

      .divider::before,
      .divider::after {
        content: '';
        flex: 1;
        border-bottom: 1px solid rgba(0, 0, 0, 0.12);
      }

      .divider span {
        padding: 0 1rem;
        font-size: 0.875rem;
      }

      .github-button {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        width: 100%;
        border-color: rgba(0, 0, 0, 0.12);
      }

      .github-icon {
        flex-shrink: 0;
      }
    `
  ]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const { email, password } = this.form.getRawValue();
    this.authService
      .login(email, password)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Welcome back!', undefined, { duration: 2000 });
          this.router.navigate(['/search']);
        },
        error: err => {
          const message = err?.error?.title ?? 'Unable to sign in. Please try again.';
          this.snackBar.open(message, 'Dismiss', { duration: 3000 });
        }
      });
  }

  loginWithGitHub(): void {
    if (this.loading()) {
      return;
    }
    this.authService.initiateGitHubOAuth();
  }
}

