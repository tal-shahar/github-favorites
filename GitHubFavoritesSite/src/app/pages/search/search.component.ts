import { CommonModule, DatePipe } from '@angular/common';
import { Component, DestroyRef, OnDestroy, computed, inject, signal } from '@angular/core';
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
import { Subject, debounceTime, takeUntil } from 'rxjs';
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
          class="search-input"
          placeholder="e.g. angular cache"
          [formControl]="searchControl"
          (input)="onQueryChange()"
          (keyup.enter)="search()"
        />
        <mat-error *ngIf="searchControl.hasError('required')">Search term is required</mat-error>
        <mat-error *ngIf="searchControl.hasError('minlength')">Type at least 2 characters</mat-error>
      </mat-form-field>
    </section>

    <mat-progress-bar *ngIf="state().loading" mode="indeterminate"></mat-progress-bar>

    <p *ngIf="!state().loading && state().results.length === 0" class="empty-state">
      Run a search to see repositories and favorite them for analysis.
    </p>

    <section class="results-grid">
      <mat-card *ngFor="let repo of state().results">
        <mat-card-header>
          <mat-card-title>
            <a [href]="'https://github.com/' + repo.owner + '/' + repo.name" target="_blank" rel="noopener noreferrer" class="repo-link">
              {{ repo.owner }}/{{ repo.name }}
              <mat-icon class="external-icon">open_in_new</mat-icon>
            </a>
          </mat-card-title>
          <mat-card-subtitle>Last push {{ repo.updatedAtUtc | date: 'medium' }}</mat-card-subtitle>
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
        flex-direction: column;
        gap: 0.5rem;
      }

      .query-field {
        flex: 1;
        min-width: 280px;
      }

      :host ::ng-deep .query-field .mat-mdc-text-field-wrapper {
        background-color: #d3e4ff;
        border-radius: 10px;
        box-shadow: 0 12px 30px rgba(122, 166, 255, 0.35);
      }

      :host ::ng-deep .query-field .mdc-notched-outline__leading,
      :host ::ng-deep .query-field .mdc-notched-outline__notch,
      :host ::ng-deep .query-field .mdc-notched-outline__trailing {
        border-color: #bcd3ff;
      }

      :host ::ng-deep .query-field.mat-focused .mdc-notched-outline__leading,
      :host ::ng-deep .query-field.mat-focused .mdc-notched-outline__notch,
      :host ::ng-deep .query-field.mat-focused .mdc-notched-outline__trailing {
        border-color: #8fc2ff;
      }

      .search-input {
        color: #0d2b66;
      }

      .search-input::placeholder {
        color: rgba(13, 43, 102, 0.65);
      }

      .results-grid {
        margin-top: 1.5rem;
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
        gap: 1rem;
      }

      :host ::ng-deep .mat-mdc-card {
        background-color: #182235;
        color: #e6eef7;
      }

      :host ::ng-deep .mat-mdc-card.mdc-card.ng-star-inserted {
        background-color: #182235;
        color: #e6eef7;
      }

      :host ::ng-deep .mat-mdc-card-subtitle {
        color: #e6eef7;
      }

      :host ::ng-deep .mat-mdc-card.mdc-card.ng-star-inserted .mat-mdc-card-subtitle {
        color: #e6eef7;
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

      .repo-link {
        color: inherit;
        text-decoration: none;
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        transition: opacity 0.2s;
      }

      .repo-link:hover {
        opacity: 0.8;
      }

      .external-icon {
        font-size: 1rem;
        width: 1rem;
        height: 1rem;
        opacity: 0.7;
      }
    `
  ]
})
export class SearchComponent implements OnDestroy {
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

  private readonly destroy$ = new Subject<void>();
  private readonly inputChanges$ = new Subject<void>();

  constructor() {
    this.inputChanges$
      .pipe(debounceTime(300), takeUntil(this.destroy$))
      .subscribe(() => this.search());
  }

  search(page = 1): void {
    if (page === 1 && this.state().loading) {
      return;
    }
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

  onQueryChange(): void {
    const trimmed = this.searchControl.value.trim();
    if (!trimmed) {
      this.state.update(current => ({ ...current, results: [], lastQuery: '', page: 1 }));
      return;
    }
    if (trimmed.length >= 2) {
      this.inputChanges$.next();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.inputChanges$.complete();
  }
}

