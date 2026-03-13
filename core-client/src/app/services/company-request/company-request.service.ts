// services/company-request.service.ts
import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../auth/auth.service';

export interface CreateCompanyRequest {
  fullName: string;
  email: string;
  companyName: string;
}

export interface CompanyRequestResponse {
  id: number;
  fullName: string;
  email: string;
  companyName: string;
  requestedAt: Date;
  status: 'Pending' | 'Approved' | 'Rejected';
  processedAt?: Date;
  processedBy?: string;
  rejectionReason?: string;
}

export interface CompanyRequestListResponse {
  requests: CompanyRequestResponse[];
  totalCount: number;
  pendingCount: number;
  approvedCount: number;
  rejectedCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class CompanyRequestService {
  private apiUrl = `${environment.apiBaseUrl}/auth/request-company`;
  private adminApiUrl = `${environment.apiBaseUrl}/admin/company-requests`;
  authService = inject(AuthService);
  constructor(private http: HttpClient) {}

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
  // Public endpoints
  createRequest(request: CreateCompanyRequest): Observable<any> {
    return this.http.post(this.apiUrl, request, { headers: this.getHeaders() });
  }

  getRequestStatus(email: string): Observable<CompanyRequestResponse[]> {
    return this.http.get<CompanyRequestResponse[]>(`${this.apiUrl}/status`, {
      params: { email }
    });
  }

  // Admin endpoints
  getAllRequests(params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    search?: string;
  }): Observable<CompanyRequestListResponse> {
    return this.http.get<CompanyRequestListResponse>(this.adminApiUrl, { params, headers: this.getHeaders() });
  }

  getRequestById(id: number): Observable<CompanyRequestResponse> {
    return this.http.get<CompanyRequestResponse>(`${this.adminApiUrl}/${id}`, { headers: this.getHeaders() });
  }

  approveRequest(id: number): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${id}/approve`, {}, { headers: this.getHeaders() });
  }

  rejectRequest(id: number, reason: string): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${id}/reject`, { reason }, { headers: this.getHeaders() });
  }
}