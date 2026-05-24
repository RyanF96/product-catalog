import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryTreeNode } from '../../../../core/models/category.model';
import { CategoryTreeNodeComponent } from './category-tree-node';

@Component({
  selector: 'app-category-tree',
  imports: [CommonModule, CategoryTreeNodeComponent],
  templateUrl: './category-tree.html'
})
export class CategoryTree {
  readonly categories = input<CategoryTreeNode[]>([]);
}
