import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  FavoriteRequest,
  FavoriteResponse,
  RepositorySearchResult
} from '../models/github.models';

@Injectable({
  providedIn: 'root'
})
export class FavoritesApiService {
  private readonly apiBaseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  searchRepositories(query: string, page: number, perPage: number): Observable<RepositorySearchResult[]> {
    const params = new HttpParams()
      .set('q', query)
      .set('page', page)
      .set('perPage', perPage);

    return this.http.get<RepositorySearchResult[]>(`${this.apiBaseUrl}/api/search`, { params });
  }

  createFavorite(payload: FavoriteRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiBaseUrl}/api/favorites`, payload);
  }

  getFavorites(): Observable<FavoriteResponse[]> {
    return this.http.get<FavoriteResponse[]>(`${this.apiBaseUrl}/api/favorites`);
  }

  removeFavorite(repoId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/api/favorites/${encodeURIComponent(repoId)}`);
  }
}

