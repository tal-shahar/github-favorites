import { CommonModule, DatePipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { FavoritesApiService } from '../../services/favorites-api.service';
import { FavoriteRequest, RepositorySearchResult } from '../../models/github.models';

type SearchState = {
  loading: boolean;
  results: RepositorySearchResult[];
  page: number;
  perPage: number;
  lastQuery: string;
};

@Component({
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatePipe,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule
  ],
  template: `
    <section class="search-header">
      <mat-form-field appearance="outline" class="query-field">
        <mat-label>Search GitHub repositories</mat-label>
        <input
          matInput
          placeholder="e.g. angular cache"
          [formControl]="searchControl"
          (keyup.enter)="search()"
        />
        <mat-error *ngIf="searchControl.hasError('required')">Search term is required</mat-error>
        <mat-error *ngIf="searchControl.hasError('minlength')">Type at least 2 characters</mat-error>
      </mat-form-field>
      <button mat-flat-button color="primary" (click)="search()" [disabled]="state().loading">
        <mat-icon>search</mat-icon>
        <span>Search</span>
      </button>
    </section>

    <mat-progress-bar *ngIf="state().loading" mode="indeterminate"></mat-progress-bar>

    <p *ngIf="!state().loading && state().results.length === 0" class="empty-state">
      Run a search to see repositories and favorite them for analysis.
    </p>

    <section class="results-grid">
      <mat-card *ngFor="let repo of state().results">
        <mat-card-header>
          <mat-card-title>{{ repo.owner }}/{{ repo.name }}</mat-card-title>
          <mat-card-subtitle>Updated {{ repo.updatedAtUtc | date: 'medium' }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>{{ repo.description || 'No description available.' }}</p>
          <div class="repo-meta">
            <span><mat-icon>star_rate</mat-icon>{{ repo.stars | number }}</span>
            <span><mat-icon>event</mat-icon>{{ repo.updatedAtUtc | date: 'shortDate' }}</span>
          </div>
        </mat-card-content>
        <mat-card-actions>
          <button
            mat-raised-button
            color="accent"
            (click)="favorite(repo)"
            [disabled]="pendingFavorite() === repo.repoId"
          >
            <mat-icon>favorite</mat-icon>
            <span *ngIf="pendingFavorite() !== repo.repoId">Favorite</span>
            <span *ngIf="pendingFavorite() === repo.repoId">Saving...</span>
          </button>
        </mat-card-actions>
      </mat-card>
    </section>

    <section class="pagination" *ngIf="state().results.length">
      <button mat-stroked-button (click)="previousPage()" [disabled]="state().page === 1 || state().loading">
        Previous
      </button>
      <span>Page {{ state().page }}</span>
      <button
        mat-stroked-button
        (click)="nextPage()"
        [disabled]="disableNextPage() || state().loading"
      >
        Next
      </button>
    </section>
  `,
  styles: [
    `
      .search-header {
        display: flex;
        gap: 1rem;
        align-items: flex-end;
        flex-wrap: wrap;
      }

      .query-field {
        flex: 1;
        min-width: 280px;
      }

      .results-grid {
        margin-top: 1.5rem;
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
        gap: 1rem;
      }

      .repo-meta {
        display: flex;
        gap: 1rem;
        font-size: 0.9rem;
        opacity: 0.8;
      }

      .repo-meta mat-icon {
        font-size: 1rem;
        width: 1rem;
        height: 1rem;
        margin-right: 0.25rem;
      }

      .pagination {
        margin-top: 1.5rem;
        display: flex;
        justify-content: center;
        gap: 1rem;
        align-items: center;
      }

      .empty-state {
        margin-top: 2rem;
        text-align: center;
        opacity: 0.8;
      }
    `
  ]
})
export class SearchComponent {
  private readonly api = inject(FavoritesApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  readonly searchControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(2)]
  });

  readonly state = signal<SearchState>({
    loading: false,
    results: [],
    page: 1,
    perPage: 10,
    lastQuery: ''
  });

  readonly pendingFavorite = signal<string | null>(null);

  readonly disableNextPage = computed(() => this.state().results.length < this.state().perPage);

  search(page = 1): void {
    if (this.searchControl.invalid) {
      this.searchControl.markAsTouched();
      return;
    }

    const query = this.searchControl.value.trim();
    if (!query) {
      return;
    }

    this.state.update(current => ({ ...current, loading: true, page, lastQuery: query }));

    this.api
      .searchRepositories(query, page, this.state().perPage)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.state.update(current => ({ ...current, loading: false })))
      )
      .subscribe({
        next: results => {
          this.state.update(current => ({ ...current, results }));
          if (!results.length) {
            this.snackBar.open('No repositories matched your query.', undefined, { duration: 2500 });
          }
        },
        error: err => {
          const message = err?.error?.title ?? 'Search failed. Please try again.';
          this.snackBar.open(message, 'Dismiss', { duration: 3000 });
        }
      });
  }

  nextPage(): void {
    if (this.disableNextPage() || this.state().loading) {
      return;
    }

    this.search(this.state().page + 1);
  }

  previousPage(): void {
    if (this.state().page === 1 || this.state().loading) {
      return;
    }

    this.search(this.state().page - 1);
  }

  favorite(repo: RepositorySearchResult): void {
    if (this.pendingFavorite()) {
      return;
    }

    const payload: FavoriteRequest = {
      repoId: repo.repoId,
      name: repo.name,
      owner: repo.owner,
      description: repo.description,
      stars: repo.stars,
      updatedAtUtc: repo.updatedAtUtc
    };

    this.pendingFavorite.set(repo.repoId);
    this.api
      .createFavorite(payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.pendingFavorite.set(null))
      )
      .subscribe({
        next: () => this.snackBar.open('Favorite saved and queued for analysis.', undefined, { duration: 2500 }),
        error: err => {
          const detail = err?.error?.title ?? 'Could not save favorite.';
          this.snackBar.open(detail, 'Dismiss', { duration: 3000 });
        }
      });
  }
}

