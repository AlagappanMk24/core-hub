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
    if (
      this.authService.hasRole('Admin') ||
      this.authService.hasRole('User') ||
      this.authService.hasRole('Customer')
    ) {
      return true;
    }
    this.router.navigate(['/notfound']);
    return false;
  }
}