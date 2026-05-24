import { Routes } from '@angular/router';

export const CATEGORY_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/category-list/category-list').then(m => m.CategoryList)
  }
];
