export interface Product {
  id: string;
  name: string;
  description: string;
  category: string;
  brand: string;
  price: number;
  stock: number;
  tags: string[];
  specifications: Record<string, string>;
  createdAt: string;
  updatedAt: string;
}

export interface ProductSearchRequest {
  query?: string;
  category?: string;
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  tags?: string[];
  inStock?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface SearchHit<T> {
  id: string;
  score: number;
  source: T;
  highlights: Record<string, string[]>;
}

export interface FacetBucket {
  key: string;
  count: number;
}

export interface SearchResponse<T> {
  hits: SearchHit<T>[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  took: number;
  facets: Record<string, FacetBucket[]>;
}

export interface AutocompleteSuggestion {
  text: string;
  highlighted: string;
  score: number;
}
