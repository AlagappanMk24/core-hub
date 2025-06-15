// login.component.ts
import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginRequest } from '../../../interfaces/auth/auth-request/login-request';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  standalone: true,
})
export class LoginComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  fb = inject(FormBuilder);

  loginForm: FormGroup;
  loading: boolean = false;
  loginError: string = '';
  loadingText: string = 'Signing in, please wait...';
  showPassword: boolean = false;

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
        this.loading = true;
        this.loadingText = 'Processing external login...';
        this.authService.externalLogin(authCode, provider).subscribe({
          next: (response) => {
            if (response.isSuccess && response.token) {
              this.loadingText = 'Navigating to dashboard...';
              setTimeout(() => {
                this.loading = false;
                this.router.navigate(['/dashboard']);
              }, 2000);
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
    });
  }

  onLogin(): void {
    this.loginError = '';

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

    setTimeout(() => {
      this.authService.login(loginData).subscribe({
        next: (response) => {
          console.log(response, 'res');
          if (response.isSucceeded) {
            this.loadingText =
              response.message || 'Login successful. Redirecting...';
            setTimeout(() => {
              this.loading = false;
              this.router.navigate(['/auth/verify-otp'], {
                queryParams: { email: this.loginForm.value.email },
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
          if (error.status === 400) {
            this.loginError =
              error.error?.message || 'Invalid email or password.';
          } else if (error.status === 403) {
            this.loginError = error.error?.message || 'Unauthorized access.';
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

  loginWithProvider(provider: string): void {
    this.loading = true;
    this.loadingText = `Connecting to ${provider}...`;

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

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  private setValidationError(): void {
    const errors = [];
    const emailControl = this.loginForm.get('email');
    const passwordControl = this.loginForm.get('password');

    if (emailControl?.errors) {
      if (emailControl.errors['required']) errors.push('Email is required');
      if (emailControl.errors['email']) errors.push('Invalid email format');
      if (emailControl.errors['maxlength'])
        errors.push('Email must be less than 255 characters');
    }
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

    if (errors.length > 0) {
      this.loginError = `Please check the following: ${errors.join(', ')}`;
    }
  }
}
