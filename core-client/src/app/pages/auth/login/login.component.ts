// login.component.ts
import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { LoginRequest } from '../../../interfaces/auth/auth-request/login-request';
import { CommonModule } from '@angular/common';
import { jwtDecode } from 'jwt-decode';

/**
 * Component for handling user login with email/password and external providers.
 */
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  standalone: true,
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  loginForm: FormGroup;
  loading: boolean = false;
  loginError: string = '';
  loadingText: string = 'Signing in, please wait...';
  showPassword: boolean = false;

  /**
   * Initializes the login form with email and password validators.
   */
  constructor() {
    this.loginForm = this.fb.group({
      email: [
        '',
        [Validators.required, Validators.email, Validators.maxLength(255)],
      ],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(100),
          Validators.pattern(
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$/
          ),
        ],
      ],
    });
  }

  /**
   * Initializes query param subscription for external login callbacks.
   */
  ngOnInit(): void {
    // Reset login error when user starts typing
    this.loginForm.valueChanges.subscribe(() => {
      this.loginError = '';
    });

    // Handle external login callback
    this.activatedRoute.queryParams.subscribe((params) => {
      const authCode = params['code'];
      const provider = params['state'];
      if (authCode && provider) {
        this.handleExternalLogin(authCode, provider);
      }
    });
  }

    /**
   * Handle external login response
   */
  private handleExternalLogin(authCode: string, provider: string): void {
    this.loading = true;
    this.loadingText = `Processing ${provider} login...`;

    this.authService.externalLogin(authCode, provider).subscribe({
      next: (response) => {
        if (response.token) {
          localStorage.setItem('authToken', response.token);
          
          // Decode token to get user information
          const decoded: any = jwtDecode(response.token);
          console.log('Decoded token in login:', decoded);
          
          // Extract roles from token
          const roles = this.extractRoles(decoded);
          console.log('User roles:', roles);
          
          // Check if company selection is needed
          const companyId = decoded['companyId'];
          const customerId = decoded['customerId'];
          
          this.loadingText = 'Login successful! Redirecting...';
          
          setTimeout(() => {
            this.loading = false;
            this.redirectBasedOnRole(roles, companyId, customerId);
          }, 1500);
        } else {
          this.loading = false;
          this.loginError =
            response.message || 'External login failed. Please try again.';
        }
      },
      error: (error) => {
        this.loading = false;
        this.loginError =
          error.error?.message ||
          'An error occurred during external login.';
        console.error('External login error:', error);
      },
    });
  }

  
  /**
   * Extract roles from decoded token
   */
  private extractRoles(decoded: any): string[] {
    const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    let roles: string[] = [];
    
    if (decoded[roleClaim]) {
      roles = Array.isArray(decoded[roleClaim]) 
        ? decoded[roleClaim] 
        : [decoded[roleClaim]];
    }
    
    return roles;
  }

  /**
   * Redirect user based on their role and company selection status
   */
  private redirectBasedOnRole(roles: string[], companyId: string, customerId: string): void {
    // Check if company selection is needed
    if (!companyId || companyId === '0') {
      console.log('Company selection needed - redirecting to select-company');
      this.router.navigate(['/auth/select-company']);
      return;
    }

    // Customer role (and not Admin/User)
    if (roles.includes('Customer') && 
        !roles.includes('Admin') && 
        !roles.includes('User') && 
        !roles.includes('Super Admin')) {

      
      console.log('Redirecting customer to customer-dashboard');
      this.router.navigate(['/customer-dashboard']);
    } 
    // Admin, User, or Super Admin roles
    else if (roles.includes('Admin') || 
             roles.includes('User') || 
             roles.includes('Super Admin')) {
      
      console.log('Redirecting admin/user to dashboard');
      this.router.navigate(['/dashboard']);
    } 
    // Fallback for any other case
    else {
      console.error('Unknown user role:', roles);
      this.router.navigate(['/notfound']);
    }
  }

  /**
   * Handles form submission for email/password login.
   */
  onLogin(): void {
    this.loginError = '';

    // Validate form
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.setValidationError();
      return;
    }

    this.loading = true;
    this.loadingText = 'Signing in, please wait...';

    const loginData: LoginRequest = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
    };

    // Simulate delay for UX
    setTimeout(() => {
      // Attempt login
      this.authService.login(loginData).subscribe({
        next: (response) => {
          if (response.isSucceeded) {
            this.loadingText =
              response.message || 'Login successful. Redirecting...';
            setTimeout(() => {
              this.loading = false;
              this.router.navigate(['/auth/verify-otp'], {
                queryParams: { otpIdentifier: response.model?.otpIdentifier},
              });
            }, 2000);
          } else {
            this.loading = false;
            this.loginError =
              response.message || 'Login failed. Please try again.';
          }
        },
        error: (error) => {
          this.loading = false;
          // Handle specific error codes
          if (error.status === 400) {
            this.loginError =
              error.error?.message || 'Invalid email or password.';
          } else if (error.status === 403) {
            this.loginError = error.error?.message || 'Unauthorized access.';
          } else if (error.status === 429) {
            const retryAfter = error.error?.retryAfterSeconds || 60;
            this.loginError = `${
              error.error?.message ||
              'Too many login attempts. Please wait and try again.'
            } Try again in ${retryAfter} seconds.`;
          } else if (error.status === 500) {
            this.loginError = 'An error occurred while processing the request.';
          } else {
            this.loginError =
              error.error?.message || 'Login failed. Please try again.';
          }
          console.error('Login error:', error);
        },
      });
    }, 2000);
  }

  /**
   * Initiates login with an external provider.
   * @param provider The external provider (e.g., Google, GitHub).
   */
  loginWithProvider(provider: string): void {
    this.loading = true;
    this.loadingText = `Connecting to ${provider}...`;

    // Get OAuth2 redirect URL
    this.authService.getExternalLoginUrl(provider).subscribe({
      next: (response) => {
        window.location.href = response.redirectUrl;
      },
      error: (error) => {
        this.loading = false;
        this.loginError = `Failed to connect to ${provider}. Please try again.`;
        console.error(`External login failed for ${provider}:`, error);
      },
    });
  }

  /**
   * Toggles password visibility in the form.
   */
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  /**
   * Sets validation error message based on form errors.
   */
  private setValidationError(): void {
    const errors = [];
    const emailControl = this.loginForm.get('email');
    const passwordControl = this.loginForm.get('password');

    // Collect email validation errors
    if (emailControl?.errors) {
      if (emailControl.errors['required']) errors.push('Email is required');
      if (emailControl.errors['email']) errors.push('Invalid email format');
      if (emailControl.errors['maxlength'])
        errors.push('Email must be less than 255 characters');
    }

    // Collect password validation errors
    if (passwordControl?.errors) {
      if (passwordControl.errors['required'])
        errors.push('Password is required');
      if (
        passwordControl.errors['minlength'] ||
        passwordControl.errors['pattern']
      ) {
        errors.push(
          'Password must be at least 8 characters with one uppercase, one lowercase, one number, and one special character'
        );
      }
      if (passwordControl.errors['maxlength'])
        errors.push('Password must be less than 100 characters');
    }
    // Set error message
    if (errors.length > 0) {
      this.loginError = `Please check the following: ${errors.join(', ')}`;
    }
  }
}
