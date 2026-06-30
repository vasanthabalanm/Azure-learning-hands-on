import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  Product, 
  ProductSearchRequest, 
  SearchResponse, 
  AutocompleteSuggestion 
} from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Search products with filters and pagination.
   */
  searchProducts(request: ProductSearchRequest): Observable<SearchResponse<Product>> {
    let params = new HttpParams();
    
    if (request.query) params = params.set('query', request.query);
    if (request.category) params = params.set('category', request.category);
    if (request.brand) params = params.set('brand', request.brand);
    if (request.minPrice !== undefined) params = params.set('minPrice', request.minPrice.toString());
    if (request.maxPrice !== undefined) params = params.set('maxPrice', request.maxPrice.toString());
    if (request.inStock !== undefined) params = params.set('inStock', request.inStock.toString());
    if (request.page !== undefined) params = params.set('page', request.page.toString());
    if (request.pageSize !== undefined) params = params.set('pageSize', request.pageSize.toString());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortOrder) params = params.set('sortOrder', request.sortOrder);
    if (request.tags?.length) {
      request.tags.forEach(tag => params = params.append('tags', tag));
    }

    return this.http.get<SearchResponse<Product>>(`${this.apiUrl}/search/products`, { params });
  }

  /**
   * Get autocomplete suggestions for search input.
   */
  getAutocomplete(query: string, limit: number = 5): Observable<AutocompleteSuggestion[]> {
    if (!query || query.length < 2) {
      return of([]);
    }
    
    const params = new HttpParams()
      .set('query', query)
      .set('limit', limit.toString());
    
    return this.http.get<AutocompleteSuggestion[]>(`${this.apiUrl}/search/autocomplete`, { params });
  }

  /**
   * Create an observable that handles autocomplete with debouncing.
   */
  createAutocompleteStream(searchTerms$: Subject<string>): Observable<AutocompleteSuggestion[]> {
    return searchTerms$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => this.getAutocomplete(term))
    );
  }
}
