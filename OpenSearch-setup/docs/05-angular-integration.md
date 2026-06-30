# Module 5: Angular Frontend Integration

## 🎯 Learning Objectives

By the end of this module, you will:

- ✅ Build a search UI with Angular
- ✅ Implement autocomplete/typeahead functionality
- ✅ Create faceted filtering
- ✅ Display search results with highlighting
- ✅ Handle pagination

---

## 5.1 Project Setup

### Step 1: Create Angular Project

```powershell
cd OpenSearch-setup

# Create Angular app with standalone components
ng new frontend --routing --style=scss --standalone
cd frontend

# Install dependencies
npm install @angular/cdk
```

### Step 2: Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── app.component.ts
│   │   ├── app.routes.ts
│   │   ├── core/
│   │   │   └── services/
│   │   │       ├── search.service.ts
│   │   │       └── api.config.ts
│   │   ├── features/
│   │   │   └── search/
│   │   │       ├── search.component.ts
│   │   │       ├── search.component.html
│   │   │       ├── search.component.scss
│   │   │       ├── components/
│   │   │       │   ├── search-box/
│   │   │       │   ├── search-results/
│   │   │       │   ├── facet-filters/
│   │   │       │   └── pagination/
│   │   │       └── models/
│   │   │           └── search.models.ts
│   │   └── shared/
│   │       └── pipes/
│   │           └── highlight.pipe.ts
│   └── environments/
│       ├── environment.ts
│       └── environment.prod.ts
```

---

## 5.2 Configuration

### environment.ts

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### environment.prod.ts

```typescript
export const environment = {
  production: true,
  apiUrl: '/api'
};
```

---

## 5.3 Models

### search.models.ts

```typescript
/**
 * Product model matching the backend.
 */
export interface Product {
  id: string;
  name: string;
  description: string;
  category: string;
  subcategory: string;
  price: number;
  brand: string;
  tags: string[];
  rating: number;
  stock: number;
  isActive: boolean;
  createdAt: string;
  specifications?: ProductSpecifications;
}

export interface ProductSpecifications {
  weight?: number;
  dimensions?: string;
  color?: string;
}

/**
 * Search request parameters.
 */
export interface SearchRequest {
  query?: string;
  category?: string;
  subcategory?: string;
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  tags?: string[];
  inStock?: boolean;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

/**
 * Search response with pagination.
 */
export interface SearchResponse<T> {
  hits: SearchHit<T>[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  took: number;
  facets?: Facets;
}

/**
 * Individual search hit with score and highlights.
 */
export interface SearchHit<T> {
  document: T;
  score: number;
  highlights?: { [field: string]: string[] };
}

/**
 * Facets for filtering.
 */
export interface Facets {
  categories: FacetBucket[];
  brands: FacetBucket[];
  tags: FacetBucket[];
  priceRanges: FacetBucket[];
}

export interface FacetBucket {
  key: string;
  count: number;
}

/**
 * Autocomplete suggestion.
 */
export interface AutocompleteSuggestion {
  text: string;
  category: string;
}

/**
 * Active filters state.
 */
export interface ActiveFilters {
  category?: string;
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  tags: string[];
  inStock?: boolean;
}
```

---

## 5.4 Search Service

### search.service.ts

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, Subject, debounceTime, distinctUntilChanged, switchMap, of, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  Product, 
  SearchRequest, 
  SearchResponse, 
  AutocompleteSuggestion 
} from '../../features/search/models/search.models';

/**
 * Service for interacting with the search API.
 */
@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  /**
   * Search products with filters and pagination.
   */
  searchProducts(request: SearchRequest): Observable<SearchResponse<Product>> {
    let params = new HttpParams();
    
    if (request.query) params = params.set('query', request.query);
    if (request.category) params = params.set('category', request.category);
    if (request.subcategory) params = params.set('subcategory', request.subcategory);
    if (request.brand) params = params.set('brand', request.brand);
    if (request.minPrice !== undefined) params = params.set('minPrice', request.minPrice.toString());
    if (request.maxPrice !== undefined) params = params.set('maxPrice', request.maxPrice.toString());
    if (request.minRating !== undefined) params = params.set('minRating', request.minRating.toString());
    if (request.tags?.length) params = params.set('tags', request.tags.join(','));
    if (request.inStock !== undefined) params = params.set('inStock', request.inStock.toString());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortOrder) params = params.set('sortOrder', request.sortOrder);
    if (request.page) params = params.set('page', request.page.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());

    return this.http.get<SearchResponse<Product>>(`${this.baseUrl}/search/products`, { params });
  }

  /**
   * Get autocomplete suggestions.
   */
  getAutocomplete(query: string, limit = 5): Observable<AutocompleteSuggestion[]> {
    if (!query || query.length < 2) {
      return of([]);
    }
    
    const params = new HttpParams()
      .set('query', query)
      .set('limit', limit.toString());
    
    return this.http.get<AutocompleteSuggestion[]>(`${this.baseUrl}/search/autocomplete`, { params })
      .pipe(catchError(() => of([])));
  }

  /**
   * Get a single product by ID.
   */
  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }

  /**
   * Create autocomplete stream with debounce.
   */
  createAutocompleteStream(input$: Observable<string>): Observable<AutocompleteSuggestion[]> {
    return input$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => this.getAutocomplete(query))
    );
  }
}
```

---

## 5.5 Highlight Pipe

### highlight.pipe.ts

```typescript
import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

/**
 * Pipe to safely render HTML highlights from search results.
 */
@Pipe({
  name: 'highlight',
  standalone: true
})
export class HighlightPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(value: string | string[] | undefined): SafeHtml {
    if (!value) return '';
    
    // Join array if needed
    const text = Array.isArray(value) ? value.join('... ') : value;
    
    // Sanitize and return
    return this.sanitizer.bypassSecurityTrustHtml(text);
  }
}
```

---

## 5.6 Search Box Component

### search-box.component.ts

```typescript
import { Component, EventEmitter, Output, Input, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { SearchService } from '../../../../core/services/search.service';
import { AutocompleteSuggestion } from '../../models/search.models';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-box">
      <div class="search-input-wrapper">
        <input
          type="text"
          [(ngModel)]="query"
          (ngModelChange)="onQueryChange($event)"
          (keyup.enter)="onSearch()"
          (focus)="showSuggestions = true"
          (blur)="hideSuggestions()"
          placeholder="Search products..."
          class="search-input"
          autocomplete="off"
        />
        <button (click)="onSearch()" class="search-button">
          🔍 Search
        </button>
      </div>
      
      @if (showSuggestions && suggestions.length > 0) {
        <ul class="suggestions-list">
          @for (suggestion of suggestions; track suggestion.text) {
            <li 
              (mousedown)="selectSuggestion(suggestion)"
              class="suggestion-item"
            >
              <span class="suggestion-text">{{ suggestion.text }}</span>
              <span class="suggestion-category">in {{ suggestion.category }}</span>
            </li>
          }
        </ul>
      }
    </div>
  `,
  styles: [`
    .search-box {
      position: relative;
      width: 100%;
      max-width: 600px;
    }
    
    .search-input-wrapper {
      display: flex;
      gap: 8px;
    }
    
    .search-input {
      flex: 1;
      padding: 12px 16px;
      font-size: 16px;
      border: 2px solid #ddd;
      border-radius: 8px;
      outline: none;
      transition: border-color 0.2s;
      
      &:focus {
        border-color: #007bff;
      }
    }
    
    .search-button {
      padding: 12px 24px;
      font-size: 16px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: background 0.2s;
      
      &:hover {
        background: #0056b3;
      }
    }
    
    .suggestions-list {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      margin-top: 4px;
      padding: 0;
      list-style: none;
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      z-index: 1000;
      max-height: 300px;
      overflow-y: auto;
    }
    
    .suggestion-item {
      padding: 12px 16px;
      cursor: pointer;
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      &:hover {
        background: #f5f5f5;
      }
    }
    
    .suggestion-text {
      font-weight: 500;
    }
    
    .suggestion-category {
      font-size: 12px;
      color: #666;
    }
  `]
})
export class SearchBoxComponent implements OnInit, OnDestroy {
  @Input() initialQuery = '';
  @Output() search = new EventEmitter<string>();
  
  private searchService = inject(SearchService);
  private querySubject = new Subject<string>();
  private subscription?: Subscription;
  
  query = '';
  suggestions: AutocompleteSuggestion[] = [];
  showSuggestions = false;

  ngOnInit(): void {
    this.query = this.initialQuery;
    
    // Setup autocomplete stream
    this.subscription = this.searchService
      .createAutocompleteStream(this.querySubject.asObservable())
      .subscribe(suggestions => {
        this.suggestions = suggestions;
      });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onQueryChange(value: string): void {
    this.querySubject.next(value);
  }

  onSearch(): void {
    this.showSuggestions = false;
    this.search.emit(this.query);
  }

  selectSuggestion(suggestion: AutocompleteSuggestion): void {
    this.query = suggestion.text;
    this.showSuggestions = false;
    this.search.emit(this.query);
  }

  hideSuggestions(): void {
    // Delay to allow click on suggestion
    setTimeout(() => {
      this.showSuggestions = false;
    }, 200);
  }
}
```

---

## 5.7 Facet Filters Component

### facet-filters.component.ts

```typescript
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Facets, ActiveFilters, FacetBucket } from '../../models/search.models';

@Component({
  selector: 'app-facet-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <aside class="facet-filters">
      <div class="filters-header">
        <h3>Filters</h3>
        @if (hasActiveFilters) {
          <button (click)="clearAllFilters()" class="clear-btn">Clear All</button>
        }
      </div>
      
      <!-- Categories -->
      @if (facets?.categories?.length) {
        <div class="facet-group">
          <h4>Categories</h4>
          @for (bucket of facets.categories; track bucket.key) {
            <label class="facet-option">
              <input 
                type="radio" 
                name="category"
                [value]="bucket.key"
                [checked]="activeFilters.category === bucket.key"
                (change)="onCategoryChange(bucket.key)"
              />
              <span>{{ bucket.key }}</span>
              <span class="count">({{ bucket.count }})</span>
            </label>
          }
        </div>
      }
      
      <!-- Brands -->
      @if (facets?.brands?.length) {
        <div class="facet-group">
          <h4>Brands</h4>
          @for (bucket of facets.brands; track bucket.key) {
            <label class="facet-option">
              <input 
                type="radio" 
                name="brand"
                [value]="bucket.key"
                [checked]="activeFilters.brand === bucket.key"
                (change)="onBrandChange(bucket.key)"
              />
              <span>{{ bucket.key }}</span>
              <span class="count">({{ bucket.count }})</span>
            </label>
          }
        </div>
      }
      
      <!-- Price Range -->
      @if (facets?.priceRanges?.length) {
        <div class="facet-group">
          <h4>Price Range</h4>
          @for (bucket of facets.priceRanges; track bucket.key) {
            <label class="facet-option">
              <input 
                type="checkbox"
                [checked]="isPriceRangeSelected(bucket.key)"
                (change)="onPriceRangeChange(bucket.key, $event)"
              />
              <span>{{ bucket.key }}</span>
              <span class="count">({{ bucket.count }})</span>
            </label>
          }
        </div>
      }
      
      <!-- Rating -->
      <div class="facet-group">
        <h4>Minimum Rating</h4>
        @for (rating of [4, 3, 2, 1]; track rating) {
          <label class="facet-option">
            <input 
              type="radio" 
              name="rating"
              [value]="rating"
              [checked]="activeFilters.minRating === rating"
              (change)="onRatingChange(rating)"
            />
            <span class="stars">{{ '★'.repeat(rating) }}{{ '☆'.repeat(5-rating) }}</span>
            <span>& Up</span>
          </label>
        }
      </div>
      
      <!-- In Stock -->
      <div class="facet-group">
        <label class="facet-option">
          <input 
            type="checkbox"
            [checked]="activeFilters.inStock"
            (change)="onInStockChange($event)"
          />
          <span>In Stock Only</span>
        </label>
      </div>
    </aside>
  `,
  styles: [`
    .facet-filters {
      width: 250px;
      padding: 16px;
      background: #f9f9f9;
      border-radius: 8px;
    }
    
    .filters-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      
      h3 {
        margin: 0;
        font-size: 18px;
      }
    }
    
    .clear-btn {
      padding: 4px 12px;
      font-size: 12px;
      background: #e74c3c;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }
    
    .facet-group {
      margin-bottom: 20px;
      
      h4 {
        margin: 0 0 8px;
        font-size: 14px;
        color: #333;
      }
    }
    
    .facet-option {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 0;
      cursor: pointer;
      font-size: 14px;
      
      input {
        cursor: pointer;
      }
      
      .count {
        color: #666;
        font-size: 12px;
      }
      
      .stars {
        color: #f39c12;
      }
    }
  `]
})
export class FacetFiltersComponent {
  @Input() facets?: Facets;
  @Input() activeFilters: ActiveFilters = { tags: [] };
  @Output() filtersChange = new EventEmitter<ActiveFilters>();

  get hasActiveFilters(): boolean {
    return !!(
      this.activeFilters.category ||
      this.activeFilters.brand ||
      this.activeFilters.minPrice !== undefined ||
      this.activeFilters.maxPrice !== undefined ||
      this.activeFilters.minRating !== undefined ||
      this.activeFilters.tags.length > 0 ||
      this.activeFilters.inStock
    );
  }

  clearAllFilters(): void {
    this.filtersChange.emit({ tags: [] });
  }

  onCategoryChange(category: string): void {
    const newFilters = { 
      ...this.activeFilters, 
      category: this.activeFilters.category === category ? undefined : category 
    };
    this.filtersChange.emit(newFilters);
  }

  onBrandChange(brand: string): void {
    const newFilters = { 
      ...this.activeFilters, 
      brand: this.activeFilters.brand === brand ? undefined : brand 
    };
    this.filtersChange.emit(newFilters);
  }

  onRatingChange(rating: number): void {
    const newFilters = { 
      ...this.activeFilters, 
      minRating: this.activeFilters.minRating === rating ? undefined : rating 
    };
    this.filtersChange.emit(newFilters);
  }

  onInStockChange(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const newFilters = { ...this.activeFilters, inStock: checked || undefined };
    this.filtersChange.emit(newFilters);
  }

  isPriceRangeSelected(key: string): boolean {
    // Simplified - implement based on your price range parsing
    return false;
  }

  onPriceRangeChange(key: string, event: Event): void {
    // Parse price range key (e.g., "$50-$100") and update filters
    const ranges: { [key: string]: { min?: number; max?: number } } = {
      'Under $50': { max: 50 },
      '$50-$100': { min: 50, max: 100 },
      '$100-$200': { min: 100, max: 200 },
      '$200-$500': { min: 200, max: 500 },
      '$500+': { min: 500 }
    };
    
    const range = ranges[key];
    if (range) {
      const checked = (event.target as HTMLInputElement).checked;
      const newFilters = {
        ...this.activeFilters,
        minPrice: checked ? range.min : undefined,
        maxPrice: checked ? range.max : undefined
      };
      this.filtersChange.emit(newFilters);
    }
  }
}
```

---

## 5.8 Search Results Component

### search-results.component.ts

```typescript
import { Component, Input } from '@angular/common';
import { CommonModule } from '@angular/common';
import { SearchHit, Product } from '../../models/search.models';
import { HighlightPipe } from '../../../../shared/pipes/highlight.pipe';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, HighlightPipe],
  template: `
    <div class="search-results">
      @for (hit of hits; track hit.document.id) {
        <article class="product-card">
          <div class="product-info">
            <h3 class="product-name">
              @if (hit.highlights?.['name']?.length) {
                <span [innerHTML]="hit.highlights['name'] | highlight"></span>
              } @else {
                {{ hit.document.name }}
              }
            </h3>
            
            <div class="product-meta">
              <span class="brand">{{ hit.document.brand }}</span>
              <span class="category">{{ hit.document.category }}</span>
              <span class="rating">
                {{ '★'.repeat(Math.floor(hit.document.rating)) }}
                {{ hit.document.rating.toFixed(1) }}
              </span>
            </div>
            
            <p class="product-description">
              @if (hit.highlights?.['description']?.length) {
                <span [innerHTML]="hit.highlights['description'] | highlight"></span>
              } @else {
                {{ hit.document.description | slice:0:150 }}...
              }
            </p>
            
            <div class="product-tags">
              @for (tag of hit.document.tags; track tag) {
                <span class="tag">{{ tag }}</span>
              }
            </div>
          </div>
          
          <div class="product-price-section">
            <span class="price">\${{ hit.document.price.toFixed(2) }}</span>
            <span class="stock" [class.out-of-stock]="hit.document.stock === 0">
              {{ hit.document.stock > 0 ? hit.document.stock + ' in stock' : 'Out of stock' }}
            </span>
            <span class="score">Score: {{ hit.score.toFixed(2) }}</span>
          </div>
        </article>
      } @empty {
        <div class="no-results">
          <p>No products found matching your search.</p>
        </div>
      }
    </div>
  `,
  styles: [`
    .search-results {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    
    .product-card {
      display: flex;
      justify-content: space-between;
      padding: 20px;
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      transition: box-shadow 0.2s;
      
      &:hover {
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      }
    }
    
    .product-info {
      flex: 1;
    }
    
    .product-name {
      margin: 0 0 8px;
      font-size: 18px;
      color: #007bff;
      
      :global(mark) {
        background: #ffeb3b;
        padding: 0 2px;
      }
    }
    
    .product-meta {
      display: flex;
      gap: 16px;
      margin-bottom: 8px;
      font-size: 14px;
      color: #666;
      
      .rating {
        color: #f39c12;
      }
    }
    
    .product-description {
      margin: 0 0 12px;
      color: #444;
      line-height: 1.5;
      
      :global(mark) {
        background: #ffeb3b;
        padding: 0 2px;
      }
    }
    
    .product-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }
    
    .tag {
      padding: 4px 8px;
      font-size: 12px;
      background: #e9ecef;
      border-radius: 4px;
    }
    
    .product-price-section {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 8px;
      min-width: 120px;
    }
    
    .price {
      font-size: 24px;
      font-weight: bold;
      color: #28a745;
    }
    
    .stock {
      font-size: 14px;
      color: #28a745;
      
      &.out-of-stock {
        color: #dc3545;
      }
    }
    
    .score {
      font-size: 12px;
      color: #999;
    }
    
    .no-results {
      padding: 40px;
      text-align: center;
      background: #f9f9f9;
      border-radius: 8px;
    }
  `]
})
export class SearchResultsComponent {
  @Input() hits: SearchHit<Product>[] = [];
  
  protected Math = Math;
}
```

---

## 5.9 Pagination Component

### pagination.component.ts

```typescript
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <nav class="pagination" aria-label="Search results pagination">
      <button 
        (click)="goToPage(currentPage - 1)"
        [disabled]="!hasPreviousPage"
        class="page-btn"
      >
        ← Previous
      </button>
      
      <div class="page-info">
        Page {{ currentPage }} of {{ totalPages }}
        <span class="total-results">({{ totalCount }} results)</span>
      </div>
      
      <button 
        (click)="goToPage(currentPage + 1)"
        [disabled]="!hasNextPage"
        class="page-btn"
      >
        Next →
      </button>
    </nav>
  `,
  styles: [`
    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 20px;
      padding: 20px;
    }
    
    .page-btn {
      padding: 10px 20px;
      font-size: 14px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      transition: background 0.2s;
      
      &:hover:not(:disabled) {
        background: #0056b3;
      }
      
      &:disabled {
        background: #ccc;
        cursor: not-allowed;
      }
    }
    
    .page-info {
      font-size: 14px;
      color: #666;
      
      .total-results {
        margin-left: 8px;
        color: #999;
      }
    }
  `]
})
export class PaginationComponent {
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Input() totalCount = 0;
  @Input() hasNextPage = false;
  @Input() hasPreviousPage = false;
  @Output() pageChange = new EventEmitter<number>();

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }
}
```

---

## 5.10 Main Search Page

### search.component.ts

```typescript
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SearchService } from '../../core/services/search.service';
import { 
  SearchResponse, 
  Product, 
  SearchRequest, 
  ActiveFilters,
  Facets 
} from './models/search.models';
import { SearchBoxComponent } from './components/search-box/search-box.component';
import { FacetFiltersComponent } from './components/facet-filters/facet-filters.component';
import { SearchResultsComponent } from './components/search-results/search-results.component';
import { PaginationComponent } from './components/pagination/pagination.component';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule,
    SearchBoxComponent,
    FacetFiltersComponent,
    SearchResultsComponent,
    PaginationComponent
  ],
  template: `
    <div class="search-page">
      <header class="search-header">
        <h1>Product Search</h1>
        <app-search-box 
          [initialQuery]="currentQuery"
          (search)="onSearch($event)"
        />
      </header>
      
      <div class="search-content">
        <app-facet-filters
          [facets]="facets"
          [activeFilters]="activeFilters"
          (filtersChange)="onFiltersChange($event)"
        />
        
        <main class="results-section">
          @if (loading) {
            <div class="loading">Searching...</div>
          } @else {
            <div class="results-header">
              <span class="result-count">
                {{ searchResponse?.totalCount || 0 }} results
                @if (currentQuery) {
                  for "{{ currentQuery }}"
                }
              </span>
              <span class="search-time" *ngIf="searchResponse?.took">
                ({{ searchResponse.took }}ms)
              </span>
              
              <select (change)="onSortChange($event)" class="sort-select">
                <option value="relevance">Sort by Relevance</option>
                <option value="price-asc">Price: Low to High</option>
                <option value="price-desc">Price: High to Low</option>
                <option value="rating">Highest Rated</option>
                <option value="newest">Newest</option>
              </select>
            </div>
            
            <app-search-results [hits]="searchResponse?.hits || []" />
            
            @if (searchResponse && searchResponse.totalPages > 1) {
              <app-pagination
                [currentPage]="searchResponse.page"
                [totalPages]="searchResponse.totalPages"
                [totalCount]="searchResponse.totalCount"
                [hasNextPage]="searchResponse.hasNextPage"
                [hasPreviousPage]="searchResponse.hasPreviousPage"
                (pageChange)="onPageChange($event)"
              />
            }
          }
        </main>
      </div>
    </div>
  `,
  styles: [`
    .search-page {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }
    
    .search-header {
      text-align: center;
      margin-bottom: 30px;
      
      h1 {
        margin-bottom: 20px;
        color: #333;
      }
      
      app-search-box {
        display: flex;
        justify-content: center;
      }
    }
    
    .search-content {
      display: flex;
      gap: 30px;
    }
    
    .results-section {
      flex: 1;
    }
    
    .results-header {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 20px;
      padding-bottom: 10px;
      border-bottom: 1px solid #eee;
    }
    
    .result-count {
      font-weight: 500;
    }
    
    .search-time {
      color: #999;
      font-size: 14px;
    }
    
    .sort-select {
      margin-left: auto;
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
    
    .loading {
      text-align: center;
      padding: 40px;
      font-size: 18px;
      color: #666;
    }
  `]
})
export class SearchComponent implements OnInit {
  private searchService = inject(SearchService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  searchResponse?: SearchResponse<Product>;
  facets?: Facets;
  loading = false;
  currentQuery = '';
  currentPage = 1;
  currentSort = 'relevance';
  activeFilters: ActiveFilters = { tags: [] };

  ngOnInit(): void {
    // Initialize from URL params
    this.route.queryParams.subscribe(params => {
      this.currentQuery = params['q'] || '';
      this.currentPage = parseInt(params['page'] || '1', 10);
      this.activeFilters = {
        category: params['category'],
        brand: params['brand'],
        minPrice: params['minPrice'] ? parseFloat(params['minPrice']) : undefined,
        maxPrice: params['maxPrice'] ? parseFloat(params['maxPrice']) : undefined,
        minRating: params['minRating'] ? parseFloat(params['minRating']) : undefined,
        tags: params['tags'] ? params['tags'].split(',') : [],
        inStock: params['inStock'] === 'true'
      };
      
      this.performSearch();
    });
  }

  onSearch(query: string): void {
    this.currentQuery = query;
    this.currentPage = 1;
    this.updateUrl();
  }

  onFiltersChange(filters: ActiveFilters): void {
    this.activeFilters = filters;
    this.currentPage = 1;
    this.updateUrl();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.updateUrl();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  onSortChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.currentSort = value;
    this.currentPage = 1;
    this.performSearch();
  }

  private updateUrl(): void {
    const queryParams: any = {};
    
    if (this.currentQuery) queryParams['q'] = this.currentQuery;
    if (this.currentPage > 1) queryParams['page'] = this.currentPage;
    if (this.activeFilters.category) queryParams['category'] = this.activeFilters.category;
    if (this.activeFilters.brand) queryParams['brand'] = this.activeFilters.brand;
    if (this.activeFilters.minPrice) queryParams['minPrice'] = this.activeFilters.minPrice;
    if (this.activeFilters.maxPrice) queryParams['maxPrice'] = this.activeFilters.maxPrice;
    if (this.activeFilters.minRating) queryParams['minRating'] = this.activeFilters.minRating;
    if (this.activeFilters.tags.length) queryParams['tags'] = this.activeFilters.tags.join(',');
    if (this.activeFilters.inStock) queryParams['inStock'] = 'true';
    
    this.router.navigate([], { queryParams, queryParamsHandling: 'replace' });
  }

  private performSearch(): void {
    this.loading = true;
    
    const [sortBy, sortOrder] = this.parseSortValue(this.currentSort);
    
    const request: SearchRequest = {
      query: this.currentQuery || undefined,
      page: this.currentPage,
      pageSize: 10,
      sortBy,
      sortOrder,
      ...this.activeFilters
    };

    this.searchService.searchProducts(request).subscribe({
      next: response => {
        this.searchResponse = response;
        this.facets = response.facets;
        this.loading = false;
      },
      error: error => {
        console.error('Search failed:', error);
        this.loading = false;
      }
    });
  }

  private parseSortValue(value: string): [string, string] {
    switch (value) {
      case 'price-asc': return ['price', 'asc'];
      case 'price-desc': return ['price', 'desc'];
      case 'rating': return ['rating', 'desc'];
      case 'newest': return ['newest', 'desc'];
      default: return ['relevance', 'desc'];
    }
  }
}
```

---

## 5.11 App Configuration

### app.routes.ts

```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'search', pathMatch: 'full' },
  { 
    path: 'search', 
    loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent)
  }
];
```

### app.config.ts

```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient()
  ]
};
```

---

## 5.12 Running the Application

### Start All Services

```powershell
# Terminal 1: OpenSearch
cd OpenSearch-setup
docker-compose up -d

# Terminal 2: .NET API
cd OpenSearch-setup/backend/OpenSearchDemo.Api
dotnet run

# Terminal 3: Angular Frontend
cd OpenSearch-setup/frontend
ng serve
```

### Test the Application

1. Open [http://localhost:4200](http://localhost:4200)
2. Try searching for "wireless" or "headphones"
3. Use facet filters to narrow results
4. Test autocomplete by typing slowly
5. Navigate through pagination

---

## 5.13 Checkpoint Questions

1. ✅ How does the autocomplete debouncing work?
2. ✅ How do facets enable filtered navigation?
3. ✅ How is search highlighting rendered safely?
4. ✅ How is URL state preserved for sharing?
5. ✅ What RxJS operators are used for autocomplete?

---

## Next Steps

✅ **Module 5 Complete!**

👉 Continue to [Module 6: Advanced Topics](./06-advanced-topics.md) for analyzers, aggregations, and optimization.
