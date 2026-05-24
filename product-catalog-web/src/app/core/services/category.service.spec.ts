import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CategoryService } from './category.service';
import { API_BASE_URL, API_ENDPOINTS } from '../constants/api.constants';
import { CategoryCreateRequest, UpdateCategoryRequest } from '../models/category.model';

describe('CategoryService', () => {
  let service: CategoryService;
  let httpMock: HttpTestingController;
  const baseUrl = `${API_BASE_URL}${API_ENDPOINTS.categories}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CategoryService]
    });
    service = TestBed.inject(CategoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCategories', () => {
    it('should retrieve all categories', () => {
      const mockCategories = [
        { id: 1, name: 'Electronics', description: 'Electronic items', parentCategoryId: null },
        { id: 2, name: 'Laptops', description: 'Laptop computers', parentCategoryId: 1 }
      ];

      service.getCategories().subscribe(categories => {
        expect(categories.length).toBe(2);
        expect(categories[0].name).toBe('Electronics');
      });

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockCategories);
    });
  });

  describe('getCategoryById', () => {
    it('should retrieve a single category', () => {
      const mockCategory = { id: 1, name: 'Electronics', description: 'Electronic items', parentCategoryId: null };

      service.getCategoryById(1).subscribe(category => {
        expect(category.id).toBe(1);
        expect(category.name).toBe('Electronics');
      });

      const req = httpMock.expectOne(`${baseUrl}/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCategory);
    });
  });

  describe('getCategoryTree', () => {
    it('should retrieve hierarchical category tree', () => {
      const mockTree = [
        {
          id: 1,
          name: 'Electronics',
          description: 'Electronic items',
          children: [
            { id: 2, name: 'Laptops', description: 'Laptop computers', children: [] }
          ]
        }
      ];

      service.getCategoryTree().subscribe(tree => {
        expect(tree.length).toBe(1);
        expect(tree[0].children!.length).toBe(1);
        expect(tree[0].children![0].name).toBe('Laptops');
      });

      const req = httpMock.expectOne(`${baseUrl}/tree`);
      expect(req.request.method).toBe('GET');
      req.flush(mockTree);
    });
  });

  describe('createCategory', () => {
    it('should create a new category', () => {
      const newCategory: CategoryCreateRequest = {
        name: 'Phones',
        description: 'Smartphones',
        parentCategoryId: 1
      };
      const mockResponse = { id: 3, ...newCategory };

      service.createCategory(newCategory).subscribe(category => {
        expect(category.id).toBe(3);
        expect(category.name).toBe('Phones');
      });

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newCategory);
      req.flush(mockResponse);
    });
  });

  describe('updateCategory', () => {
    it('should update an existing category', () => {
      const updateRequest: UpdateCategoryRequest = {
        name: 'Updated Electronics',
        description: 'Updated description',
        parentCategoryId: null
      };
      const mockResponse = { id: 1, ...updateRequest };

      service.updateCategory(1, updateRequest).subscribe(category => {
        expect(category.name).toBe('Updated Electronics');
      });

      const req = httpMock.expectOne(`${baseUrl}/1`);
      expect(req.request.method).toBe('PUT');
      req.flush(mockResponse);
    });
  });

  describe('deleteCategory', () => {
    it('should delete a category', () => {
      service.deleteCategory(1).subscribe(() => {
        expect(true).toBe(true);
      });

      const req = httpMock.expectOne(`${baseUrl}/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
