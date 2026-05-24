import { Component, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryTreeNode } from '../../../../core/models/category.model';

@Component({
  selector: 'app-category-tree-node',
  imports: [CommonModule, CategoryTreeNodeComponent],
  templateUrl: './category-tree-node.html'
})
export class CategoryTreeNodeComponent {
  readonly node = input.required<CategoryTreeNode>();
  protected readonly isExpanded = signal(true);

  protected get indentation(): number {
    return (this.node().level ?? 0) * 20;
  }

  protected get hasChildren(): boolean {
    return (this.node().children?.length ?? 0) > 0;
  }

  protected toggleExpanded(): void {
    this.isExpanded.update(v => !v);
  }
}
