// AuthService.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment.development';
import { LoginRequest } from '../../interfaces/auth/auth-request/login-request';
import { AuthResponse } from '../../interfaces/auth/auth-response/auth-response';
import { RegisterRequest } from '../../interfaces/auth/auth-request/register-request';
import { ResetPasswordRequest } from '../../interfaces/auth/auth-request/resetpassword-request';
import { ChangePasswordRequest } from '../../interfaces/auth/auth-request/changepassword-request';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  baseApiUrl: string = environment.apiBaseUrl;
  private tokenKey = 'authToken';
  private otpTokenKey = 'otpToken';
  private otpIdentifierKey = 'otpIdentifier';

  constructor(private http: HttpClient) {}

  /**
   * Login with email & password
   */
  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseApiUrl}/auth/login`, data)
      .pipe(
        map((response) => {
          if (
            response.isSucceeded &&
            response.model?.otpToken &&
            response.model?.otpIdentifier
          ) {
            localStorage.setItem(this.otpTokenKey, response.model.otpToken);
            localStorage.setItem(
              this.otpIdentifierKey,
              response.model.otpIdentifier
            );
          }
          return response;
        })
      );
  }

  /**
   * Register a new user
   */
  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseApiUrl}/auth/register`, data)
      .pipe(
        map((response) => {
          return response;
        })
      );
  }

  verifyOtp(data: {
    otpIdentifier: string;
    otp: string;
    otpToken: string;
  }): Observable<any> {
    return this.http.post(`${this.baseApiUrl}/auth/validate-otp`, data).pipe(
      map((response: any) => {
        if (response.isAuthenticated && response.token) {
          localStorage.setItem(this.tokenKey, response.token);
          // const decoded: any = jwtDecode(response.token);
          // console.log(decoded);
          // if (!decoded.companyId) {
          //   // Redirect to company selection if no companyId
          //   window.location.href = '/auth/select-company';
          // }
          this.clearOtpToken();
          const decoded: any = jwtDecode(response.token);
          const roles = Array.isArray(decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
            ? decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            : decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            ? [decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']]
            : [];
        }
        return response;
      })
    );
  }

  resendOtp(data: { otpIdentifier: string }): Observable<any> {
    return this.http.post(`${this.baseApiUrl}/auth/resend-otp`, data);
  }
  /**
   * Get user details from JWT
   */
  getUserDetail = () => {
    const token = this.getAuthToken();
    if (!token) return null;
    try {
      // const decodedToken: any = jwtDecode(token);
      // const roles = Array.isArray(decodedToken.role)
      //   ? decodedToken.role
      //   : decodedToken.role
      //   ? [decodedToken.role]
      //   : [];
      // return {
      //   id: decodedToken.nameid,
      //   fullName: decodedToken.name,
      //   email: decodedToken.email,
      //   roles: roles,
      //   companyId: decodedToken.companyId,
      // };
      const decodedToken: any = jwtDecode(token);
      const roles = Array.isArray(decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
        ? decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        : decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        ? [decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']]
        : [];
      return {
        id: decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        fullName: decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        email: decodedToken['email'],
        roles: roles,
        companyId: decodedToken['companyId'],
      };
    } catch (error) {
      console.error('Error decoding token:', error);
      this.logout();
      return null;
    }
  };

  isAuthenticated(): boolean {
    return this.isLoggedIn();
  }
  /**
   * Check if the user is logged in
   */
  isLoggedIn = (): boolean => {
    const token = this.getAuthToken();
    if (!token) return false;
    return !this.isTokenExpired();
  };

  getLoggedInUser() {
    return JSON.parse(localStorage.getItem('user') || '{}');
  }
  /**
   * Check if JWT token is expired
   */
  private isTokenExpired() {
    const token = this.getAuthToken();
    if (!token) return true;
    try {
      const decoded: any = jwtDecode(token);
      const isExpired = Date.now() >= decoded['exp'] * 1000;
      if (isExpired) this.logout();
      return isExpired;
    } catch (error) {
      console.error('Invalid token format:', error);
      this.logout();
      return true;
    }
  }

  // Get external login URL
  getExternalLoginUrl(provider: string): Observable<any> {
    return this.http.get(
      `${this.baseApiUrl}/auth/external-login-url?provider=${provider}`
    );
  }

  // Exchange authorization code for JWT token
  externalLogin(authCode: string, provider: string): Observable<any> {
    return this.http
      .post(`${this.baseApiUrl}/auth/external-login`, {
        authorizationCode: authCode,
        provider,
      })
      .pipe(
        map((response: any) => {
          if (response.token) {
            localStorage.setItem(this.tokenKey, response.token);
            const decoded: any = jwtDecode(response.token);
            if (!decoded.companyId) {
              // Redirect to company selection if no companyId
              window.location.href = '/auth/select-company';
            }
          }
          return response;
        })
      );
  }

  /**
   * Logout
   */
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem('user');
  }

  /**
   * Get JWT token from local storage
   */
  getAuthToken(): string | null {
    const token = localStorage.getItem('authToken');
    return token;
  }

  getOtpToken = (): string | null => localStorage.getItem(this.otpTokenKey);

  getOtpIdentifier = (): string | null =>
    localStorage.getItem(this.otpIdentifierKey);

  clearOtpToken(): void {
    localStorage.removeItem(this.otpTokenKey);
    localStorage.removeItem(this.otpIdentifierKey);
  }

  /**
   * Forgot password
   */
  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.baseApiUrl}auth/forgot-password`, { email });
  }

  /**
   * Reset password
   */
  resetPassword(data: ResetPasswordRequest): Observable<any> {
    return this.http.put(`${this.baseApiUrl}auth/reset-password`, data);
  }

  /**
   * Change password
   */
  changePassword(data: ChangePasswordRequest): Observable<any> {
    return this.http.put(`${this.baseApiUrl}account/change-password`, data);
  }

  // Check if user has a specific role
  hasRole(role: string): boolean {
    const user = this.getUserDetail();
    return user?.roles.includes(role) || false;
  }

  updateCompany(companyId: number): Observable<any> {
    return this.http.post(`${this.baseApiUrl}auth/update-company`, {
      companyId,
    });
  }
  
  /**
   * Requests a new company to be added by sending an email to the admin.
   * @param request - Object containing user details and requested company name
   * @returns Observable<any> - Response from the API
   */
  requestCompany(request: { fullName: string; email: string; companyName: string }): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.baseApiUrl}/request-company`, request, { headers });
  }
}
