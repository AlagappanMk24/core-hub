import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth/auth.service';
import { environment } from '../../environments/environment.development';

export interface EmailSettings {
  fromEmail: string;
}

export interface SendEmailRequest {
  to: string;
  cc?: string;
  bcc?: string;
  subject: string;
  body: string;
  attachPdf: boolean;
  sendCopyToSelf: boolean;
  customerId: number;
  invoiceId?: number;
  type: 'statement' | 'invoice' | 'custom';
}

@Injectable({
  providedIn: 'root',
})
export class EmailService {
  private readonly apiUrl = `${environment.apiBaseUrl}/email`;

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService
  ) {}

  private handleError(error: unknown, message: string): Observable<never> {
    console.error(message, error);
    return throwError(() => new Error(message));
  }

  getEmailSettings(): Observable<EmailSettings> {
    return this.http
      .get<EmailSettings>(`${this.apiUrl}/settings`)
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch email settings')
        )
      );
  }

  saveEmailSettings(settings: EmailSettings): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/settings`, settings)
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to save email settings')
        )
      );
  }
   sendCustomerEmail(request: SendEmailRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-customer-email`, request, {
      headers: { Authorization: `Bearer ${this.authService.getAuthToken()}` },
    });
  }

  getEmailHistory(customerId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/history/${customerId}`, {
      headers: { Authorization: `Bearer ${this.authService.getAuthToken()}` },
    });
  }
}