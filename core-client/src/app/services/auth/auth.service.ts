// AuthService.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
  baseApiUrl: string = environment.apiUrl;
  private tokenKey = 'authToken';
  private otpTokenKey = 'otpToken';
  private otpIdentifierKey = 'otpIdentifier';

  constructor(private http: HttpClient) {}

  /**
   * Login with email & password
   */
  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseApiUrl}auth/login`, data)
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
      .post<AuthResponse>(`${this.baseApiUrl}auth/register`, data)
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
    return this.http.post(`${this.baseApiUrl}auth/validate-otp`, data).pipe(
      map((response: any) => {
        if (response.isAuthenticated && response.token) {
          localStorage.setItem(this.tokenKey, response.token);
          this.clearOtpToken();
        }
        return response;
      })
    );
  }

  resendOtp(data: { otpIdentifier: string }): Observable<any> {
    return this.http.post(`${this.baseApiUrl}auth/resend-otp`, data);
  }
  /**
   * Get user details from JWT
   */
  getUserDetail = () => {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decodedToken: any = jwtDecode(token);
      const roles = Array.isArray(decodedToken.role)
        ? decodedToken.role
        : decodedToken.role
        ? [decodedToken.role]
        : [];
      return {
        id: decodedToken.nameid,
        fullName: decodedToken.name,
        email: decodedToken.email,
        roles: roles,
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
    const token = this.getToken();
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
    const token = this.getToken();
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
      `${this.baseApiUrl}auth/external-login-url?provider=${provider}`
    );
  }

  // Exchange authorization code for JWT token
  externalLogin(authCode: string, provider: string): Observable<any> {
    return this.http
      .post(`${this.baseApiUrl}auth/external-login`, {
        authorizationCode: authCode,
        provider,
      })
      .pipe(
        map((response: any) => {
          if (response.Token) {
            localStorage.setItem(this.tokenKey, response.Token);
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
  getToken = (): string | null => localStorage.getItem(this.tokenKey);

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
}
