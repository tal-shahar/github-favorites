import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule, RouterLink],
  template: `
    <button
      mat-icon-button
      [matMenuTriggerFor]="menu"
      class="user-menu-trigger"
      aria-label="User menu"
    >
      <img
        *ngIf="avatarUrl() as url; else defaultAvatar"
        [src]="url"
        [alt]="username() || email()"
        class="avatar"
      />
      <ng-template #defaultAvatar>
        <div class="avatar-placeholder">
          <mat-icon>person</mat-icon>
        </div>
      </ng-template>
    </button>

    <mat-menu #menu="matMenu" class="user-menu">
      <div class="user-info">
        <img
          *ngIf="avatarUrl() as url; else defaultAvatarInfo"
          [src]="url"
          [alt]="username() || email()"
          class="avatar-large"
        />
        <ng-template #defaultAvatarInfo>
          <div class="avatar-placeholder-large">
            <mat-icon>person</mat-icon>
          </div>
        </ng-template>
        <div class="user-details">
          <div class="username">{{ username() || email() }}</div>
          <div class="email" *ngIf="username()">{{ email() }}</div>
        </div>
      </div>

      <mat-divider></mat-divider>

      <button mat-menu-item routerLink="/search">
        <mat-icon>search</mat-icon>
        <span>Search</span>
      </button>

      <button mat-menu-item routerLink="/favorites">
        <mat-icon>star</mat-icon>
        <span>Favorites</span>
      </button>

      <mat-divider></mat-divider>

      <button mat-menu-item (click)="logout()">
        <mat-icon>exit_to_app</mat-icon>
        <span>Sign out</span>
      </button>
    </mat-menu>
  `,
  styles: [
    `
      .user-menu-trigger {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        overflow: hidden;
        padding: 0;
      }

      .avatar {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 50%;
      }

      .avatar-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background-color: rgba(0, 0, 0, 0.1);
      }

      .avatar-placeholder mat-icon {
        color: rgba(0, 0, 0, 0.54);
      }

      ::ng-deep .user-menu {
        min-width: 240px;
        margin-top: 8px;
      }

      .user-info {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 16px;
      }

      .avatar-large {
        width: 48px;
        height: 48px;
        border-radius: 50%;
        object-fit: cover;
      }

      .avatar-placeholder-large {
        width: 48px;
        height: 48px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        background-color: rgba(0, 0, 0, 0.1);
      }

      .avatar-placeholder-large mat-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        color: rgba(0, 0, 0, 0.54);
      }

      .user-details {
        flex: 1;
        min-width: 0;
      }

      .username {
        font-weight: 600;
        font-size: 14px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }

      .email {
        font-size: 12px;
        color: rgba(0, 0, 0, 0.6);
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        margin-top: 2px;
      }

      button[mat-menu-item] {
        display: flex;
        align-items: center;
        gap: 12px;
      }

      button[mat-menu-item] mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
      }
    `
  ]
})
export class UserMenuComponent {
  private readonly authService = inject(AuthService);

  readonly email = this.authService.currentUserEmail;
  readonly username = this.authService.currentUsername;
  readonly avatarUrl = this.authService.currentAvatarUrl;

  logout(): void {
    this.authService.logout();
  }
}

