import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary" class="app-toolbar">
      <span class="app-title" routerLink="/">GitHub Favorites</span>
      <span class="spacer"></span>

      <ng-container *ngIf="isAuthenticated(); else loggedOut">
        <a mat-button routerLink="/search" routerLinkActive="active-link">Search</a>
        <a mat-button routerLink="/favorites" routerLinkActive="active-link">Favorites</a>
        <span class="user-email" *ngIf="email() as mail">{{ mail }}</span>
        <button mat-stroked-button color="accent" (click)="logout()">Logout</button>
      </ng-container>

      <ng-template #loggedOut>
        <button mat-stroked-button color="accent" routerLink="/login">Login</button>
      </ng-template>
    </mat-toolbar>

    <main class="app-shell">
      <router-outlet />
    </main>
  `,
  styles: [
    `
      .app-toolbar {
        position: sticky;
        top: 0;
        z-index: 10;
        display: flex;
        gap: 0.5rem;
      }

      .app-title {
        font-weight: 600;
        cursor: pointer;
      }

      .spacer {
        flex: 1;
      }

      .user-email {
        font-size: 0.85rem;
        opacity: 0.8;
        margin-right: 0.5rem;
      }

      .app-shell {
        padding: 1.5rem;
        max-width: 1024px;
        margin: 0 auto;
      }

      a.active-link {
        font-weight: 600;
      }
    `
  ]
})
export class AppComponent {
  private readonly authService = inject(AuthService);

  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly email = this.authService.currentUserEmail;

  logout(): void {
    this.authService.logout();
  }
}
