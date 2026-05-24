import { Component, OnInit, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { ProductService } from '../../../../core/services/product.service';
import { CategoryService } from '../../../../core/services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Product, ProductCreateRequest } from '../../../../core/models/product.model';
import { Category } from '../../../../core/models/category.model';
import { getFormFieldError } from '../../../../shared/utils/form-utils';
import { Loading } from '../../../../shared/components/loading/loading';
import { ErrorMessage } from '../../../../shared/components/error-message/error-message';

@Component({
  selector: 'app-product-form',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, Loading, ErrorMessage],
  templateUrl: './product-form.html'
})
export class ProductForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly notificationService = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isEditMode = signal(false);
  protected readonly productId = signal<number | null>(null);
  protected readonly categories = signal<Category[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly errorMessage = signal<string>('');

  protected readonly productForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    sku: ['', [Validators.required, Validators.maxLength(50)]],
    price: [0, [Validators.required, Validators.min(0)]],
    quantity: [0, [Validators.required, Validators.min(0)]],
    categoryId: [null, Validators.required]
  });

  constructor() {
    this.loadCategories();
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = parseInt(idParam, 10);
      this.isEditMode.set(true);
      this.productId.set(id);
      this.loadProduct(id);
    }
  }

  private loadCategories(): void {
    this.categoryService.getCategories().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => { /* error interceptor handles toast */ }
    });
  }

  private loadProduct(id: number): void {
    this.isLoading.set(true);
    this.productService.getProductById(id).pipe(
      finalize(() => this.isLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (product) => this.populateForm(product),
      error: () => this.errorMessage.set('Failed to load product')
    });
  }

  private populateForm(product: Product): void {
    this.productForm.patchValue({
      name: product.name,
      description: product.description,
      sku: product.sku,
      price: product.price,
      quantity: product.quantity,
      categoryId: product.categoryId
    });
  }

  protected onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formValue = this.productForm.value as ProductCreateRequest;

    const request$ = this.isEditMode() && this.productId()
      ? this.productService.updateProduct(this.productId()!, formValue)
      : this.productService.createProduct(formValue);

    request$.pipe(
      finalize(() => this.isSaving.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notificationService.success(
          this.isEditMode() ? 'Product updated successfully' : 'Product created successfully'
        );
        this.router.navigate(['/products']);
      },
      error: () => {
        this.errorMessage.set(
          this.isEditMode() ? 'Failed to update product' : 'Failed to create product'
        );
      }
    });
  }

  protected getFieldError(fieldName: string): string {
    return getFormFieldError(this.productForm, fieldName);
  }
}
