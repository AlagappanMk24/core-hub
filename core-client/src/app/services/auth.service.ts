// AuthService.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../environments/environment.development';
import { LoginRequest } from '../interfaces/auth/auth-request/login-request';
import { AuthResponse } from '../interfaces/auth/auth-response/auth-response';
import { RegisterRequest } from '../interfaces/auth/auth-request/register-request';
import { ResetPasswordRequest } from '../interfaces/auth/auth-request/resetpassword-request';
import { ChangePasswordRequest } from '../interfaces/auth/auth-request/changepassword-request';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  baseApiUrl: string = environment.apiUrl;
  private tokenKey = 'authToken';

  constructor(private http: HttpClient) {}

  /**
   * Login with email & password
   */
  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseApiUrl}auth/login`, data)
      .pipe(
        map((response) => {
          if (response.isSucceeded) {
            localStorage.setItem(this.tokenKey, response.token);
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

  verifyOtp(data: { email: string; otp: string }): Observable<any> {
    return this.http.post(`${this.baseApiUrl}auth/validate-otp`, data);
  }

  resendOtp(data: { email: string }): Observable<any> {
    return this.http.post(`${this.baseApiUrl}auth/resend-otp`, data);
  }
  /**
   * Get user details from JWT
   */
  getUserDetail = () => {
    const token = this.getToken();
    if (!token) return null;
    const decodedToken: any = jwtDecode(token);
    return {
      id: decodedToken.nameid,
      fullName: decodedToken.name,
      email: decodedToken.email,
      roles: decodedToken.role || [],
    };
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
    const decoded: any = jwtDecode(token);
    const isExpired = Date.now() >= decoded['exp'] * 1000;
    if (isExpired) this.logout();
    return isExpired;
  }

  // Get external login URL
  getExternalLoginUrl(provider: string): Observable<any> {
    return this.http.get(
      `${this.baseApiUrl}auth/external-login-url?provider=${provider}`
    );
  }

  // Exchange authorization code for JWT token
  externalLogin(authCode: string, provider: string): Observable<any> {
    return this.http.post(`${this.baseApiUrl}auth/external-login`, {
      authorizationCode: authCode,
      provider,
    });
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
  private getToken = (): string | null => localStorage.getItem(this.tokenKey);

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
}
