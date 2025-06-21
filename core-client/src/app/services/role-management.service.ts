import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, of, throwError } from 'rxjs';
import { environment } from '../environments/environment.development';
import { Role } from '../interfaces/auth/role/role.interface';
import { Menu } from '../interfaces/auth/role/menu.interface';
import { Permission } from '../interfaces/auth/role/permission.interface';
import { RoleMenuPermission } from '../interfaces/auth/role/role-menu-permission.interface';

@Injectable({
  providedIn: 'root',
})

export class RoleManagementService {
  private apiUrl = environment.apiUrl;
  private menus: Menu[] = [
    { id: 'dashboard', name: 'Dashboard', icon: 'fas fa-tachometer-alt' },
    { id: 'users', name: 'Users', icon: 'fas fa-users' },
    { id: 'orders', name: 'Orders', icon: 'fas fa-shopping-cart' },
    { id: 'invoices', name: 'Invoices', icon: 'fas fa-file-invoice' },
    { id: 'customers', name: 'Customers', icon: 'fas fa-user-friends' }
  ];

  constructor(private http: HttpClient) {}

  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.apiUrl}roles`).pipe(
      catchError(this.handleError('getRoles'))
    );
  }

  getMenus(): Observable<Menu[]> {
    return of(this.menus);
  }

  getPermissions(): Observable<Permission[]> {
     return this.http.get<Permission[]>(`${this.apiUrl}roles/permissions`).pipe(
      catchError(this.handleError('getPermissions'))
    );
  }

  getRoleMenuPermissions(roleId: string): Observable<RoleMenuPermission[]> {
    return this.http
      .get<RoleMenuPermission[]>(`${this.apiUrl}roles/role-menu-permissions?roleId=${roleId}`)
      .pipe(catchError(this.handleError('getRoleMenuPermissions')));
  }

  saveRoleMenuPermissions(permissions: RoleMenuPermission[]): Observable<any> {
    // return this.http.post(`${this.apiUrl}roles/role-menu-permissions`, permissions);
     return this.http
      .post(`${this.apiUrl}roles/role-menu-permissions`, permissions)
      .pipe(catchError(this.handleError('saveRoleMenuPermissions')));
  }
  
  private handleError(operation: string) {
    return (error: HttpErrorResponse): Observable<never> => {
      let errorMessage = `Error in ${operation}: ${error.status} - ${error.message}`;
      if (error.status === 401) {
        errorMessage = 'Unauthorized access. Please log in again.';
      } else if (error.status === 403) {
        errorMessage = 'Forbidden. Admin role required.';
      } else if (error.status === 404) {
        errorMessage = 'API endpoint not found or login redirect occurred. Check authentication.';
      }
      console.error(errorMessage, error);
      return throwError(() => new Error(errorMessage));
    };
  }
}
