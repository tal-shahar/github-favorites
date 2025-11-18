import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
import { UserMenuComponent } from './components/user-menu/user-menu.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatButtonModule,
    MatDividerModule,
    MatIconModule,
    UserMenuComponent
  ],
  template: `
    <mat-sidenav-container class="app-container">
      <mat-sidenav #drawer class="app-sidenav" mode="over">
        <div class="sidenav-header">
          <span>Navigate</span>
        </div>
        <mat-nav-list>
          <a mat-list-item routerLink="/search" routerLinkActive="active-link" (click)="drawer.close()">Search</a>
          <a mat-list-item routerLink="/favorites" routerLinkActive="active-link" (click)="drawer.close()">Favorites</a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar color="primary" class="app-toolbar">
          <button
            *ngIf="isAuthenticated()"
            mat-icon-button
            aria-label="Open navigation drawer"
            (click)="drawer.toggle()"
            class="menu-button"
          >
            <mat-icon>menu</mat-icon>
          </button>

          <span class="app-title" routerLink="/">GitHub Favorites</span>
          <span class="spacer"></span>

          <ng-container *ngIf="isAuthenticated(); else loggedOut">
            <app-user-menu></app-user-menu>
          </ng-container>

          <ng-template #loggedOut>
            <button mat-stroked-button color="accent" routerLink="/login">Login</button>
          </ng-template>
        </mat-toolbar>

        <main class="app-shell">
          <router-outlet />
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [
    `
      .app-container {
        min-height: 100vh;
        background-color: #0f1424;
      }

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

      .menu-button {
        margin-right: 0.5rem;
      }

      .spacer {
        flex: 1;
      }

      .app-shell {
        padding: 1.5rem;
        max-width: 1024px;
        margin: 0 auto;
      }

      a.active-link {
        font-weight: 600;
      }

      .menu-button {
        margin-right: 0.5rem;
      }

      .app-sidenav {
        width: 240px;
        padding-top: 1rem;
      }

      .sidenav-header {
        font-weight: 600;
        padding: 0 1rem 0.5rem;
      }
    `
  ]
})
export class AppComponent {
  private readonly authService = inject(AuthService);

  readonly isAuthenticated = this.authService.isAuthenticated;
}
