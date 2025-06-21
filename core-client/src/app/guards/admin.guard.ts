import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    const user = this.authService.getUserDetail();
    if (user && user.roles.includes('Admin')) {
      return true;
    }
    this.router.navigate(['/auth/login']);
    return false;
  }
}