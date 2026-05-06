// recurring-invoice.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment.development';
import {
  PaginatedRecurringInvoices,
  RecurringInvoice,
  RecurringInvoiceFilter,
  RecurringInvoiceInstance,
  RecurringInvoiceInstanceFilter,
  RecurringInvoiceStats,
  StatusCounts,
} from '../../../../interfaces/invoice/recurring-invoice/recurring-invoice.interface';

@Injectable({
  providedIn: 'root',
})
export class RecurringInvoiceService {
  private readonly apiUrl = `${environment.apiBaseUrl}/recurring-invoices`;

  constructor(private readonly http: HttpClient) {}

  private mapToRecurringInvoice(apiResponse: any): RecurringInvoice {
    return {
      ...apiResponse,
      startDate: apiResponse.startDate
        ? new Date(apiResponse.startDate)
        : undefined,
      endDate: apiResponse.endDate ? new Date(apiResponse.endDate) : undefined,
      pausedDate: apiResponse.pausedDate
        ? new Date(apiResponse.pausedDate)
        : undefined,
      cancelledDate: apiResponse.cancelledDate
        ? new Date(apiResponse.cancelledDate)
        : undefined,
      nextInvoiceDate: apiResponse.nextInvoiceDate
        ? new Date(apiResponse.nextInvoiceDate)
        : undefined,
      lastInvoiceDate: apiResponse.lastInvoiceDate
        ? new Date(apiResponse.lastInvoiceDate)
        : undefined,
      createdDate: apiResponse.createdDate
        ? new Date(apiResponse.createdDate)
        : undefined,
      updatedDate: apiResponse.updatedDate
        ? new Date(apiResponse.updatedDate)
        : undefined,
      customerName: apiResponse.customer?.name || 'Unknown Customer',
      generatedInvoices: apiResponse.generatedInvoices?.map(
        (instance: any) => ({
          ...instance,
          generatedDate: new Date(instance.generatedDate),
          scheduledGenerationDate: new Date(instance.scheduledGenerationDate),
        }),
      ),
    };
  }

  private handleError(error: any, message: string): Observable<never> {
    console.error(`${message}:`, error);
    return throwError(() => error || new Error(message));
  }

  getPagedRecurringInvoices(
    filter: RecurringInvoiceFilter,
  ): Observable<PaginatedRecurringInvoices> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.frequency) params = params.set('frequency', filter.frequency);
    if (filter.customerId)
      params = params.set('customerId', filter.customerId.toString());
    if (filter.nextDateFrom)
      params = params.set('nextDateFrom', filter.nextDateFrom);
    if (filter.nextDateTo) params = params.set('nextDateTo', filter.nextDateTo);
    if (filter.startDateFrom)
      params = params.set('startDateFrom', filter.startDateFrom);
    if (filter.startDateTo)
      params = params.set('startDateTo', filter.startDateTo);
    if (filter.endDateFrom)
      params = params.set('endDateFrom', filter.endDateFrom);
    if (filter.endDateTo) params = params.set('endDateTo', filter.endDateTo);
    if (filter.minAmount)
      params = params.set('minAmount', filter.minAmount.toString());
    if (filter.maxAmount)
      params = params.set('maxAmount', filter.maxAmount.toString());
    if (filter.autoSend !== undefined)
      params = params.set('autoSend', filter.autoSend.toString());
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
    if (filter.sortOrder) params = params.set('sortOrder', filter.sortOrder);

    return this.http
      .get<PaginatedRecurringInvoices>(this.apiUrl, { params })
      .pipe(
          map((response) => {
          console.log('API Response:', response);
          return {
            ...response,
            items: response.items?.map((item) => this.mapToRecurringInvoice(item)) || [],
          };
        }),
        catchError((error) =>
          this.handleError(error, 'Failed to fetch recurring invoices'),
        ),
      );
  }

  getRecurringInvoiceById(id: number): Observable<RecurringInvoice> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map((response) => this.mapToRecurringInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to fetch recurring invoice'),
      ),
    );
  }

  getStatusCounts(): Observable<StatusCounts> {
    return this.http
      .get<StatusCounts>(`${this.apiUrl}/status-counts`)
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch status counts'),
        ),
      );
  }

  getStats(): Observable<RecurringInvoiceStats> {
    return this.http
      .get<RecurringInvoiceStats>(`${this.apiUrl}/stats`)
      .pipe(
        catchError((error) => this.handleError(error, 'Failed to fetch stats')),
      );
  }

  createRecurringInvoice(formData: FormData): Observable<RecurringInvoice> {
    return this.http.post<any>(this.apiUrl, formData).pipe(
      map((response) => this.mapToRecurringInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to create recurring invoice'),
      ),
    );
  }

  updateRecurringInvoice(
    id: number,
    formData: FormData,
  ): Observable<RecurringInvoice> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, formData).pipe(
      map((response) => this.mapToRecurringInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to update recurring invoice'),
      ),
    );
  }

  deleteRecurringInvoice(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to delete recurring invoice'),
        ),
      );
  }

  pauseRecurringInvoice(id: number): Observable<RecurringInvoice> {
    return this.http.post<any>(`${this.apiUrl}/${id}/pause`, {}).pipe(
      map((response) => this.mapToRecurringInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to pause recurring invoice'),
      ),
    );
  }

  resumeRecurringInvoice(id: number): Observable<RecurringInvoice> {
    return this.http.post<any>(`${this.apiUrl}/${id}/resume`, {}).pipe(
      map((response) => this.mapToRecurringInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to resume recurring invoice'),
      ),
    );
  }

  cancelRecurringInvoice(id: number): Observable<RecurringInvoice> {
    return this.http.post<any>(`${this.apiUrl}/${id}/cancel`, {}).pipe(
      map((response) => this.mapToRecurringInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to cancel recurring invoice'),
      ),
    );
  }

  generateNow(id: number): Observable<RecurringInvoiceInstance> {
    return this.http.post<any>(`${this.apiUrl}/${id}/generate-now`, {}).pipe(
      map((response) => ({
        ...response,
        generatedDate: new Date(response.generatedDate),
        scheduledGenerationDate: new Date(response.scheduledGenerationDate),
      })),
      catchError((error) =>
        this.handleError(error, 'Failed to generate invoice now'),
      ),
    );
  }

  getGeneratedInstances(
    filter: RecurringInvoiceInstanceFilter,
  ): Observable<{ items: RecurringInvoiceInstance[]; totalCount: number }> {
    let params = new HttpParams()
      .set('recurringInvoiceId', filter.recurringInvoiceId.toString())
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/instances`, { params }).pipe(
      map((response) => ({
        ...response,
        items: response.items.map((instance: any) => ({
          ...instance,
          generatedDate: new Date(instance.generatedDate),
          scheduledGenerationDate: new Date(instance.scheduledGenerationDate),
        })),
      })),
      catchError((error) =>
        this.handleError(error, 'Failed to fetch generated instances'),
      ),
    );
  }

  exportRecurringInvoicesExcel(
    filter: RecurringInvoiceFilter,
  ): Observable<Blob> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.frequency) params = params.set('frequency', filter.frequency);
    if (filter.customerId)
      params = params.set('customerId', filter.customerId.toString());

    return this.http
      .get(`${this.apiUrl}/export/excel`, {
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(
            error,
            'Failed to export recurring invoices to Excel',
          ),
        ),
      );
  }

  exportRecurringInvoicesPdf(filter: RecurringInvoiceFilter): Observable<Blob> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.status) params = params.set('status', filter.status);
    if (filter.frequency) params = params.set('frequency', filter.frequency);
    if (filter.customerId)
      params = params.set('customerId', filter.customerId.toString());

    return this.http
      .get(`${this.apiUrl}/export/pdf`, {
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to export recurring invoices to PDF'),
        ),
      );
  }

  getFrequencies(): Observable<{ value: string; label: string }[]> {
    return this.http
      .get<{ value: string; label: string }[]>(`${this.apiUrl}/frequencies`)
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch frequencies'),
        ),
      );
  }

  getStatuses(): Observable<{ value: string; label: string }[]> {
    return this.http
      .get<{ value: string; label: string }[]>(`${this.apiUrl}/statuses`)
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch statuses'),
        ),
      );
  }
}
