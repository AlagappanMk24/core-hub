import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class CustomerGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
   const user = this.authService.getUserDetail();
    console.log('CustomerGuard: User details:', user); // Debug
    if (this.authService.hasRole('Customer') || this.authService.hasRole('Admin')) {
      console.log('CustomerGuard: Access granted for route:', state.url);
      return true;
    }
    console.log('CustomerGuard: Access denied, redirecting to /notfound');
    this.router.navigate(['/notfound']);
    return false;
  }
}