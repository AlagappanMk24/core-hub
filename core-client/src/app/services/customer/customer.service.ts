import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import { environment } from '../../environments/environment';
import {
  PaginatedResult,
  Customer,
  CustomerCreateDto,
  CustomerUpdateDto,
  CustomerResponse,
  PaginatedResponse,
  CustomerStats,
  CustomerFilterRequest
} from './models/customer.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private readonly apiUrl = `${environment.apiBaseUrl}/customer`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAuthToken();
    if (!token) {
      throw new Error('No authentication token found');
    }
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  private handleError(err: any, defaultMessage: string): Observable<never> {
    console.error(`Error: ${defaultMessage}`, err);
    let errorMessage = defaultMessage;
    if (err.status === 401) {
      errorMessage = 'Unauthorized: Invalid or expired token';
    } else if (err.status === 400 && err.error?.detail) {
      errorMessage = err.error.detail;
    } else if (err.status === 404 && err.error?.detail) {
      errorMessage = err.error.detail;
    } else if (err.status === 500 && err.error?.detail?.includes('Database Error')) {
      errorMessage = 'Database error occurred';
    }
    return throwError(() => new Error(errorMessage));
  }

  private mapToCustomer(response: CustomerResponse): Customer {
    return {
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
    };
  }

  private buildQueryParams(paramsObj: { [key: string]: string | number| null | undefined }): HttpParams {
    let params = new HttpParams();
    Object.entries(paramsObj).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, value.toString());
      }
    });
    return params;
  }

 getCustomers(filter: CustomerFilterRequest): Observable<PaginatedResult<Customer>> {
    const params = this.buildQueryParams({
      pageNumber: filter.pageNumber,
      pageSize: filter.pageSize,
      search: filter.search?.trim(),
      status: filter.status
    });

    return this.http.get<PaginatedResponse>(this.apiUrl, {
      headers: this.getAuthHeaders(),
      params
    }).pipe(
      map(response => ({
        items: response.items.map(item => this.mapToCustomer(item)),
        totalCount: response.total,
        pageNumber: response.page,
        pageSize: response.pageSize,
        totalPages: Math.ceil(response.total / response.pageSize)
      })),
      catchError(err => this.handleError(err, 'Failed to fetch customers'))
    );
  }


  getCustomerById(id: number): Observable<Customer> {
    return this.http.get<CustomerResponse>(`${this.apiUrl}/${id}`, {
      headers: this.getAuthHeaders()
    }).pipe(
      map(this.mapToCustomer),
      catchError(err => this.handleError(err, 'Failed to fetch customer'))
    );
  }

  getCustomerStats(): Observable<CustomerStats> {
    return this.http.get<CustomerStats>(`${this.apiUrl}/stats`, {
      headers: this.getAuthHeaders()
    }).pipe(
      catchError(err => this.handleError(err, 'Failed to fetch customer statistics'))
    );
  }

  createCustomer(customer: CustomerCreateDto): Observable<Customer> {
    return this.http.post<CustomerResponse>(this.apiUrl, customer, {
      headers: this.getAuthHeaders()
    }).pipe(
      map(this.mapToCustomer),
      catchError(err => this.handleError(err, 'Failed to create customer'))
    );
  }

  updateCustomer(id: number, customer: CustomerUpdateDto): Observable<Customer> {
    return this.http.put<CustomerResponse>(`${this.apiUrl}/${id}`, customer, {
      headers: this.getAuthHeaders()
    }).pipe(
      map(this.mapToCustomer),
      catchError(err => this.handleError(err, 'Failed to update customer'))
    );
  }

  deleteCustomer(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getAuthHeaders()
    }).pipe(
      catchError(err => this.handleError(err, 'Failed to delete customer'))
    );
  }

  exportCustomersExcel(search: string = ''): Observable<Blob> {
    const params = this.buildQueryParams({ search: search.trim() });
    return this.http.get(`${this.apiUrl}/export/excel`, {
      headers: this.getAuthHeaders(),
      params,
      responseType: 'blob'
    }).pipe(
      catchError(err => this.handleError(err, 'Failed to export customers to Excel'))
    );
  }

  exportCustomersPdf(search: string = ''): Observable<Blob> {
    const params = this.buildQueryParams({ search: search.trim() });
    return this.http.get(`${this.apiUrl}/export/pdf`, {
      headers: this.getAuthHeaders(),
      params,
      responseType: 'blob'
    }).pipe(
      catchError(err => this.handleError(err, 'Failed to export customers to PDF'))
    );
  }
}