import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth/auth.service';
import { environment } from '../../../environments/environment.development';
import {
  PaginatedResult,
  Customer,
  CustomerCreateDto,
  CustomerUpdateDto,
  CustomerResponseDto,
  PaginatedResponse,
  CustomerStats,
  CustomerFilterRequest,
  CustomerPayment,
  CustomerInvoice,
  CustomerActivity,
  SpendingTrend,
  Communication,
  CustomerNote,
} from './models/customer.model';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private readonly apiUrl = `${environment.apiBaseUrl}/customer`;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
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
    } else if (
      err.status === 500 &&
      err.error?.detail?.includes('Database Error')
    ) {
      errorMessage = 'Database error occurred';
    }
    return throwError(() => new Error(errorMessage));
  }

  private mapToCustomer(response: CustomerResponseDto): Customer {
    return {
      id: response.id,
      name: response.name,
      email: response.email,
      phoneNumber: response.phoneNumber,
      taxId: response.taxId,
      website: response.website,
      creditLimit: response.creditLimit,
      defaultPaymentTerms: response.defaultPaymentTerms,
      defaultCurrency: response.defaultCurrency,
      customerGroupId: response.customerGroupId,
      customerGroupName: response.customerGroupName,
      status: response.status,
      activeSince: response.activeSince,
      lastPurchaseDate: response.lastPurchaseDate,
      totalPurchases: response.totalPurchases,
      averagePaymentDays: response.averagePaymentDays,
      companyId: response.companyId,
      addressLine1: response.addressLine1,
      addressLine2: response.addressLine2,
      city: response.city,
      state: response.state,
      countryCode: response.countryCode,
      countryName: response.countryName,
      zipCode: response.zipCode,
      createdDate: response.createdDate,
      companyName: response.companyName,
      notes: [],
      tags: [],
    };
  }

  private buildQueryParams(paramsObj: {
    [key: string]: string | number | null | undefined | Date;
  }): HttpParams {
    let params = new HttpParams();
    Object.entries(paramsObj).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        if (value instanceof Date) {
          params = params.set(key, value.toISOString());
        } else {
          params = params.set(key, value.toString());
        }
      }
    });
    return params;
  }

  getCustomers(
    filter: CustomerFilterRequest,
  ): Observable<PaginatedResult<Customer>> {
    const params = this.buildQueryParams({
      pageNumber: filter.pageNumber,
      pageSize: filter.pageSize,
      search: filter.search?.trim(),
      status: filter.status,
      minTotalPurchases: filter.minTotalPurchases,
      maxTotalPurchases: filter.maxTotalPurchases,
      country: filter.country,
      customerGroup: filter.customerGroup,
      minCreditLimit: filter.minCreditLimit,
      maxCreditLimit: filter.maxCreditLimit,
      dateFrom: filter.dateFrom,
      dateTo: filter.dateTo,
    });

    return this.http
      .get<PaginatedResponse>(this.apiUrl, {
        headers: this.getAuthHeaders(),
        params,
      })
      .pipe(
        map((response) => ({
          items: response.items.map((item) => this.mapToCustomer(item)),
          totalCount: response.totalCount,
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalPages: response.totalPages,
        })),
        catchError((err) => this.handleError(err, 'Failed to fetch customers')),
      );
  }

  getCustomerById(id: number): Observable<Customer> {
    return this.http
      .get<CustomerResponseDto>(`${this.apiUrl}/${id}`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        map(this.mapToCustomer),
        catchError((err) => this.handleError(err, 'Failed to fetch customer')),
      );
  }

  getCustomerStats(): Observable<CustomerStats> {
    return this.http
      .get<CustomerStats>(`${this.apiUrl}/stats`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch customer statistics'),
        ),
      );
  }

  createCustomer(customer: CustomerCreateDto): Observable<Customer> {
    return this.http
      .post<CustomerResponseDto>(this.apiUrl, customer, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        map(this.mapToCustomer),
        catchError((err) => this.handleError(err, 'Failed to create customer')),
      );
  }

  updateCustomer(
    id: number,
    customer: CustomerUpdateDto,
  ): Observable<Customer> {
    return this.http
      .put<CustomerResponseDto>(`${this.apiUrl}/${id}`, customer, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        map(this.mapToCustomer),
        catchError((err) => this.handleError(err, 'Failed to update customer')),
      );
  }

  deleteCustomer(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) => this.handleError(err, 'Failed to delete customer')),
      );
  }

  exportCustomers(
    filter: CustomerFilterRequest,
    format: string,
  ): Observable<Blob> {
    const params = this.buildQueryParams({
      search: filter.search?.trim(),
      status: filter.status,
      format: format,
      minTotalPurchases: filter.minTotalPurchases,
      maxTotalPurchases: filter.maxTotalPurchases,
      country: filter.country,
      customerGroup: filter.customerGroup,
      minCreditLimit: filter.minCreditLimit,
      maxCreditLimit: filter.maxCreditLimit,
      dateFrom: filter.dateFrom,
      dateTo: filter.dateTo,
    });

    return this.http
      .get(`${this.apiUrl}/export/${format}`, {
        headers: this.getAuthHeaders(),
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, `Failed to export customers to ${format}`),
        ),
      );
  }

  exportSelectedCustomers(ids: number[]): Observable<Blob> {
    return this.http
      .post(
        `${this.apiUrl}/export/selected`,
        { ids },
        {
          headers: this.getAuthHeaders(),
          responseType: 'blob',
        },
      )
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to export selected customers'),
        ),
      );
  }

  exportCustomersExcel(filter: CustomerFilterRequest): Observable<Blob> {
    const params = this.buildQueryParams({
      search: filter.search?.trim(),
      status: filter.status,
    });
    return this.http
      .get(`${this.apiUrl}/export/excel`, {
        headers: this.getAuthHeaders(),
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to export customers to Excel'),
        ),
      );
  }

  exportCustomersPdf(filter: CustomerFilterRequest): Observable<Blob> {
    const params = this.buildQueryParams({
      search: filter.search?.trim(),
      status: filter.status,
    });
    return this.http
      .get(`${this.apiUrl}/export/pdf`, {
        headers: this.getAuthHeaders(),
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to export customers to PDF'),
        ),
      );
  }

  /**
   * Get all invoices for a specific customer
   */
  getCustomerInvoices(customerId: number): Observable<CustomerInvoice[]> {
    return this.http
      .get<CustomerInvoice[]>(`${this.apiUrl}/${customerId}/invoices`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch customer invoices'),
        ),
      );
  }
  /**
   * Get all payments for a specific customer
   */
  getCustomerPayments(customerId: number): Observable<CustomerPayment[]> {
    return this.http
      .get<CustomerPayment[]>(`${this.apiUrl}/${customerId}/payments`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch customer payments'),
        ),
      );
  }

  /**
   * Get recent activities for a specific customer
   */
  getCustomerActivities(customerId: number): Observable<CustomerActivity[]> {
    return this.http
      .get<CustomerActivity[]>(`${this.apiUrl}/${customerId}/activities`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch customer activities'),
        ),
      );
  }

  /**
   * Get spending trend for a specific customer
   */
  getCustomerSpendingTrend(customerId: number): Observable<SpendingTrend[]> {
    return this.http
      .get<SpendingTrend[]>(`${this.apiUrl}/${customerId}/spending-trend`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch spending trend'),
        ),
      );
  }

  getCustomerGroups(): Observable<string[]> {
    return this.http
      .get<string[]>(`${this.apiUrl}/groups`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch customer groups'),
        ),
      );
  }

  getCustomerCountries(): Observable<string[]> {
    return this.http
      .get<string[]>(`${this.apiUrl}/countries`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) => this.handleError(err, 'Failed to fetch countries')),
      );
  }

  generateStatement(customerId: number): Observable<Blob> {
    return this.http
      .get(`${this.apiUrl}/${customerId}/statement`, {
        headers: this.getAuthHeaders(),
        responseType: 'blob',
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to generate statement'),
        ),
      );
  }

  getCommunicationHistory(customerId: number): Observable<Communication[]> {
    return this.http
      .get<Communication[]>(`${this.apiUrl}/${customerId}/communications`, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to fetch communication history'),
        ),
      );
  }

  exportCustomerData(customerId: number): Observable<Blob> {
    return this.http
      .get(`${this.apiUrl}/${customerId}/export-data`, {
        headers: this.getAuthHeaders(),
        responseType: 'blob',
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to export customer data'),
        ),
      );
  }
  
  addCustomerNote(customerId: number, note: string): Observable<CustomerNote> {
    return this.http
      .post<CustomerNote>(
        `${this.apiUrl}/${customerId}/notes`,
        { note },
        {
          headers: this.getAuthHeaders(),
        },
      )
      .pipe(catchError((err) => this.handleError(err, 'Failed to add note')));
  }

  scheduleFollowUp(customerId: number, data: any): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/${customerId}/follow-up`, data, {
        headers: this.getAuthHeaders(),
      })
      .pipe(
        catchError((err) =>
          this.handleError(err, 'Failed to schedule follow-up'),
        ),
      );
  }
}
