import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    // Check for both Admin and Super Admin roles
    if (this.authService.hasRole('Admin') || this.authService.hasRole('Super Admin')) {
      return true;
    }
     console.log('AdminGuard: User does not have Admin or Super Admin role');
    this.router.navigate(['/notfound']);
    return false;
  }
}