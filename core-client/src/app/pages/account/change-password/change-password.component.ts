// change-password.component.ts
import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { CommonModule } from '@angular/common';
import { ChangePasswordRequest } from '../../../interfaces/auth/auth-request/changepassword-request';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css'],
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  standalone: true,
})
export class ChangePasswordComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);
  fb = inject(FormBuilder);

  changePasswordForm: FormGroup;
  loading: boolean = false;
  changePasswordError: string = '';
  changePasswordSuccess: string = '';
  loadingText: string = 'Changing password, please wait...';
  showCurrentPassword: boolean = false;
  showNewPassword: boolean = false;
  showConfirmPassword: boolean = false;

  constructor() {
    this.changePasswordForm = this.fb.group({
      currentPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(100),
        ],
      ],
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
      confirmPassword: [
        '',
        [Validators.required],
      ],
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Reset error and success messages when user starts typing
    this.changePasswordForm.valueChanges.subscribe(() => {
      this.changePasswordError = '';
      this.changePasswordSuccess = '';
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    if (confirmPassword?.errors?.['passwordMismatch']) {
      delete confirmPassword.errors['passwordMismatch'];
      if (Object.keys(confirmPassword.errors).length === 0) {
        confirmPassword.setErrors(null);
      }
    }
    
    return null;
  }

  onChangePassword(): void {
    this.changePasswordError = '';
    this.changePasswordSuccess = '';

    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      this.setValidationError();
      return;
    }

    this.loading = true;
    this.loadingText = 'Changing password, please wait...';

    const changePasswordData: ChangePasswordRequest = {
      currentPassword: this.changePasswordForm.value.currentPassword,
      newPassword: this.changePasswordForm.value.newPassword,
      confirmPassword: this.changePasswordForm.value.confirmPassword,
    };

    setTimeout(() => {
      this.authService.changePassword(changePasswordData).subscribe({
        next: (response) => {
          if (response.isSucceeded) {
            this.loadingText = 'Password changed successfully!';
            this.changePasswordSuccess = response.message || 'Password changed successfully!';
            setTimeout(() => {
              this.loading = false;
              this.changePasswordForm.reset();
              // Optionally redirect to profile or dashboard
              // this.router.navigate(['/dashboard']);
            }, 2000);
          } else {
            this.loading = false;
            this.changePasswordError = response.message || 'Failed to change password. Please try again.';
          }
        },
        error: (error) => {
          this.loading = false;
          if (error.status === 400) {
            this.changePasswordError = error.error?.message || 'Invalid current password or new password requirements not met.';
          } else if (error.status === 401) {
            this.changePasswordError = 'Current password is incorrect.';
          } else if (error.status === 403) {
            this.changePasswordError = error.error?.message || 'Unauthorized access.';
          } else if (error.status === 500) {
            this.changePasswordError = 'An error occurred while processing the request.';
          } else {
            this.changePasswordError = error.error?.message || 'Failed to change password. Please try again.';
          }
          console.error('Change password error:', error);
        },
      });
    }, 2000);
  }

  togglePasswordVisibility(field: string): void {
    switch (field) {
      case 'current':
        this.showCurrentPassword = !this.showCurrentPassword;
        break;
      case 'new':
        this.showNewPassword = !this.showNewPassword;
        break;
      case 'confirm':
        this.showConfirmPassword = !this.showConfirmPassword;
        break;
    }
  }

  private setValidationError(): void {
    const errors = [];
    const currentPasswordControl = this.changePasswordForm.get('currentPassword');
    const newPasswordControl = this.changePasswordForm.get('newPassword');
    const confirmPasswordControl = this.changePasswordForm.get('confirmPassword');

    if (currentPasswordControl?.errors) {
      if (currentPasswordControl.errors['required']) errors.push('Current password is required');
      if (currentPasswordControl.errors['minlength']) 
        errors.push('Current password must be at least 8 characters');
      if (currentPasswordControl.errors['maxlength'])
        errors.push('Current password must be less than 100 characters');
    }

    if (newPasswordControl?.errors) {
      if (newPasswordControl.errors['required']) errors.push('New password is required');
      if (newPasswordControl.errors['minlength'] || newPasswordControl.errors['pattern']) {
        errors.push('New password must be at least 8 characters with one uppercase, one lowercase, one number, and one special character');
      }
      if (newPasswordControl.errors['maxlength'])
        errors.push('New password must be less than 100 characters');
    }

    if (confirmPasswordControl?.errors) {
      if (confirmPasswordControl.errors['required']) errors.push('Confirm password is required');
      if (confirmPasswordControl.errors['passwordMismatch']) errors.push('Passwords do not match');
    }

    if (errors.length > 0) {
      this.changePasswordError = `Please check the following: ${errors.join(', ')}`;
    }
  }
}