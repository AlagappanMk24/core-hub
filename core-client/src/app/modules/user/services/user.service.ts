// src/app/services/user/user.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';

export interface UserListDto {
  id: string;
  userName: string;
  email: string;
  fullName: string;
  roles: string[];
  companyId?: number;
  companyName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/user`;

  getUserList(): Observable<UserListDto[]> {
    return this.http.get<UserListDto[]>(`${this.apiUrl}/list`);
  }

  getUsersByCompany(companyId: number): Observable<UserListDto[]> {
    return this.http.get<UserListDto[]>(`${this.apiUrl}/company/${companyId}`);
  }

  getUsersByRole(role: string): Observable<UserListDto[]> {
    return this.http.get<UserListDto[]>(`${this.apiUrl}/role/${role}`);
  }
}