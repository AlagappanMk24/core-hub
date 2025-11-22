import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { ProductResponse, CartViewModel, AddToCartRequest } from '../../interfaces/products/product.interface';
import { AuthService } from '../../services/auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiBaseUrl = `${environment.apiBaseUrl}`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getAuthToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    });
  }

  getAllProducts(): Observable<ProductResponse[]> {
    return this.http.get<ProductResponse[]>(`${this.apiBaseUrl}home`, { headers: this.getHeaders() });
  }

  getTrendingProducts(): Observable<ProductResponse[]> {
    return this.http.get<ProductResponse[]>(`${this.apiBaseUrl}home/trending`, { headers: this.getHeaders() });
  }

 getCart(): Observable<CartViewModel> {
  return this.http.get<CartViewModel>(`${this.apiBaseUrl}cart`, { headers: this.getHeaders() })
    .pipe(
      tap(response => console.log('GetCart Response:', response)),
      catchError(error => {
        console.error('GetCart Error:', error);
        if (error.status === 401) {
          this.authService.logout();
          window.location.href = '/auth/login?returnUrl=/cart';
        }
        return throwError(error);
      })
    );
}

  addToCart(request: AddToCartRequest): Observable<CartViewModel> {
    return this.http.post<CartViewModel>(`${this.apiBaseUrl}cart`, request, { headers: this.getHeaders() });
  }

  updateCartItemCount(cartItemId: number, count: number): Observable<CartViewModel> {
     return this.http.put<CartViewModel>(`${this.apiBaseUrl}cart/${cartItemId}`, count, { headers: this.getHeaders() });
  }

  removeFromCart(cartItemId: number): Observable<CartViewModel> {
    return this.http.delete<CartViewModel>(`${this.apiBaseUrl}cart/${cartItemId}`, { headers: this.getHeaders() });
  }
}