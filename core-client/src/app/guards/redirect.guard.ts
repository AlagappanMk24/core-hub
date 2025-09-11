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
    console.log('RedirectGuard: User details:', user);
    if (!user || !user.roles) {
      console.log('RedirectGuard: No user or roles, redirecting to /notfound');
      return this.router.createUrlTree(['/notfound']);
    }
    console.log('RedirectGuard: Roles found:', user.roles);
    if (user.roles.includes('Customer') && !user.roles.includes('Admin') && !user.roles.includes('User')) {
      console.log('RedirectGuard: Redirecting to /customer-dashboard');
      return this.router.createUrlTree(['/customer-dashboard']);
    } else if (user.roles.includes('Admin') || user.roles.includes('User')) {
      console.log('RedirectGuard: Redirecting to /dashboard');
      return this.router.createUrlTree(['/dashboard']);
    }
    console.log('RedirectGuard: No matching roles, redirecting to /notfound');
    return this.router.createUrlTree(['/notfound']);
  }
}