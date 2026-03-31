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
    private readonly authService: AuthService,
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

  private getMultipartHeaders(): HttpHeaders {
    const token = this.authService.getAuthToken();
    if (!token) {
      throw new Error('No authentication token found');
    }
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  }

  private mapToInvoice(apiResponse: any): Invoice {
    return {
      id: apiResponse.id,
      invoiceNumber: apiResponse.invoiceNumber,
      poNumber: apiResponse.poNumber,
      issueDate: new Date(apiResponse.issueDate),
      dueDate: new Date(apiResponse.dueDate),
      sentDate: apiResponse.sentDate
        ? new Date(apiResponse.sentDate)
        : undefined,
      paidDate: apiResponse.paidDate
        ? new Date(apiResponse.paidDate)
        : undefined,
      invoiceStatus: apiResponse.invoiceStatus,
      paymentStatus: apiResponse.paymentStatus,
      type: apiResponse.type,
      customerId: apiResponse.customerId,
      companyId: apiResponse.companyId,
      currency: apiResponse.currency,
      currencyRate: apiResponse.currencyRate || 1,
      subtotal: apiResponse.subtotal,
      discountTotal: apiResponse.discountTotal || 0,
      taxTotal: apiResponse.taxTotal,
      shippingAmount: apiResponse.shippingAmount || 0,
      adjustmentAmount: apiResponse.adjustmentAmount || 0,
      adjustmentDescription: apiResponse.adjustmentDescription,
      totalAmount: apiResponse.totalAmount,
      amountPaid: apiResponse.amountPaid || 0,
      amountDue: apiResponse.amountDue,
      amountRefunded: apiResponse.amountRefunded || 0,
      paymentMethod: apiResponse.paymentMethod,
      paymentGateway: apiResponse.paymentGateway,
      paymentTerms: apiResponse.paymentTerms,
      paymentTransactionId: apiResponse.paymentTransactionId,
      notes: apiResponse.notes,
      customerNotes: apiResponse.customerNotes,
      internalNotes: apiResponse.internalNotes,
      termsAndConditions: apiResponse.termsAndConditions,
      footerNote: apiResponse.footerNote,
      projectDetail: apiResponse.projectDetail,
      isAutomated: apiResponse.isAutomated,
      isRecurring: apiResponse.isRecurring,
      recurringInvoiceId: apiResponse.recurringInvoiceId,
      sourceSystem: apiResponse.sourceSystem,
      customer: apiResponse.customer,
      items: apiResponse.items || [],
      taxDetails: apiResponse.taxDetails || [],
      discounts: apiResponse.discounts || [],
      invoiceAttachments: apiResponse.invoiceAttachments || [],
      payments: apiResponse.payments || [],
      auditLogs: apiResponse.auditLogs || [],
      createdDate: new Date(apiResponse.createdDate),
      createdBy: apiResponse.createdBy,
      updatedDate: apiResponse.updatedDate
        ? new Date(apiResponse.updatedDate)
        : undefined,
      updatedBy: apiResponse.updatedBy,
    };
  }

  private handleError(error: unknown, message: string): Observable<never> {
    console.error(message, error);
    return throwError(() => new Error(message));
  }

  getPagedInvoices(
    filter: InvoiceFilter,
  ): Observable<PaginatedResult<Invoice>> {
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

    return this.http
      .get<PaginatedResult<Invoice>>(this.apiUrl, {
        headers: this.getHeaders(),
        params,
      })
      .pipe(
        map((response) => ({
          ...response,
          items: response.items.map((item) => this.mapToInvoice(item)),
        })),
        catchError((error) =>
          this.handleError(error, 'Failed to fetch invoices'),
        ),
      );
  }

  getInvoiceById(id: Number): Observable<Invoice> {
    return this.http
      .get<InvoiceApiResponse>(`${this.apiUrl}/${id}`, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((apiResponse: InvoiceApiResponse) =>
          this.mapToInvoice(apiResponse),
        ),
        catchError((error) =>
          this.handleError(error, 'Failed to fetch invoice'),
        ),
      );
  }

  getInvoiceStats(): Observable<InvoiceStats> {
    return this.http
      .get<InvoiceStats>(`${this.apiUrl}/stats`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) => this.handleError(error, 'Failed to fetch stats')),
      );
  }

  createInvoice(formData: FormData): Observable<Invoice> {
    const headers = this.getMultipartHeaders();
    return this.http.post<any>(this.apiUrl, formData, { headers }).pipe(
      map((response) => this.mapToInvoice(response)),
      catchError((error) =>
        this.handleError(error, 'Failed to create invoice'),
      ),
    );
  }

  updateInvoice(formData: FormData): Observable<Invoice> {
    const invoiceId = formData.get('Id') || formData.get('id');
    if (!invoiceId) {
      return throwError(() => new Error('Invoice ID is required for update'));
    }
    const headers = this.getMultipartHeaders();
    return this.http
      .put<any>(`${this.apiUrl}/${invoiceId}`, formData, { headers })
      .pipe(
        map((response) => this.mapToInvoice(response)),
        catchError((error) =>
          this.handleError(error, 'Failed to update invoice'),
        ),
      );
  }

  deleteInvoice(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to delete invoice'),
        ),
      );
  }

  getTaxTypes(): Observable<TaxType[]> {
    return this.http
      .get<TaxType[]>(`${this.apiUrl}/tax-types`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch tax types'),
        ),
      );
  }

  createTaxType(taxType: TaxTypeCreate): Observable<TaxType> {
    return this.http
      .post<TaxType>(`${this.apiUrl}/tax-types`, taxType, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to create tax type'),
        ),
      );
  }

  downloadInvoicePdf(invoiceId: number): Observable<Blob> {
    return this.http
      .get(`${this.apiUrl}/${invoiceId}/pdf`, {
        headers: this.getHeaders(),
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to download invoice PDF'),
        ),
      );
  }

  sendInvoice(invoiceId: number, emailData: any): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/${invoiceId}/send`, emailData, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to send invoice'),
        ),
      );
  }

  duplicateInvoice(id: number): Observable<void> {
    return this.http
      .post<void>(
        `${this.apiUrl}/${id}/duplicate`,
        {},
        {
          headers: this.getHeaders(),
        },
      )
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to duplicate invoice'),
        ),
      );
  }

  getInvoiceSettings(): Observable<InvoiceSettings> {
    return this.http
      .get<InvoiceSettings>(`${this.apiUrl}/settings`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch invoice settings'),
        ),
      );
  }

  saveInvoiceSettings(settings: InvoiceSettings): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/settings`, settings, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to save invoice settings'),
        ),
      );
  }

  getNextInvoiceNumber(): Observable<string> {
    return this.http
      .get<string>(`${this.apiUrl}/next-invoice-number`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch next invoice number'),
        ),
      );
  }

  exportInvoicesExcel(filter: InvoiceFilter): Observable<Blob> {
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

    return this.http
      .get(`${this.apiUrl}/export/excel`, {
        headers: this.getHeaders(),
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to export invoices to Excel'),
        ),
      );
  }

  exportInvoicesPdf(filter: InvoiceFilter): Observable<Blob> {
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

    return this.http
      .get(`${this.apiUrl}/export/pdf`, {
        headers: this.getHeaders(),
        params,
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to export invoices to PDF'),
        ),
      );
  }

  importInvoices(file: File): Observable<{
    successCount: number;
    errorCount: number;
    errors: string[];
  }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http
      .post<{
        isSuccess: boolean;
        result: any;
        errorMessage: string;
      }>(`${this.apiUrl}/import`, formData)
      .pipe(
        map((response) => {
          if (!response.isSuccess) {
            throw new Error(response.errorMessage);
          }
          return response.result;
        }),
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
          this.handleError(error, 'Failed to download import template'),
        ),
      );
  }

  deleteAttachment(invoiceId: number, attachmentId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${invoiceId}/attachments/${attachmentId}`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to delete attachment'),
        ),
      );
  }
  getAttachment(fileUrl: string): Observable<Blob> {
    return this.http
      .get(fileUrl, {
        headers: this.getHeaders(),
        responseType: 'blob',
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch attachment'),
        ),
      );
  }
}
