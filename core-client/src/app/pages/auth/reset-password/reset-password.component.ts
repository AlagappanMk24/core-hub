import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    MatInputModule,
    MatIconModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink,
    CommonModule,
  ],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
})
export class ResetPasswordComponent {
  form: FormGroup;
  hidePassword = true;
  hideConfirmPassword = true;
  isSubmitting = false;
  passwordResetSuccess = false;
  token: string | null = null;
  email: string | null = null;
  loading = false;
  router = inject(Router);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required, Validators.minLength(6)]],
    });

    // Apply validator separately
    this.form.setValidators(this.passwordMatchValidator);

    // Get email and token from query params
    this.route.queryParams.subscribe((params) => {
      this.email = params['email'] || null;
      this.token = params['token'] || null;
      console.log(
        'Extracted Email:',
        this.email,
        'Extracted Token:',
        this.token
      );
    });
  }

  // Password Validator
  passwordMatchValidator: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors | null => {
    console.log('ggg');
    const password = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmNewPassword')?.value;
    return password && confirmPassword && password !== confirmPassword
      ? { passwordMismatch: true }
      : null;
  };

  // Toggle Password Visibility
  toggleVisibility(field: 'password' | 'confirmPassword') {
    if (field === 'password') {
      this.hidePassword = !this.hidePassword;
    } else {
      this.hideConfirmPassword = !this.hideConfirmPassword;
    }
  }

  // Submit Form
  submit() {
    if (this.form.valid && this.email) {
      this.isSubmitting = true;
      this.loading = true;
      const token = decodeURIComponent(
        this.route.snapshot.queryParamMap.get('token') || ''
      );
      const requestData = {
        email: this.email,
        token: token,
        newPassword: this.form.value.newPassword,
        confirmPassword: this.form.value.confirmNewPassword,
      };
      this.authService.resetPassword(requestData).subscribe({
        next: (response) => {
          if (response.isSucceeded) {
            this.loading = false;
            this.snackBar.open('Password reset successful!', 'Close', {
              duration: 3000,
              panelClass: ['bg-green-500', 'text-white'],
            });
            this.passwordResetSuccess = true;
          } else {
            this.snackBar.open('Failed to reset password.', 'Close', {
              duration: 3000,
              panelClass: ['bg-green-500', 'text-white'],
            });
          }
        },
        error: (err: HttpErrorResponse) => {
          this.snackBar.open('Failed to reset password!', 'Close', {
            duration: 3000,
            panelClass: ['bg-red-500', 'text-white'],
          });
          this.loading = false;
        },
        complete: () => {
          this.isSubmitting = false;
        },
      });
    }
  }
}
