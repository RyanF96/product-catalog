export interface Product {
  id: number;
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
  categoryName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProductCreateRequest {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
}

export interface ProductUpdateRequest {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
}

export interface ProductSearchResult {
  items: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ProductSearchParams {
  pageNumber?: number;
  pageSize?: number;
  categoryId?: number | null;
  searchTerm?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  inStockOnly?: boolean | null;
}
