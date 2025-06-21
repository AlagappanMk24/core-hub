import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { ResetPasswordRequest } from '../../../interfaces/auth/auth-request/resetpassword-request';
import { CommonModule } from '@angular/common';

// Custom validator for password confirmation
export function passwordMatchValidator(
  control: AbstractControl
): ValidationErrors | null {
  const newPassword = control.get('newPassword');
  const confirmPassword = control.get('confirmPassword');

  if (!newPassword || !confirmPassword) {
    return null;
  }

  if (newPassword.value !== confirmPassword.value) {
    confirmPassword.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  } else {
    const errors = confirmPassword.errors;
    if (errors) {
      delete errors['passwordMismatch'];
      confirmPassword.setErrors(Object.keys(errors).length ? errors : null);
    }
    return null;
  }
}

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  standalone: true,
})
export class ResetPasswordComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  fb = inject(FormBuilder);

  resetPasswordForm: FormGroup;
  loading: boolean = false;
  resetPasswordError: string = '';
  successMessage: string = '';
  loadingText: string = 'Resetting password, please wait...';
  showNewPassword: boolean = false;
  showConfirmPassword: boolean = false;
  passwordStrength: string = '';
  token: string = '';
  email: string = '';

  constructor() {
    this.resetPasswordForm = this.fb.group(
      {
        newPassword: [
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
        confirmPassword: ['', [Validators.required, Validators.maxLength(100)]],
      },
      { validators: passwordMatchValidator }
    );
  }

  ngOnInit(): void {
    // Retrieve email and token from query parameters
    this.activatedRoute.queryParams.subscribe((params) => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';
      if (!this.email || !this.token) {
        this.resetPasswordError =
          'Invalid or missing reset link. Please request a new one.';
        this.resetPasswordForm.disable();
      }
    });

    // Update password strength on newPassword changes
    this.resetPasswordForm
      .get('newPassword')
      ?.valueChanges.subscribe((value) => {
        this.updatePasswordStrength(value);
        this.resetPasswordError = '';
      });

    // Clear error on form changes
    this.resetPasswordForm.valueChanges.subscribe(() => {
      if (this.resetPasswordError && this.resetPasswordForm.valid) {
        this.resetPasswordError = '';
      }
    });
  }

  onResetPassword(): void {
    this.resetPasswordError = '';
    this.successMessage = '';

    if (this.resetPasswordForm.invalid || !this.email || !this.token) {
      this.resetPasswordForm.markAllAsTouched();
      this.setValidationError();
      return;
    }

    this.loading = true;
    this.loadingText = 'Resetting password, please wait...';

    const resetData: ResetPasswordRequest = {
      email: this.email,
      token: this.token,
      newPassword: this.resetPasswordForm.value.newPassword,
      confirmPassword: this.resetPasswordForm.value.confirmPassword,
    };

    this.authService.resetPassword(resetData).subscribe({
      next: (response) => {
        if (response.isSuccess || response.isSucceeded) {
          this.loadingText =
            response.message || 'Your password has been reset successfully!';
          setTimeout(() => {
            this.loadingText = 'Navigating to login...';
            setTimeout(() => {
              this.loading = false;
              this.router.navigate(['/auth/login']);
            }, 1000);
          }, 2000);
        } else {
          this.loading = false;
          this.resetPasswordError =
            response.message || 'Failed to reset password. Please try again.';
        }
      },
      error: (error) => {
        this.loading = false;
        if (error.status === 404) {
          this.resetPasswordError = 'Invalid email address.';
        } else if (error.status === 400) {
          this.resetPasswordError =
            error.error?.message ||
            'Invalid or expired reset link. Please request a new one.';
        } else if (error.status === 500) {
          this.resetPasswordError =
            'An error occurred while processing the request.';
        } else {
          this.resetPasswordError =
            error.error?.message ||
            'Failed to reset password. Please try again.';
        }
        console.error('Reset password error:', error);
      },
    });
  }

  togglePasswordVisibility(field: 'newPassword' | 'confirmPassword'): void {
    if (field === 'newPassword') {
      this.showNewPassword = !this.showNewPassword;
    } else {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (
      event.key === 'Enter' &&
      !this.loading &&
      this.resetPasswordForm.valid
    ) {
      this.onResetPassword();
    }
  }

  private updatePasswordStrength(password: string): void {
    if (!password) {
      this.passwordStrength = '';
      return;
    }
    const length = password.length;
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[@$!%*?&]/.test(password);

    if (length < 8 || !hasUpper || !hasLower || !hasNumber || !hasSpecial) {
      this.passwordStrength = 'weak';
    } else if (length >= 8 && length < 12) {
      this.passwordStrength = 'fair';
    } else if (length >= 12 && length < 16) {
      this.passwordStrength = 'good';
    } else {
      this.passwordStrength = 'strong';
    }
  }

  private setValidationError(): void {
    const errors = [];
    const newPasswordControl = this.resetPasswordForm.get('newPassword');
    const confirmPasswordControl =
      this.resetPasswordForm.get('confirmPassword');

    if (!this.email || !this.token) {
      errors.push('Invalid or missing reset link. Please request a new one.');
    }

    if (newPasswordControl?.errors) {
      if (newPasswordControl.errors['required'])
        errors.push('New password is required');
      if (
        newPasswordControl.errors['minlength'] ||
        newPasswordControl.errors['pattern']
      ) {
        errors.push(
          'Password must be at least 8 characters with one uppercase, one lowercase, one number, and one special character'
        );
      }
      if (newPasswordControl.errors['maxlength'])
        errors.push('Password must be less than 100 characters');
    }

    if (confirmPasswordControl?.errors) {
      if (confirmPasswordControl.errors['required'])
        errors.push('Please confirm your password');
      if (confirmPasswordControl.errors['passwordMismatch'])
        errors.push('Passwords do not match');
      if (confirmPasswordControl.errors['maxlength'])
        errors.push('Confirm password must be less than 100 characters');
    }

    if (errors.length > 0) {
      this.resetPasswordError = `Please check the following: ${errors.join(
        ', '
      )}`;
    }
  }
}
