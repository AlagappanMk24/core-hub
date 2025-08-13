import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import { environment } from '../../environments/environment';

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address: {
    address1: string;
    address2: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
  };
}

export interface CustomerCreateDto {
  name: string;
  email: string;
  phoneNumber: string;
  address1: string;
  address2?: string;
  city: string;
  state?: string;
  country: string;
  zipCode: string;
}

export interface CustomerResponse {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address: {
    address1: string;
    address2?: string;
    city: string;
    state?: string;
    country: string;
    zipCode: string;
  };
}

interface PaginatedResponse {
  items: CustomerResponse[];
  total: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private apiUrl = `${environment.apiBaseUrl}/customer`;

  constructor(private http: HttpClient, private authService: AuthService) {}

  getCustomers(pageNumber: number = 1, pageSize: number = 10, search: string = ''): Observable<PaginatedResult<Customer>> {
    const token = this.authService.getAuthToken();
    if (!token) {
      return throwError(() => new Error('No authentication token found'));
    }
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<PaginatedResponse>(this.apiUrl, {
      headers: new HttpHeaders({ Authorization: `Bearer ${token}` }),
      params
    }).pipe(
      catchError(err => {
        console.error('Error fetching customers:', err);
        return throwError(() => new Error('Failed to fetch customers'));
      }),
      map(response => ({
        items: response.items.map(item => ({
          id: item.id,
          name: item.name,
          email: item.email,
          phoneNumber: item.phoneNumber,
          address: {
            address1: item.address.address1,
            address2: item.address.address2 || '',
            city: item.address.city,
            state: item.address.state || '',
            country: item.address.country,
            zipCode: item.address.zipCode
          }
        })),
        totalCount: response.total,
        pageNumber: response.page,
        pageSize: response.pageSize,
        totalPages: Math.ceil(response.total / response.pageSize)
      }))
    );
  }

  getCustomerById(id: number): Observable<Customer> {
    const token = this.authService.getAuthToken();
    if (!token) {
      return throwError(() => new Error('No authentication token found'));
    }
    return this.http.get<CustomerResponse>(`${this.apiUrl}/${id}`, {
      headers: new HttpHeaders({ Authorization: `Bearer ${token}` })
    }).pipe(
      catchError(err => {
        console.error('Error fetching customer:', err);
        return throwError(() => new Error('Failed to fetch customer'));
      }),
      map(response => ({
        id: response.id,
        name: response.name,
        email: response.email,
        phoneNumber: response.phoneNumber,
        address: {
          address1: response.address.address1,
          address2: response.address.address2 || '',
          city: response.address.city,
          state: response.address.state || '',
          country: response.address.country,
          zipCode: response.address.zipCode
        }
      }))
    );
  }

  createCustomer(customer: CustomerCreateDto): Observable<CustomerResponse> {
    const token = this.authService.getAuthToken();
    if (!token) {
      return throwError(() => new Error('No authentication token found'));
    }
    return this.http.post<CustomerResponse>(this.apiUrl, customer, {
      headers: new HttpHeaders({ Authorization: `Bearer ${token}` })
    }).pipe(
      catchError(err => {
        console.error('Error creating customer:', err);
        let errorMessage = 'Failed to create customer';
        if (err.status === 400 && err.error?.detail) {
          errorMessage = err.error.detail;
        }
        return throwError(() => new Error(errorMessage));
      })
    );
  }
}