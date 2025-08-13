import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import { environment } from '../../environments/environment.development';

export interface EmailSettings {
  fromEmail: string;
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

  private handleError(error: unknown, message: string): Observable<never> {
    console.error(message, error);
    return throwError(() => new Error(message));
  }

  getEmailSettings(): Observable<EmailSettings> {
    return this.http
      .get<EmailSettings>(`${this.apiUrl}/settings`, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to fetch email settings')
        )
      );
  }

  saveEmailSettings(settings: EmailSettings): Observable<void> {
    return this.http
      .post<void>(`${this.apiUrl}/settings`, settings, {
        headers: this.getHeaders(),
      })
      .pipe(
        catchError((error) =>
          this.handleError(error, 'Failed to save email settings')
        )
      );
  }
}
