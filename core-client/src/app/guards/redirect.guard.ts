import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';
import { inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class RedirectGuard implements CanActivate {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | UrlTree {
    const user = this.authService.getUserDetail();
    if (!user || !user.roles) {
      return this.router.createUrlTree(['/notfound']);
    }
    if (user.roles.includes('Customer') && !user.roles.includes('Admin') && !user.roles.includes('User')) {
      return this.router.createUrlTree(['/customer-dashboard']);
    } else if (user.roles.includes('Admin') || user.roles.includes('User')) {
      return this.router.createUrlTree(['/dashboard']);
    }
    return this.router.createUrlTree(['/notfound']);
  }
}