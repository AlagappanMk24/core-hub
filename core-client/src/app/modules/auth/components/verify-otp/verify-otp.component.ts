import {
  Component,
  OnInit,
  OnDestroy,
  ViewChildren,
  QueryList,
  ElementRef,
  inject,
  ChangeDetectorRef,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  statusCode: number;
  data: T;
  timestamp?: string;
  traceId?: string | null;
}

@Component({
  selector: 'app-verify-otp',
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.css'],
  imports: [FormsModule, CommonModule, RouterModule],
  standalone: true,
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  @ViewChildren('otpInput') otpInputs!: QueryList<ElementRef>;

  authService = inject(AuthService);
  router = inject(Router);
  route = inject(ActivatedRoute);
  cdr = inject(ChangeDetectorRef);

  // OTP Input Fields
  otpDigits: string[] = ['', '', '', '', '', ''];

  // Timer Configuration
  timeLeft: number = 120;
  resendTimer: number = 0;
  private timer: any;
  private resendTimerInterval: any;

  // UI State
  isVerifying: boolean = false;
  errorMessage: string = '';
  loadingText: string = 'Verifying OTP, please wait...';
  maskedContact: string = '';
  otpIdentifier: string = '';

  // Success Animation State
  isSuccess: boolean = false;
  successMessage: string = '';
  private redirectTimeout: any;

  // Security Tracking
  remainingAttempts: number = 5;
  maxAttempts: number = 5;
  isLocked: boolean = false;
  lockoutDisplay: string = '';
  lockoutSecondsRemaining: number = 0;
  private lockoutTimer: any;

  // Progressive Delay
  isDelayed: boolean = false;
  delayRemaining: number = 0;
  private delayTimer: any;

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.otpIdentifier = params['otpIdentifier'] || '';

      if (this.otpIdentifier) {
        this.maskedContact = 'your registered email';
      } else {
        this.errorMessage =
          'No verification identifier found. Please try again.';
        this.router.navigate(['/auth/login']);
      }
    });
    this.startTimer();
  }

  ngOnDestroy(): void {
    this.clearTimer();
    this.clearResendTimer();
    this.clearDelayTimer();
    this.clearLockoutTimer();
  }

  onVerifyOTP(): void {
    if (
      !this.isOtpComplete() ||
      this.isVerifying ||
      this.isDelayed ||
      this.isLocked
    ) {
      return;
    }

    this.isVerifying = true;
    this.errorMessage = '';

    const otpCode = this.otpDigits.join('');
    const otpToken = this.authService.getOtpToken() || '';

    this.authService
      .verifyOtp({
        otpIdentifier: this.otpIdentifier,
        otp: otpCode,
        otpToken,
      })
      .subscribe({
        next: (response: ApiResponse) => {
          console.log('OTP Verify Response:', response);

          this.isVerifying = false;

          if (response.success && response.data) {
            // Success - 200 OK
            localStorage.setItem('authToken', response.data.token);
            this.showSuccessAndRedirect();
          } else {
            // Handle failure based on status code
            this.handleApiFailure(response);
          }
        },
        error: (error: HttpErrorResponse) => {
          console.error('OTP verification error:', error);
          this.isVerifying = false;

          // Handle HTTP error responses
          if (error.error) {
            this.handleApiFailure(error.error);
          } else {
            this.showError('An error occurred during OTP verification.');
          }
        },
      });
  }

  private handleApiFailure(response: ApiResponse): void {
    const statusCode = response.statusCode;
    const message = response.message;
    const data = response.data;
    console.log(`OTP verification failed with status ${statusCode}:`, response);
    console.log(`OTP verification Data:`, response.data);
    switch (statusCode) {
      case 403:
        // Wrong OTP (1-4 attempts) or OTP Expired
        if (data?.remainingAttempts !== undefined) {
          this.remainingAttempts = data.remainingAttempts;
          this.showError(
            message ||
              `Invalid code. ${this.remainingAttempts} attempt(s) remaining.`,
          );
          this.clearOTP();
          setTimeout(() => this.focusInput(0), 500);
        } else if (data?.progressiveDelaySeconds) {
          // Progressive delay case
          this.remainingAttempts =
            data.remainingAttempts || this.remainingAttempts;
          this.showError(
            message ||
              `Invalid code. Please wait ${data.progressiveDelaySeconds} seconds.`,
          );
          this.applyProgressiveDelay(data.progressiveDelaySeconds);
        } else {
          this.showError(message || 'Invalid verification code.');
          this.clearOTP();
          setTimeout(() => this.focusInput(0), 500);
        }
        break;

      case 429:
        // Lockout or too many attempts
        if (data?.isLocked || data?.lockoutSecondsRemaining) {
          this.isLocked = true;
          this.lockoutSecondsRemaining = data.lockoutSecondsRemaining || 900; // 15 minutes default
          this.startLockoutCountdown();
          this.updateLockoutDisplay();
          this.showError(
            message || 'Too many failed attempts. Please try again later.',
          );
        } else {
          this.showError(
            message || 'Too many requests. Please wait before trying again.',
          );
        }
        break;

      default:
        this.showError(message || 'Verification failed. Please try again.');
        this.clearOTP();
        setTimeout(() => this.focusInput(0), 500);
        break;
    }
  }

  onResendOTP(event: Event): void {
    event.preventDefault();

    if (this.resendTimer > 0 || this.isLocked || this.isVerifying) {
      if (this.resendTimer > 0) {
        this.showError(
          `Please wait ${this.resendTimer} seconds before requesting another OTP.`,
        );
      }
      return;
    }

    this.isVerifying = true;
    this.errorMessage = '';
    this.loadingText = 'Resending OTP, please wait...';

    this.authService
      .resendOtp({ otpIdentifier: this.otpIdentifier })
      .subscribe({
        next: (response: ApiResponse) => {
          this.isVerifying = false;

          if (response.success) {
            // Reset all state
            this.timeLeft = 120;
            this.remainingAttempts = 5;
            this.isLocked = false;
            this.isDelayed = false;
            this.lockoutSecondsRemaining = 0;
            this.clearLockoutTimer();
            this.startTimer();
            this.startResendCooldown();
            this.clearOTP();
            this.showSuccess(response.message || 'OTP sent successfully!');

            setTimeout(() => {
              this.errorMessage = '';
            }, 3000);
          } else {
            this.showError(
              response.message || 'Failed to resend OTP. Please try again.',
            );
          }
        },
        error: (error: HttpErrorResponse) => {
          this.isVerifying = false;

          if (error.status === 429 && error.error) {
            const retryAfter = error.error.data?.retryAfterSeconds || 60;
            this.startResendCooldown(retryAfter);
            this.showError(
              error.error.message ||
                `Too many resend requests. Please wait ${retryAfter} seconds.`,
            );
          } else if (error.error) {
            this.showError(
              error.error.message || 'An error occurred while resending OTP.',
            );
          } else {
            this.showError('An error occurred while resending OTP.');
          }
        },
      });
  }

  private applyProgressiveDelay(seconds: number): void {
    this.isDelayed = true;
    this.delayRemaining = seconds;
    this.delayTimer = setInterval(() => {
      this.delayRemaining--;
      if (this.delayRemaining <= 0) {
        this.clearDelayTimer();
        this.isDelayed = false;
        this.delayRemaining = 0;
        this.enableOtpInputs();
        this.cdr.detectChanges();
      }
      this.cdr.detectChanges();
    }, 1000);
  }

  private startLockoutCountdown(): void {
    this.clearLockoutTimer();
    this.lockoutTimer = setInterval(() => {
      if (this.lockoutSecondsRemaining > 0) {
        this.lockoutSecondsRemaining--;
        this.updateLockoutDisplay();
        this.cdr.detectChanges();
      } else {
        this.clearLockoutTimer();
        this.isLocked = false;
        this.remainingAttempts = this.maxAttempts;
        this.errorMessage = '';
        this.enableOtpInputs();
        this.cdr.detectChanges();
      }
    }, 1000);
  }

  private updateLockoutDisplay(): void {
    const minutes = Math.floor(this.lockoutSecondsRemaining / 60);
    const seconds = this.lockoutSecondsRemaining % 60;

    if (minutes > 0 && seconds > 0) {
      this.lockoutDisplay = `${minutes} minute${minutes !== 1 ? 's' : ''} and ${seconds} second${seconds !== 1 ? 's' : ''}`;
    } else if (minutes > 0) {
      this.lockoutDisplay = `${minutes} minute${minutes !== 1 ? 's' : ''}`;
    } else {
      this.lockoutDisplay = `${seconds} second${seconds !== 1 ? 's' : ''}`;
    }
  }

  private startResendCooldown(seconds: number = 60): void {
    this.resendTimer = seconds;
    this.resendTimerInterval = setInterval(() => {
      this.resendTimer--;
      if (this.resendTimer <= 0) {
        this.clearResendTimer();
      }
      this.cdr.detectChanges();
    }, 1000);
  }

  private showSuccess(message: string): void {
    this.errorMessage = '';
    // You can add a success toast/notification here
    console.log('Success:', message);
  }

  private showSuccessAndRedirect(): void {
    // Set success state
    this.isSuccess = true;
    this.successMessage = 'Access Granted! Redirecting to Dashboard...';

    // Trigger confetti animation
    this.triggerConfetti();

    // Clear OTP inputs and show success overlay
    this.clearOTP();

    // Get user role for personalized message
    const user = this.authService.getUserDetail();
    const role = user?.roles?.[0] || 'User';

    // Update loading text based on role
    setTimeout(() => {
      this.successMessage = `Welcome back, ${user?.fullName || user?.email || 'User'}!`;
    }, 1000);

    setTimeout(() => {
      this.successMessage = `Redirecting you to ${role === 'Customer' ? 'your portal' : 'the dashboard'}...`;
    }, 2000);

    // Redirect after 3 seconds
    this.redirectTimeout = setTimeout(() => {
      this.router.navigate(['/redirect-after-login']);
    }, 3000);
  }

  /**
   * Triggers confetti animation on successful verification
   */
  private triggerConfetti(): void {
    if (typeof window !== 'undefined') {
      // Check if canvas is supported by trying to create and get context
      const canvas = document.createElement('canvas');
      if (canvas.getContext && canvas.getContext('2d')) {
        this.startConfetti();
      } else {
        // Fallback: Create simple particles without canvas
        this.createSimpleParticles();
      }
    }
  } /**
   * Creates simple falling particles as confetti alternative
   */
  private createSimpleParticles(): void {
    const colors = [
      '#c084fc',
      '#a855f7',
      '#9333ea',
      '#7e22ce',
      '#ec4899',
      '#10b981',
    ];
    const particleCount = 100;

    for (let i = 0; i < particleCount; i++) {
      const particle = document.createElement('div');
      particle.className = 'success-particle';
      particle.style.position = 'fixed';
      particle.style.left = Math.random() * window.innerWidth + 'px';
      particle.style.top = '-20px';
      particle.style.width = Math.random() * 10 + 5 + 'px';
      particle.style.height = Math.random() * 10 + 5 + 'px';
      particle.style.backgroundColor =
        colors[Math.floor(Math.random() * colors.length)];
      particle.style.borderRadius = '50%';
      particle.style.pointerEvents = 'none';
      particle.style.zIndex = '9999';
      particle.style.opacity = '0.8';
      particle.style.animation = `fall ${Math.random() * 2 + 1}s linear forwards`;

      document.body.appendChild(particle);

      setTimeout(() => particle.remove(), 3000);
    }
  }

  private startConfetti(): void {
    // Simple confetti effect using JavaScript
    const duration = 3000;
    const animationEnd = Date.now() + duration;

    const interval: any = setInterval(() => {
      const timeLeft = animationEnd - Date.now();

      if (timeLeft <= 0) {
        clearInterval(interval);
        return;
      }

      const particleCount = 30 * (timeLeft / duration);

      // Create floating particles
      for (let i = 0; i < particleCount; i++) {
        const particle = document.createElement('div');
        particle.className = 'success-particle';
        particle.style.left = Math.random() * window.innerWidth + 'px';
        particle.style.top = '-20px';
        particle.style.backgroundColor = `hsl(${Math.random() * 360}, 100%, 50%)`;
        particle.style.position = 'fixed';
        particle.style.width = Math.random() * 8 + 4 + 'px';
        particle.style.height = Math.random() * 8 + 4 + 'px';
        particle.style.borderRadius = '50%';
        particle.style.pointerEvents = 'none';
        particle.style.zIndex = '9999';
        particle.style.animation = `fall ${Math.random() * 2 + 1}s linear forwards`;

        document.body.appendChild(particle);

        setTimeout(() => particle.remove(), 3000);
      }
    }, 200);
  }

  private showError(message: string): void {
    this.errorMessage = message;
    this.updateInputStyling();
    this.cdr.detectChanges();
  }

  private enableOtpInputs(): void {
    this.otpInputs.forEach((input) => {
      input.nativeElement.disabled = false;
    });
  }

  isOtpComplete(): boolean {
    return this.otpDigits.every((digit) => digit && /^[0-9]$/.test(digit));
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  onKeydown(event: KeyboardEvent, index: number): void {
    if (this.isDelayed || this.isLocked) {
      event.preventDefault();
      return;
    }

    const value = event.key;

    if (
      !/^[0-9]$/.test(value) &&
      value !== 'Backspace' &&
      value !== 'Enter' &&
      value !== 'ArrowLeft' &&
      value !== 'ArrowRight'
    ) {
      event.preventDefault();
      return;
    }

    if (/^[0-9]$/.test(value)) {
      event.preventDefault();
      this.otpDigits[index] = value;

      for (let i = index + 1; i < 6; i++) {
        this.otpDigits[i] = '';
      }

      if (index < 5) {
        setTimeout(() => this.focusInput(index + 1), 50);
      } else {
        // Auto-submit when last digit entered
        if (this.isOtpComplete()) {
          setTimeout(() => this.onVerifyOTP(), 100);
        }
      }
    }

    if (value === 'Backspace' && !this.otpDigits[index] && index > 0) {
      event.preventDefault();
      this.otpDigits[index - 1] = '';
      setTimeout(() => this.focusInput(index - 1), 50);
    }

    if (value === 'Enter' && this.isOtpComplete()) {
      event.preventDefault();
      this.onVerifyOTP();
    }

    if (value === 'ArrowLeft' && index > 0) {
      event.preventDefault();
      this.focusInput(index - 1);
    }

    if (value === 'ArrowRight' && index < 5) {
      event.preventDefault();
      this.focusInput(index + 1);
    }

    this.updateInputStyling();
    this.cdr.detectChanges();
  }

  onInput(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.trim();

    if (value && !/^[0-9]$/.test(value)) {
      this.otpDigits[index] = '';
      input.value = '';
      this.cdr.detectChanges();
    }
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pastedData = event.clipboardData?.getData('text').trim();

    if (pastedData && /^\d+$/.test(pastedData)) {
      const digits = pastedData.slice(0, 6).split('');

      for (let i = 0; i < 6; i++) {
        this.otpDigits[i] = digits[i] || '';
      }

      this.cdr.detectChanges();

      if (this.isOtpComplete()) {
        setTimeout(() => this.onVerifyOTP(), 100);
      } else {
        const nextEmptyIndex = this.otpDigits.findIndex((d) => !d);
        if (nextEmptyIndex !== -1) {
          this.focusInput(nextEmptyIndex);
        }
      }
      this.updateInputStyling();
    }
  }

  onChangeContact(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/auth/forgot-password']);
  }

  private updateInputStyling(): void {
    this.otpInputs.forEach((input, index) => {
      const element = input.nativeElement;
      element.classList.remove('filled', 'error');
      if (this.otpDigits[index]) {
        element.classList.add('filled');
      }
      if (this.errorMessage && this.otpDigits[index]) {
        element.classList.add('error');
      }
    });
  }

  private clearOTP(): void {
    this.otpDigits = ['', '', '', '', '', ''];
    this.updateInputStyling();
    this.cdr.detectChanges();
  }

  private focusInput(index: number): void {
    const input = this.otpInputs.toArray()[index]?.nativeElement;
    if (input && !this.isDelayed && !this.isLocked) {
      input.focus();
    }
  }

  private startTimer(): void {
    this.clearTimer();
    this.timer = setInterval(() => {
      if (this.timeLeft > 0) {
        this.timeLeft--;
        this.cdr.detectChanges();
      } else {
        this.clearTimer();
        if (!this.isLocked) {
          this.showError('OTP has expired. Please request a new code.');
        }
      }
    }, 1000);
  }

  private clearTimer(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  private clearResendTimer(): void {
    if (this.resendTimerInterval) {
      clearInterval(this.resendTimerInterval);
      this.resendTimerInterval = null;
    }
  }

  private clearDelayTimer(): void {
    if (this.delayTimer) {
      clearInterval(this.delayTimer);
      this.delayTimer = null;
    }
  }

  private clearLockoutTimer(): void {
    if (this.lockoutTimer) {
      clearInterval(this.lockoutTimer);
      this.lockoutTimer = null;
    }
  }
}
