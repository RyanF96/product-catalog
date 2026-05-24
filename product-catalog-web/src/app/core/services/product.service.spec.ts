import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { API_BASE_URL, API_ENDPOINTS } from '../constants/api.constants';
import { ProductCreateRequest } from '../models/product.model';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;
  const baseUrl = `${API_BASE_URL}${API_ENDPOINTS.products}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProductService]
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getProducts', () => {
    it('should retrieve products with pagination and filters', () => {
      const mockResponse = {
        items: [
          { id: 1, name: 'Laptop', description: 'Gaming laptop', sku: 'LAP-001', price: 999.99, quantity: 10, categoryId: 1, categoryName: 'Electronics', createdAt: '2024-01-01', updatedAt: '2024-01-01' }
        ],
        totalCount: 1,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false
      };

      service.getProducts({ pageNumber: 1, pageSize: 10, categoryId: 1, searchTerm: 'laptop' }).subscribe(result => {
        expect(result.items.length).toBe(1);
        expect(result.totalCount).toBe(1);
        expect(result.totalPages).toBe(1);
        expect(result.hasNextPage).toBe(false);
        expect(result.items[0].name).toBe('Laptop');
      });

      const req = httpMock.expectOne(request =>
        request.url === baseUrl &&
        request.params.get('PageNumber') === '1' &&
        request.params.get('PageSize') === '10' &&
        request.params.get('CategoryId') === '1' &&
        request.params.get('SearchTerm') === 'laptop'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should omit null/undefined params', () => {
      const mockResponse = { items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: false };

      service.getProducts({}).subscribe(result => {
        expect(result.items.length).toBe(0);
      });

      const req = httpMock.expectOne(request =>
        request.url === baseUrl &&
        request.params.keys().length === 0
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should include minPrice, maxPrice, and inStockOnly params', () => {
      const mockResponse = {
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
      };

      service.getProducts({ minPrice: 10, maxPrice: 100, inStockOnly: true }).subscribe(result => {
        expect(result.items.length).toBe(0);
      });

      const req = httpMock.expectOne(request =>
        request.url === baseUrl &&
        request.params.get('MinPrice') === '10' &&
        request.params.get('MaxPrice') === '100' &&
        request.params.get('InStockOnly') === 'true'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getProductById', () => {
    it('should retrieve a single product', () => {
      const mockProduct = { id: 1, name: 'Laptop', description: 'Gaming laptop', sku: 'LAP-001', price: 999.99, quantity: 10, categoryId: 1, categoryName: 'Electronics', createdAt: '2024-01-01', updatedAt: '2024-01-01' };

      service.getProductById(1).subscribe(product => {
        expect(product.id).toBe(1);
        expect(product.name).toBe('Laptop');
      });

      const req = httpMock.expectOne(`${baseUrl}/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockProduct);
    });
  });

  describe('createProduct', () => {
    it('should create a new product', () => {
      const newProduct: ProductCreateRequest = {
        name: 'Mouse',
        description: 'Wireless mouse',
        sku: 'MOU-001',
        price: 29.99,
        quantity: 50,
        categoryId: 2
      };
      const mockResponse = { id: 2, ...newProduct, createdAt: '2024-01-01', updatedAt: '2024-01-01' };

      service.createProduct(newProduct).subscribe(product => {
        expect(product.id).toBe(2);
        expect(product.name).toBe('Mouse');
      });

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newProduct);
      req.flush(mockResponse);
    });
  });

  describe('updateProduct', () => {
    it('should update an existing product without id in body', () => {
      const updatedProduct = {
        name: 'Updated Laptop',
        description: 'Updated gaming laptop',
        sku: 'LAP-001',
        price: 899.99,
        quantity: 5,
        categoryId: 1
      };
      const mockResponse = { ...updatedProduct, id: 1, createdAt: '2024-01-01', updatedAt: '2024-01-02' };

      service.updateProduct(1, updatedProduct).subscribe(product => {
        expect(product.price).toBe(899.99);
        expect(product.quantity).toBe(5);
      });

      const req = httpMock.expectOne(`${baseUrl}/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).not.toHaveProperty('id');
      req.flush(mockResponse);
    });
  });

  describe('deleteProduct', () => {
    it('should delete a product', () => {
      service.deleteProduct(1).subscribe(() => {
        expect(true).toBe(true);
      });

      const req = httpMock.expectOne(`${baseUrl}/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
