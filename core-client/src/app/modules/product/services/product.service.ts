// product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { Product, ProductCategory, ProductFilter } from '../../../interfaces/products/product.interface';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private readonly apiUrl = `${environment.apiBaseUrl}/products`;

  constructor(private readonly http: HttpClient) {}

  getProducts(filter: ProductFilter): Observable<{ items: Product[]; totalCount: number }> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.category) params = params.set('category', filter.category);
    if (filter.isActive !== undefined) params = params.set('isActive', filter.isActive.toString());

    return this.http.get<{ items: Product[]; totalCount: number }>(this.apiUrl, { params });
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getCategories(): Observable<ProductCategory[]> {
    return this.http.get<ProductCategory[]>(`${this.apiUrl}/categories`);
  }
}