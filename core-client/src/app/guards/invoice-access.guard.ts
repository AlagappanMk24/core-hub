import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class InvoiceAccessGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const user = this.authService.getUserDetail();
    console.log('InvoiceAccessGuard: User details:', user);
    if (
      this.authService.hasRole('Admin') ||
      this.authService.hasRole('User') ||
      this.authService.hasRole('Customer')
    ) {
      console.log('InvoiceAccessGuard: Access granted for route:', state.url);
      return true;
    }
    console.log('InvoiceAccessGuard: Access denied, redirecting to /notfound');
    this.router.navigate(['/notfound']);
    return false;
  }
}