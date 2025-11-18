import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/github/callback',
    loadComponent: () => import('./pages/github-callback/github-callback.component').then(m => m.GitHubCallbackComponent)
  },
  {
    path: 'search',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/search/search.component').then(m => m.SearchComponent)
  },
  {
    path: 'favorites',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/favorites/favorites.component').then(m => m.FavoritesComponent)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'search'
  },
  {
    path: '**',
    redirectTo: 'search'
  }
];
