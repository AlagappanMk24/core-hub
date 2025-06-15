// forgot-password.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
  imports: [FormsModule, CommonModule]
})
export class ForgotPasswordComponent {
  email: string = '';
  isLoading: boolean = false;
  emailSent: boolean = false;
  errorMessage: string = '';

  constructor(private router: Router) {}

  onResetPassword(): void {
    // Reset error message
    this.errorMessage = '';

    // Basic validation
    if (!this.email) {
      this.errorMessage = 'Please enter your email address';
      this.showError(this.errorMessage);
      return;
    }

    // Email validation
    if (!this.isEmailValid()) {
      this.errorMessage = 'Please enter a valid email address';
      this.showError(this.errorMessage);
      return;
    }

    // Start loading
    this.isLoading = true;

    // Simulate API call to send reset email
    this.sendResetEmail();
  }

  private sendResetEmail(): void {
    // Simulate API call delay
    setTimeout(() => {
      try {
        // Here you would typically call your password reset service
        console.log('Password reset email sent to:', this.email);
        
        // Simulate successful email send
        this.emailSent = true;
        this.isLoading = false;
        
        // In a real application, you would call your backend service like:
        // this.authService.sendPasswordResetEmail(this.email).subscribe({
        //   next: (response) => {
        //     this.emailSent = true;
        //     this.isLoading = false;
        //   },
        //   error: (error) => {
        //     this.errorMessage = 'Failed to send reset email. Please try again.';
        //     this.isLoading = false;
        //     this.showError(this.errorMessage);
        //   }
        // });

      } catch (error) {
        this.errorMessage = 'An error occurred. Please try again.';
        this.isLoading = false;
        this.showError(this.errorMessage);
      }
    }, 2000); // Simulate 2 second delay
  }

  onResendEmail(): void {
    console.log('Resending password reset email to:', this.email);
    this.emailSent = false;
    this.onResetPassword();
  }

  goBackToLogin(): void {
    // Navigate back to login page
    this.router.navigate(['/auth/login']);
  }

  // Validation helpers
  isEmailValid(): boolean {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailPattern.test(this.email);
  }

  private showError(message: string): void {
    // You can implement a toast notification service here
    alert(message);
  }

  // Navigation methods
  onContactSupport(): void {
    console.log('Contact support clicked');
    // Implement contact support functionality
    // This could open a modal, navigate to support page, or open email client
    window.location.href = 'mailto:support@example.com?subject=Password Reset Help';
  }

  onCreateNewAccount(): void {
    console.log('Create new account clicked');
    // Navigate to registration page
    this.router.navigate(['/auth/register']);
  }

  // Additional utility methods
  resetForm(): void {
    this.email = '';
    this.isLoading = false;
    this.emailSent = false;
    this.errorMessage = '';
  }

  // Handle form submission on Enter key
  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !this.isLoading && !this.emailSent) {
      this.onResetPassword();
    }
  }

  // Format email for display (truncate if too long)
  getDisplayEmail(): string {
    if (this.email.length > 30) {
      return this.email.substring(0, 27) + '...';
    }
    return this.email;
  }

  // Check if form is valid for submission
  isFormValid(): boolean {
    return this.email.trim() !== '' && this.isEmailValid();
  }

  // Handle back navigation
  onBackToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  // Lifecycle hooks
  ngOnInit(): void {
    // Initialize component
    console.log('ForgotPasswordComponent initialized');
    
    // Check if email is passed as query parameter
    // This is useful if user clicks "Forgot Password" from login page
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras?.state?.['email']) {
      this.email = navigation.extras.state['email'];
    }
  }

  ngOnDestroy(): void {
    // Cleanup if needed
    console.log('ForgotPasswordComponent destroyed');
  }
}