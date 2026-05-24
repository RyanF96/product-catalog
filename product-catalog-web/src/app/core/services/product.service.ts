import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL, API_ENDPOINTS } from '../constants/api.constants';
import {
  Product,
  ProductCreateRequest,
  ProductUpdateRequest,
  ProductSearchResult,
  ProductSearchParams
} from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}${API_ENDPOINTS.products}`;

  getProducts(params: ProductSearchParams): Observable<ProductSearchResult> {
    let httpParams = new HttpParams();

    if (params.pageNumber !== undefined) {
      httpParams = httpParams.set('PageNumber', params.pageNumber.toString());
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('PageSize', params.pageSize.toString());
    }
    if (params.categoryId !== undefined && params.categoryId !== null) {
      httpParams = httpParams.set('CategoryId', params.categoryId.toString());
    }
    if (params.searchTerm !== undefined && params.searchTerm !== null && params.searchTerm.trim().length > 0) {
      httpParams = httpParams.set('SearchTerm', params.searchTerm.trim());
    }
    if (params.minPrice !== undefined && params.minPrice !== null) {
      httpParams = httpParams.set('MinPrice', params.minPrice.toString());
    }
    if (params.maxPrice !== undefined && params.maxPrice !== null) {
      httpParams = httpParams.set('MaxPrice', params.maxPrice.toString());
    }
    if (params.inStockOnly !== undefined && params.inStockOnly !== null) {
      httpParams = httpParams.set('InStockOnly', params.inStockOnly.toString());
    }

    return this.http.get<ProductSearchResult>(this.baseUrl, { params: httpParams });
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  createProduct(product: ProductCreateRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, product);
  }

  updateProduct(id: number, product: ProductUpdateRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}`, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
