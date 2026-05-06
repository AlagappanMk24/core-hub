import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router,
} from '@angular/router';
import { AuthService } from '../services/auth/auth.service';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class OtpGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    // Prevent access if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
      return false;
    }

    const otpToken = this.authService.getOtpToken();
    const otpIdentifier = route.queryParams['otpIdentifier'];

    if (otpToken && otpIdentifier) {
      try {
        const decoded: any = jwtDecode(otpToken);
        if (
          decoded.otp_purpose === 'verification' &&
          decoded.exp * 1000 > Date.now()
        ) {
          return true;
        }
      } catch {
        // Invalid token
      }
    }

    // Redirect to login if no valid OTP token or email mismatch
    this.authService.clearOtpToken();
    this.router.navigate(['/auth/login']);
    return false;
  }
}
