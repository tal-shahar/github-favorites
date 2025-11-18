import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { FavoritesApiService } from '../../services/favorites-api.service';
import { AnalysisDto, FavoriteResponse } from '../../models/github.models';

type FavoritesState = {
  loading: boolean;
  items: FavoriteResponse[];
};

@Component({
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule
  ],
  template: `
    <section class="header">
      <div>
        <h2>Your favorites</h2>
        <p>Track repositories and review their health analysis once the worker finishes processing.</p>
      </div>
      <button mat-stroked-button color="primary" (click)="refresh()" [disabled]="state().loading">
        Refresh
      </button>
    </section>

    <mat-progress-bar *ngIf="state().loading" mode="indeterminate"></mat-progress-bar>

    <p *ngIf="!state().loading && !state().items.length" class="empty-state">
      No favorites yet. Use the search page to add repositories to this list.
    </p>

    <section class="favorites-grid">
      <mat-card *ngFor="let favorite of state().items">
        <mat-card-header>
          <mat-card-title>{{ favorite.owner }}/{{ favorite.name }}</mat-card-title>
          <mat-card-subtitle>Tracked since {{ favorite.createdAtUtc | date: 'medium' }}</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <p>{{ favorite.description || 'No description provided.' }}</p>
          <div class="chips">
            <mat-chip color="primary" selected>
              <mat-icon>star_rate</mat-icon>
              {{ favorite.stars | number }} stars
            </mat-chip>
            <mat-chip color="accent" selected>
              <mat-icon>event</mat-icon>
              updated {{ favorite.updatedAtUtc | date: 'shortDate' }}
            </mat-chip>
            <mat-chip *ngIf="favorite.analysis; else pendingState" color="primary" selected>
              <mat-icon>favorite</mat-icon>
              Health {{ favorite.analysis!.healthScore | number : '1.0-2' }}
            </mat-chip>
          </div>

          <ng-template #pendingState>
            <mat-chip color="warn" selected class="pending-chip">
              <mat-icon>hourglass_empty</mat-icon>
              <span>Analysis pending</span>
            </mat-chip>
          </ng-template>

          <section *ngIf="favorite.analysis as analysis" class="analysis-grid">
            <div>
              <p class="label">License</p>
              <p>{{ analysis.license || 'Unknown' }}</p>
            </div>
            <div>
              <p class="label">Primary language</p>
              <p>{{ analysis.primaryLanguage || 'Unknown' }}</p>
            </div>
            <div>
              <p class="label">Open issues</p>
              <p>{{ analysis.openIssues }}</p>
            </div>
            <div>
              <p class="label">Forks</p>
              <p>{{ analysis.forks }}</p>
            </div>
            <div>
              <p class="label">Stars snapshot</p>
              <p>{{ analysis.starsSnapshot }}</p>
            </div>
            <div>
              <p class="label">Readme length</p>
              <p>{{ analysis.readmeLength | number }} chars</p>
            </div>
            <div>
              <p class="label">Activity</p>
              <p>{{ analysis.activityDays }} days since last push</p>
            </div>
            <div>
              <p class="label">Default branch</p>
              <p>{{ analysis.defaultBranch }}</p>
            </div>
          </section>

          <section *ngIf="favorite.analysis as analysis" class="topics">
            <p class="label">Topics</p>
            <div class="chip-row">
              <mat-chip *ngFor="let topic of analysis.topics">{{ topic }}</mat-chip>
              <span *ngIf="!analysis.topics.length">No topics provided by GitHub.</span>
            </div>
          </section>

          <section *ngIf="favorite.analysis as analysis" class="languages">
            <p class="label">Languages</p>
            <div class="chip-row">
              <mat-chip *ngFor="let lang of languageEntries(analysis)">
                {{ lang[0] }} — {{ lang[1] | number }}
              </mat-chip>
              <span *ngIf="!Object.keys(analysis.languages ?? {}).length">No language data yet.</span>
            </div>
          </section>
        </mat-card-content>

        <mat-card-actions>
          <button mat-button color="warn" (click)="removeFavorite(favorite)" [disabled]="state().loading">
            <mat-icon>delete</mat-icon>
            Remove
          </button>
        </mat-card-actions>
      </mat-card>
    </section>
  `,
  styles: [
    `
      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 1rem;
        flex-wrap: wrap;
      }

      .favorites-grid {
        margin-top: 1.5rem;
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .chips {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        margin: 1rem 0;
      }

      .analysis-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
        gap: 0.5rem 1rem;
        margin-bottom: 1rem;
      }

      .topics,
      .languages {
        margin-bottom: 0.75rem;
      }

      .chip-row {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        align-items: center;
      }

      .label {
        font-size: 0.8rem;
        text-transform: uppercase;
        opacity: 0.7;
        margin-bottom: 0.15rem;
      }

      .pending-chip {
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
      }

      :host ::ng-deep .mat-mdc-standard-chip:not(.mdc-evolution-chip--disabled) {
        display: inline-flex;
        align-items: center;
        vertical-align: middle;
      }

      :host ::ng-deep .mat-mdc-standard-chip:not(.mdc-evolution-chip--disabled) .mdc-evolution-chip__text-label {
        display: inline-flex;
        align-items: center;
        line-height: 1.5;
      }

      .empty-state {
        margin-top: 2rem;
        text-align: center;
        opacity: 0.8;
      }
    `
  ]
})
export class FavoritesComponent {
  protected readonly Object = Object;

  private readonly api = inject(FavoritesApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly state = signal<FavoritesState>({
    loading: true,
    items: []
  });

  constructor() {
    this.refresh();
  }

  refresh(): void {
    this.state.update(current => ({ ...current, loading: true }));
    this.api
      .getFavorites()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.state.update(current => ({ ...current, loading: false })))
      )
      .subscribe({
        next: favorites => this.state.set({ loading: false, items: favorites }),
        error: err => {
          const detail = err?.error?.title ?? 'Unable to load favorites.';
          this.snackBar.open(detail, 'Dismiss', { duration: 3000 });
        }
      });
  }

  removeFavorite(favorite: FavoriteResponse): void {
    this.state.update(current => ({ ...current, loading: true }));
    this.api
      .removeFavorite(favorite.repoId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.state.update(current => ({ ...current, loading: false })))
      )
      .subscribe({
        next: () => {
          this.state.update(current => ({
            loading: false,
            items: current.items.filter(item => item.repoId !== favorite.repoId)
          }));
          this.snackBar.open('Favorite removed.', undefined, { duration: 2000 });
        },
        error: err => {
          const detail = err?.error?.title ?? 'Unable to remove favorite.';
          this.snackBar.open(detail, 'Dismiss', { duration: 3000 });
        }
      });
  }

  languageEntries(analysis: AnalysisDto): [string, number][] {
    return Object.entries(analysis.languages ?? {});
  }
}

