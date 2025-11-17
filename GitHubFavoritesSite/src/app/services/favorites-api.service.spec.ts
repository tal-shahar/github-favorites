import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { FavoritesApiService } from './favorites-api.service';
import { environment } from '../../environments/environment';

describe('FavoritesApiService', () => {
  let service: FavoritesApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        FavoritesApiService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(FavoritesApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('calls the search endpoint with pagination params', () => {
    service.searchRepositories('angular', 2, 5).subscribe(response => {
      expect(response).toEqual([]);
    });

    const req = httpMock.expectOne(r => r.url === `${environment.apiBaseUrl}/api/search`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('q')).toBe('angular');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('perPage')).toBe('5');

    req.flush([]);
  });
});

