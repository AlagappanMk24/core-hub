// // guards/unauth.guard.ts
// import { Injectable } from '@angular/core';
// import { CanActivate, Router } from '@angular/router';
// import { AuthService } from '../services/auth/auth.service';

// @Injectable({
//   providedIn: 'root',
// })

// export class UnauthGuard implements CanActivate {
//   constructor(private authService: AuthService, private router: Router) {}

//   canActivate(): boolean {
//     if (this.authService.isAuthenticated()) {
//       this.router.navigate(['/dashboard']);
//       return false;
//     }
//     return true;
//   }
// }
// guards/unauth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class UnauthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    if (this.authService.isAuthenticated()) {
      const token = this.authService.getAuthToken();
      if (token) {
        const decoded: any = jwtDecode(token);
        const companyId = decoded['companyId'];
        
        // Allow access to select-company if companyId is missing
        if (route.routeConfig?.path === 'select-company' && (!companyId || companyId === '0')) {
          return true; // Allow access to select company
        }
      }
      
      // For all other cases, redirect to dashboard
      this.router.navigate(['/dashboard']);
      return false;
    }
    return true;
  }
}