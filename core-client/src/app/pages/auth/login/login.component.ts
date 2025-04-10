import { Component, OnInit, inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    MatInputModule,
    RouterLink,
    MatSnackBarModule,
    MatIconModule,
    ReactiveFormsModule,
    CommonModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  authService = inject(AuthService);
  matSnackBar = inject(MatSnackBar);
  router = inject(Router);
  hide = true;
  form!: FormGroup;
  fb = inject(FormBuilder);
  loading: boolean = false;
  loginError: string = '';

  constructor(
    private activatedRoute: ActivatedRoute // FIXED: Changed `route` to `activatedRoute`
  ) {}

  login() {
    this.loading = true;
    this.authService.login(this.form.value).subscribe({
      next: () => {
        // Redirect to Verify OTP Page
        this.router.navigate(['/auth/verify-otp'], {
          queryParams: { email: this.form.value.email },
        });
      },
      error: (error) => {
        console.log(error, 'Error Response');
  
        this.loading = false;
  
        if (error.status === 400) {
          if (error.error?.errors) {
            // Extract validation errors
            const validationErrors = error.error.errors;
            this.loginError = Object.values(validationErrors).flat().join(' ');
          } else if (error.error?.message) {
            this.loginError = error.error.message;
          } else {
            this.loginError = 'Invalid login attempt. Please check your credentials.';
          }
        } else if (error.status === 403) {
          console.log('403 Forbidden Error');
          this.loginError = error.error?.message || 'Unauthorized access.';
        } else if (error.status === 500) {
          this.loginError = 'An error occurred while processing the request.';
        } else {
          this.loginError = error.error?.message || 'Login failed. Please try again.';
        }
      },
    });
  }
  
  // Initiate external login (Google, Microsoft, Facebook)
  loginWithProvider(provider: string) {
    this.loading = true; // Start loading effect

    this.authService.getExternalLoginUrl(provider).subscribe({
      next: (response: any) => {
        window.location.href = response.redirectUrl; // Redirect to external login page
      },
      error: (error) => {
        console.error('Login failed:', error);
        this.loading = false;
      },
    });

    // Keep loading state active until navigation is complete
    this.router.events.subscribe((event) => {
      if (event.constructor.name === 'NavigationEnd') {
        this.loading = false;
      }
    });
  }

  logout() {
    this.authService.logout();
    localStorage.removeItem('authToken');
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
    // Reset login error when user starts typing
    this.form.valueChanges.subscribe(() => {
      this.loginError = '';
    });
    this.activatedRoute.queryParams.subscribe(
      (params: { [key: string]: string }) => {
        // FIXED: Explicitly typed params
        const authCode = params['code'];
        const provider = params['state'];
        if (authCode && provider) {
          this.loading = true;
          this.authService.externalLogin(authCode, provider).subscribe({
            next: (response: any) => {
              if (response.token) {
                localStorage.setItem('authToken', response.token);
                this.router.navigate(['/dashboard']); // Redirect after login
              }
            },
            error: (error) => {
              console.error('Login failed:', error);
              this.loading = false;
            },
          });
        }
      }
    );
  }
}
