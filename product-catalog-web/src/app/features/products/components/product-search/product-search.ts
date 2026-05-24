import { Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-product-search',
  imports: [ReactiveFormsModule],
  templateUrl: './product-search.html'
})
export class ProductSearch {
  readonly placeholder = input<string>('Search products...');
  readonly debounceMs = input<number>(300);
  readonly searchChange = output<string>();

  protected readonly searchControl = new FormControl('');

  constructor() {
    this.searchControl.valueChanges.pipe(
      debounceTime(this.debounceMs()),
      distinctUntilChanged(),
      takeUntilDestroyed()
    ).subscribe(value => {
      this.searchChange.emit(value ?? '');
    });
  }
}
