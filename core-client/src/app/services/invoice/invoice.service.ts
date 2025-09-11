import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import {
  Invoice,
  InvoiceApiResponse,
  InvoiceFilter,
  InvoiceSettings,
  InvoiceStats,
  InvoiceUpsert,
  PaginatedResult,
  TaxType,
  TaxTypeCreate,
} from '../../interfaces/invoice/invoice.interface';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class InvoiceService {
  private readonly apiUrl = `${environment.apiBaseUrl}/invoice`;

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService
  ) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getAuthToken();
    if (!token) {
      throw new Error('No authentication token found');
    }
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  private mapToInvoice(apiResponse: InvoiceApiResponse): Invoice {
    return {
      id: apiResponse.id,
      customerName: apiResponse.customer?.name || 'Unknown',
      invoiceNumber: apiResponse.invoiceNumber,
      invoiceStatus: apiResponse.invoiceStatus,
      paymentStatus: apiResponse.paymentStatus,
      totalAmount: apiResponse.totalAmount,
      issueDate: new Date(apiResponse.issueDate),
      dueDate: new Date(apiResponse.dueDate),
      poNumber: apiResponse.poNumber || '',
      projectDetail: apiResponse.projectDetail || '',
      items: apiResponse.items || [],
      taxDetails: apiResponse.taxDetails || [],
      discounts: apiResponse.discounts || [],
      paymentMethod: apiResponse.paymentMethod || '',
      notes: apiResponse.notes || '',
      customerId: apiResponse.customerId,
      currency: apiResponse.currency || 'USD',
      isAutomated: apiResponse.isAutomated || false,
      customer: apiResponse.customer,
      subtotal: apiResponse.subtotal,
    };
  }

  private handleError(error: unknown, message: string): Observable<never> {
    console.error(message, error);
    return throwError(() => new Error(message));
  }

  getPagedInvoices(filter: {
    pageNumber: number;
    pageSize: number;
    search?: string;
    invoiceStatus?: string;
    paymentStatus?: string;
    customerId?: number;
    taxType?: number;
    minAmount?: number;
    maxAmount?: number;
    invoiceNumberFrom?: string;
    invoiceNumberTo?: string;
    issueDateFrom?: string;
    issueDateTo?: string;
    dueDateFrom?: string;
    dueDateTo?: string;
  }): Observable<PaginatedResult<Invoice>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.invoiceStatus)
      params = params.set('invoiceStatus', filter.invoiceStatus);
    if (filter.paymentStatus)
      params = params.set('paymentStatus', filter.paymentStatus);
    if (filter.customerId)
      params = params.set('customerId', filter.customerId.toString());
    if (filter.taxType)
      params = params.set('taxType', filter.taxType.toString());
    if (filter.minAmount)
      params = params.set('minAmount', filter.minAmount.toString());
    if (filter.maxAmount)
      params = params.set('maxAmount', filter.maxAmount.toString());
    if (filter.invoiceNumberFrom)
      params = params.set('invoiceNumberFrom', filter.invoiceNumberFrom);
    if (filter.invoiceNumberTo)
      params = params.set('invoiceNumberTo', filter.invoiceNumberTo);
    if (filter.issueDateFrom)
      params = params.set('issueDateFrom', filter.issueDateFrom);
    if (filter.issueDateTo)
      params = params.set('issueDateTo', filter.issueDateTo);
    if (filter.dueDateFrom)
      params = params.set('dueDateFrom', filter.dueDateFrom);
    if (filter.dueDateTo) params = params.set('dueDateTo', filter.dueDateTo);

    return this.http.get<PaginatedResult<Invoice>>(this.apiUrl, {
      headers: this.getHeaders(),
      params,
    });
  }

  getInvoiceById(id: string): Observable<Invoice> {
    return this.http
      .get<InvoiceApiResponse>(`${this.apiUrl}/${id}`, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((apiResponse: InvoiceApiResponse) =>
          this.mapToInvoice(apiResponse)
        ),
        catchError((error) =>
          this.handleError(error, 'Failed to fetch invoice')
        )
      );
  }

  getInvoiceStats(): Observable<InvoiceStats> {
    return this.http
      .get<InvoiceStats>(`${this.apiUrl}/stats`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) => this.handleError(error, 'Failed to fetch stats'))
      );
  }

  createInvoice(invoice: InvoiceUpsert): Observable<{ model: number }> {
    return this.http
      .post<{ model: number }>(
        this.apiUrl,
        { invoiceDto: invoice },
        {
          headers: this.getHeaders(),
        }
      )
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to create invoice')
        )
      );
  }

  updateInvoice(invoice: InvoiceUpsert): Observable<{ model: number }> {
    if (!invoice.id) {
      return throwError(() => new Error('Invoice ID is required for update'));
    }
    return this.http
      .put<{ model: number }>(
        `${this.apiUrl}/${invoice.id}`,
        { invoiceDto: invoice },
        {
          headers: this.getHeaders(),
        }
      )
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to update invoice')
        )
      );
  }

  sendInvoice1(invoiceId: number): Observable<any> {
    const token = this.authService.getAuthToken();
    if (!token) {
      return throwError(() => new Error('No authentication token found'));
    }
    return this.http
      .post(
        `${this.apiUrl}/${invoiceId}/send`,
        {},
        {
          headers: new HttpHeaders({ Authorization: `Bearer ${token}` }),
        }
      )
      .pipe(
        catchError((err) => {
          console.error('Error sending invoice:', err);
          return throwError(() => new Error('Failed to send invoice'));
        })
      );
  }

  getTaxTypes(): Observable<TaxType[]> {
    return this.http
      .get<TaxType[]>(`${this.apiUrl}/tax-types`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch tax types')
        )
      );
  }

  createTaxType(taxType: TaxTypeCreate): Observable<TaxType> {
    return this.http
      .post<TaxType>(`${this.apiUrl}/tax-types`, taxType, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to create tax type')
        )
      );
  }

  downloadInvoicePdf(invoiceId: string): Observable<Blob> {
    return this.http
      .get(`${this.apiUrl}/${invoiceId}/pdf`, {
        headers: this.getHeaders(),
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to download invoice PDF')
        )
      );
  }

  sendInvoice(invoiceId: number, emailData: any): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/${invoiceId}/send`, emailData, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) => this.handleError(error, 'Failed to send invoice'))
      );
  }

  deleteInvoice(id: string): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to delete invoice')
        )
      );
  }

  duplicateInvoice(id: string): Observable<void> {
    return this.http
      .post<void>(
        `${this.apiUrl}/${id}/duplicate`,
        {},
        {
          headers: this.getHeaders(),
        }
      )
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to duplicate invoice')
        )
      );
  }

  getInvoiceSettings(): Observable<InvoiceSettings> {
    return this.http
      .get<InvoiceSettings>(`${this.apiUrl}/settings`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch invoice settings')
        )
      );
  }

  saveInvoiceSettings(settings: InvoiceSettings): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/settings`, settings, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to save invoice settings')
        )
      );
  }

  getNextInvoiceNumber(): Observable<string> {
    return this.http
      .get<string>(`${this.apiUrl}/next-invoice-number`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch next invoice number')
        )
      );
  }

  // exportInvoicesExcel(
  //   pageNumber: number,
  //   pageSize: number,
  //   search: string,
  //   invoiceStatus: string | null,
  //   paymentStatus: string | null
  // ): Observable<Blob> {
  //   let params: any = { pageNumber, pageSize };
  //   if (search) params.search = search;
  //   if (invoiceStatus) params.invoiceStatus = invoiceStatus;
  //   if (paymentStatus) params.paymentStatus = paymentStatus;
  //   return this.http.get(`${this.apiUrl}/export/excel`, {
  //     headers: this.getHeaders(),
  //     params,
  //     responseType: 'blob',
  //   });
  // }

  // exportInvoicesPdf(
  //   pageNumber: number,
  //   pageSize: number,
  //   search: string,
  //   invoiceStatus: string | null,
  //   paymentStatus: string | null
  // ): Observable<Blob> {
  //   let params: any = { pageNumber, pageSize };
  //   if (search) params.search = search;
  //   if (invoiceStatus) params.invoiceStatus = invoiceStatus;
  //   if (paymentStatus) params.paymentStatus = paymentStatus;
  //   return this.http.get(`${this.apiUrl}/export/pdf`, {
  //     headers: this.getHeaders(),
  //     params,
  //     responseType: 'blob',
  //   });
  // }

   exportInvoicesExcel(filter: InvoiceFilter): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/excel`, {
      params: { ...filter },
      responseType: 'blob',
    });
  }

  exportInvoicesPdf(filter: InvoiceFilter): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/pdf`, {
      params: { ...filter },
      responseType: 'blob',
    });
  }

  importInvoices(file: File): Observable<{
    successCount: number;
    errorCount: number;
    errors: string[];
  }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http
      .post<{ isSuccess: boolean; result: any; errorMessage: string }>(
        `${this.apiUrl}/import`,
        formData
      )
      .pipe(
        map((response) => {
          if (!response.isSuccess) {
            throw new Error(response.errorMessage);
          }
          return response.result;
        })
      );
  }

  downloadImportTemplate(): Observable<Blob> {
    return this.http
      .get(`${this.apiUrl}/template`, {
        headers: this.getHeaders(),
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to download import template')
        )
      );
  }
}
