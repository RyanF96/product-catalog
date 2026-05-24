import { Component, OnInit, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { CategoryService } from '../../../../core/services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { Category, CategoryTreeNode } from '../../../../core/models/category.model';
import { getFormFieldError } from '../../../../shared/utils/form-utils';
import { Loading } from '../../../../shared/components/loading/loading';
import { ErrorMessage } from '../../../../shared/components/error-message/error-message';
import { CategoryTree } from '../category-tree/category-tree';

@Component({
  selector: 'app-category-list',
  imports: [CommonModule, ReactiveFormsModule, Loading, ErrorMessage, CategoryTree],
  templateUrl: './category-list.html'
})
export class CategoryList implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly notificationService = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly flatCategories = signal<Category[]>([]);
  protected readonly treeCategories = signal<CategoryTreeNode[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly errorMessage = signal<string>('');
  protected readonly showForm = signal(false);

  protected readonly categoryForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    parentCategoryId: [null]
  });

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.categoryService.getCategories().pipe(
      finalize(() => this.isLoading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (categories) => {
        this.flatCategories.set(categories);
        this.loadCategoryTree();
      },
      error: () => {
        this.errorMessage.set('Failed to load categories');
      }
    });
  }

  private loadCategoryTree(): void {
    this.categoryService.getCategoryTree().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (tree) => this.treeCategories.set(tree),
      error: () => { /* error interceptor handles toast */ }
    });
  }

  protected onSubmit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    const formValue = this.categoryForm.value;

    this.categoryService.createCategory({
      name: formValue.name,
      description: formValue.description,
      parentCategoryId: formValue.parentCategoryId ? parseInt(formValue.parentCategoryId, 10) : null
    }).pipe(
      finalize(() => this.isSaving.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notificationService.success('Category created successfully');
        this.categoryForm.reset({ parentCategoryId: null });
        this.showForm.set(false);
        this.loadCategories();
      },
      error: () => {
        this.errorMessage.set('Failed to create category');
      }
    });
  }

  protected toggleForm(): void {
    this.showForm.set(!this.showForm());
  }

  protected getFieldError(fieldName: string): string {
    return getFormFieldError(this.categoryForm, fieldName);
  }
}
