import { Component, inject, signal, computed, effect, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { SearchService } from '../../services/search.service';
import { ProductSearchRequest, AutocompleteSuggestion, FacetBucket } from '../../models/product.model';
import { ProductListComponent } from '../product-list/product-list.component';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductListComponent],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit, OnDestroy {
  private readonly searchService = inject(SearchService);
  private readonly searchTerms$ = new Subject<string>();
  private subscriptions: Subscription[] = [];

  // Search state signals
  searchQuery = signal('');
  selectedCategory = signal<string | undefined>(undefined);
  selectedBrand = signal<string | undefined>(undefined);
  minPrice = signal<number | undefined>(undefined);
  maxPrice = signal<number | undefined>(undefined);
  inStockOnly = signal(false);
  currentPage = signal(1);
  pageSize = signal(10);
  sortBy = signal('_score');
  sortOrder = signal<'asc' | 'desc'>('desc');

  // Results state
  searchResults = signal<any>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  
  // Autocomplete state
  suggestions = signal<AutocompleteSuggestion[]>([]);
  showSuggestions = signal(false);

  // Computed values for facets
  categoryFacets = computed(() => this.searchResults()?.facets?.['categories'] || []);
  brandFacets = computed(() => this.searchResults()?.facets?.['brands'] || []);
  priceRangeFacets = computed(() => this.searchResults()?.facets?.['priceRanges'] || []);

  // Computed pagination info
  totalPages = computed(() => this.searchResults()?.totalPages || 0);
  totalResults = computed(() => this.searchResults()?.total || 0);
  searchTook = computed(() => this.searchResults()?.took || 0);

  ngOnInit(): void {
    // Set up autocomplete subscription
    const autocompleteSub = this.searchService
      .createAutocompleteStream(this.searchTerms$)
      .subscribe({
        next: (suggestions) => this.suggestions.set(suggestions),
        error: (err) => console.error('Autocomplete error:', err)
      });
    
    this.subscriptions.push(autocompleteSub);
    
    // Initial search to load all products
    this.search();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  onSearchInput(event: Event): void {
    const query = (event.target as HTMLInputElement).value;
    this.searchQuery.set(query);
    this.searchTerms$.next(query);
    this.showSuggestions.set(true);
  }

  selectSuggestion(suggestion: AutocompleteSuggestion): void {
    this.searchQuery.set(suggestion.text);
    this.showSuggestions.set(false);
    this.currentPage.set(1);
    this.search();
  }

  hideSuggestions(): void {
    // Delay to allow click on suggestion
    setTimeout(() => this.showSuggestions.set(false), 200);
  }

  search(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const request: ProductSearchRequest = {
      query: this.searchQuery() || undefined,
      category: this.selectedCategory(),
      brand: this.selectedBrand(),
      minPrice: this.minPrice(),
      maxPrice: this.maxPrice(),
      inStock: this.inStockOnly() ? true : undefined,
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortOrder: this.sortOrder()
    };

    this.searchService.searchProducts(request).subscribe({
      next: (response) => {
        this.searchResults.set(response);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Search error:', err);
        this.error.set(err.message || 'Search failed. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  onCategorySelect(category: string | undefined): void {
    this.selectedCategory.set(category);
    this.currentPage.set(1);
    this.search();
  }

  onBrandSelect(brand: string | undefined): void {
    this.selectedBrand.set(brand);
    this.currentPage.set(1);
    this.search();
  }

  onPriceRangeChange(): void {
    this.currentPage.set(1);
    this.search();
  }

  onStockFilterChange(): void {
    this.currentPage.set(1);
    this.search();
  }

  onSortChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    const [sortBy, sortOrder] = value.split(':');
    this.sortBy.set(sortBy);
    this.sortOrder.set(sortOrder as 'asc' | 'desc');
    this.currentPage.set(1);
    this.search();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.search();
    }
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedCategory.set(undefined);
    this.selectedBrand.set(undefined);
    this.minPrice.set(undefined);
    this.maxPrice.set(undefined);
    this.inStockOnly.set(false);
    this.currentPage.set(1);
    this.sortBy.set('_score');
    this.sortOrder.set('desc');
    this.search();
  }

  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);
    
    if (end - start < 4) {
      if (start === 1) {
        end = Math.min(total, 5);
      } else {
        start = Math.max(1, total - 4);
      }
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }
}
