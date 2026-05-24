export interface Category {
  id: number;
  name: string;
  description: string | null;
  parentCategoryId: number | null;
}

export interface CategoryTreeNode {
  id: number;
  name: string;
  description: string | null;
  children: CategoryTreeNode[] | null;
  level?: number;
  hasChildren?: boolean;
}

export interface CategoryCreateRequest {
  name: string;
  description: string | null;
  parentCategoryId: number | null;
}

export interface UpdateCategoryRequest {
  name: string;
  description: string | null;
  parentCategoryId: number | null;
}
