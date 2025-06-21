// forgot-password.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
  imports: [FormsModule, CommonModule, RouterModule]
})
export class ForgotPasswordComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);

  email: string = '';
  isLoading: boolean = false;
  emailSent: boolean = false;
  errorMessage: string = '';
  loadingText: string = 'Sending reset link, please wait...';

  ngOnInit(): void {
    // Check if email is passed as query parameter
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras?.state?.['email']) {
      this.email = navigation.extras.state['email'];
    }
  }

  onResetPassword(): void {
    this.errorMessage = '';

    // Validate email
    if (!this.isFormValid()) {
      this.setValidationError();
      return;
    }

    this.isLoading = true;
    this.loadingText = 'Sending reset link, please wait...';

    this.authService.forgotPassword(this.email).subscribe({
      next: (response) => {
        if (response.isSuccess || response.isSucceeded) {
          this.loadingText = 'Email sent successfully...';
          setTimeout(() => {
            this.isLoading = false;
            this.emailSent = true;
          }, 2000);
        } else {
          this.isLoading = false;
          this.errorMessage = response.message || 'Failed to send reset email. Please try again.';
        }
      },
      error: (error) => {
        this.isLoading = false;
        if (error.status === 404) {
          this.errorMessage = 'Invalid email address.';
        } else if (error.status === 500) {
          this.errorMessage = 'An error occurred while processing the request.';
        } else {
          this.errorMessage = error.error?.message || 'Failed to send reset email. Please try again.';
        }
        console.error('Forgot password error:', error);
      }
    });
  }

  onResendEmail(): void {
    this.emailSent = false;
    this.onResetPassword();
  }

  goBackToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  onContactSupport(event: Event): void {
    event.preventDefault();
    window.location.href = 'mailto:support@example.com?subject=Password Reset Help';
  }

  onCreateNewAccount(): void {
    this.router.navigate(['/auth/register']);
  }

  resetForm(): void {
    this.email = '';
    this.isLoading = false;
    this.emailSent = false;
    this.errorMessage = '';
    this.loadingText = 'Sending reset link, please wait...';
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !this.isLoading && !this.emailSent) {
      this.onResetPassword();
    }
  }

  getDisplayEmail(): string {
    if (this.email.length > 30) {
      return this.email.substring(0, 27) + '...';
    }
    return this.email;
  }

  isFormValid(): boolean {
    return this.email.trim() !== '' && this.isEmailValid();
  }

  isEmailValid(): boolean {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailPattern.test(this.email) && this.email.length <= 255;
  }

  private setValidationError(): void {
    if (!this.email) {
      this.errorMessage = 'Email is required';
    } else if (!this.isEmailValid()) {
      this.errorMessage = 'Please enter a valid email address';
    } else if (this.email.length > 255) {
      this.errorMessage = 'Email must be less than 255 characters';
    }
  }
}