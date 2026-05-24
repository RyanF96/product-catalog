import { Component, OnInit, DestroyRef, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { ProductService } from '../../../../core/services/product.service';
import { CategoryService } from '../../../../core/services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Product, ProductSearchResult, ProductSearchParams } from '../../../../core/models/product.model';
import { Category } from '../../../../core/models/category.model';
import { Loading } from '../../../../shared/components/loading/loading';
import { ErrorMessage } from '../../../../shared/components/error-message/error-message';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { ProductSearch } from '../product-search/product-search';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, RouterLink, Loading, ErrorMessage, ConfirmDialog, ProductSearch],
  templateUrl: './product-list.html'
})
export class ProductList implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<Category[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string>('');
  protected readonly totalCount = signal(0);
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly selectedCategoryId = signal<number | null>(null);
  protected readonly searchTerm = signal<string>('');
  protected readonly productToDelete = signal<Product | null>(null);
  protected readonly isDeleting = signal(false);

  constructor() {
    this.loadCategories();
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  private loadCategories(): void {
    this.categoryService.getCategories().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => { /* error interceptor handles toast */ }
    });
  }

  private loadProducts(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    const params: ProductSearchParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      categoryId: this.selectedCategoryId(),
      searchTerm: this.searchTerm() || null
    };

    this.productService.getProducts(params).pipe(
      finalize(() => this.isLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (result: ProductSearchResult) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
      },
      error: () => {
        this.errorMessage.set('Failed to load products. Please try again.');
      }
    });
  }

  protected onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.pageNumber.set(1);
    this.loadProducts();
  }

  protected onCategoryChange(categoryId: string): void {
    this.selectedCategoryId.set(categoryId ? parseInt(categoryId, 10) : null);
    this.pageNumber.set(1);
    this.loadProducts();
  }

  protected onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadProducts();
  }

  protected onDeleteClick(product: Product): void {
    this.productToDelete.set(product);
  }

  protected confirmDelete(): void {
    const product = this.productToDelete();
    if (!product) return;

    this.isDeleting.set(true);
    this.productService.deleteProduct(product.id).pipe(
      finalize(() => {
        this.isDeleting.set(false);
        this.productToDelete.set(null);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notificationService.success(`Product "${product.name}" deleted successfully`);
        this.loadProducts();
      },
      error: () => {
        this.notificationService.error('Failed to delete product');
      }
    });
  }

  protected cancelDelete(): void {
    this.productToDelete.set(null);
  }

  protected deleteMessage(): string {
    const product = this.productToDelete();
    return product ? `Are you sure you want to delete "${product.name}"? This action cannot be undone.` : '';
  }

  protected readonly totalPages = computed(() =>
    Math.ceil(this.totalCount() / this.pageSize())
  );

  protected readonly hasNextPage = computed(() =>
    this.pageNumber() < this.totalPages()
  );

  protected readonly hasPreviousPage = computed(() =>
    this.pageNumber() > 1
  );

  protected readonly pages = computed(() => {
    const total = this.totalPages();
    const current = this.pageNumber();
    const pages: number[] = [];
    const maxVisible = 5;

    let start = Math.max(1, current - Math.floor(maxVisible / 2));
    let end = Math.min(total, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  });

  protected getStockStatus(quantity: number): { label: string; class: string } {
    if (quantity === 0) return { label: 'Out of Stock', class: 'bg-red-100 text-red-800' };
    if (quantity < 10) return { label: 'Low Stock', class: 'bg-yellow-100 text-yellow-800' };
    return { label: 'In Stock', class: 'bg-green-100 text-green-800' };
  }
}
