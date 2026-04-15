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


  getAllProducts(): Observable<ProductResponse[]> {
    return this.http.get<ProductResponse[]>(`${this.apiBaseUrl}home`);
  }

  getTrendingProducts(): Observable<ProductResponse[]> {
    return this.http.get<ProductResponse[]>(`${this.apiBaseUrl}home/trending`,);
  }

 getCart(): Observable<CartViewModel> {
  return this.http.get<CartViewModel>(`${this.apiBaseUrl}cart`)
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
    return this.http.post<CartViewModel>(`${this.apiBaseUrl}cart`, request, );
  }

  updateCartItemCount(cartItemId: number, count: number): Observable<CartViewModel> {
     return this.http.put<CartViewModel>(`${this.apiBaseUrl}cart/${cartItemId}`, count,);
  }

  removeFromCart(cartItemId: number): Observable<CartViewModel> {
    return this.http.delete<CartViewModel>(`${this.apiBaseUrl}cart/${cartItemId}`, );
  }
}