import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment.development';
import { Company } from '../../interfaces/company/company.interface';
import { AuthService } from '../auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private apiBaseUrl = `${environment.apiBaseUrl}/company`;

  constructor(private http: HttpClient, private readonly authService: AuthService) {}

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

  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(this.apiBaseUrl, {headers: this.getHeaders()}).pipe(
      catchError(err => {
        console.error('Error fetching companies:', err);
        return throwError(() => new Error('Failed to fetch companies'));
      })
    );
  }

  getCompanyById(id: number): Observable<Company> {
    return this.http.get<Company>(`${this.apiBaseUrl}/${id}`, {headers: this.getHeaders()}).pipe(
      catchError(err => {
        console.error(`Error fetching company ${id}:`, err);
        return throwError(() => new Error(`Failed to fetch company with ID ${id}`));
      })
    );
  }

  createCompany(company: { name: string }): Observable<Company> {
    return this.http.post<Company>(this.apiBaseUrl, company, { headers: this.getHeaders() }).pipe(
      catchError(err => {
        console.error('Error creating company:', err);
        return throwError(() => new Error('Failed to create company'));
      })
    );
  }
}