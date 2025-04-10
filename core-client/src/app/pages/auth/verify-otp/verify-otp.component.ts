import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

@Component({
  selector: 'app-verify-otp',
  imports: [
    MatInputModule,
    MatIconModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink,
    CommonModule,
  ],
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.css'],
})
export class VerifyOtpComponent implements OnInit {
  authService = inject(AuthService);
  route = inject(ActivatedRoute);
  router = inject(Router);
  fb = inject(FormBuilder);
  matSnackBar = inject(MatSnackBar);

  otpForm!: FormGroup;
  loading: boolean = false;
  email: string = '';
  resending: boolean = false;
  otpResentMessage: string = '';
  otpValidationMessage: string = '';

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') || '';

    this.otpForm = this.fb.group({
      otp: [
        '',
        [Validators.required, Validators.minLength(6), Validators.maxLength(6)],
      ],
    });
    this.otpForm.get('otp')?.valueChanges.subscribe(() => {
      this.otpValidationMessage = ''; // Clear error message when user types
    });
  }

  verifyOtp() {
    this.loading = true;
    this.otpValidationMessage = ''; // Clear previous message
    this.authService
      .verifyOtp({ email: this.email, otp: this.otpForm.value.otp })
      .subscribe({
        next: (response) => {
          this.loading = false;
          // Store the token
          localStorage.setItem('authToken', response.token);
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.loading = false;
          console.error('OTP Verification Error:', error);
          // Show error message dynamically from API response
          this.otpValidationMessage =
            error.error?.message || 'Invalid OTP, please try again';
        },
      });
  }

  resendOtp() {
    this.resending = true;
    this.otpResentMessage = ''; // Clear previous message
    this.authService.resendOtp({ email: this.email }).subscribe({
      next: (response) => {
        this.resending = false;
        this.otpResentMessage = response.message;
        // Hide the message after 5 seconds
        setTimeout(() => {
          this.otpResentMessage = '';
        }, 5000);
      },
      error: (error) => {
        this.resending = false;
        this.matSnackBar.open(
          error.error?.message || 'Failed to resend OTP, please try again',
          'Close',
          { duration: 5000 }
        );
      },
    });
  }
}
