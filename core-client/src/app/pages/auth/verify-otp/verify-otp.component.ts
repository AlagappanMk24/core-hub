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
import { AuthService } from '../../../services/auth/auth.service';

/**
 * Component for handling OTP verification during two-factor authentication.
 */
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

  otpDigits: string[] = ['', '', '', '', '', ''];
  timeLeft: number = 120; // 2 minutes in seconds
  isVerifying: boolean = false;
  errorMessage: string = '';
  loadingText: string = 'Verifying OTP, please wait...';
  maskedContact: string = '';
  contactType: 'email' | 'phone' = 'email';
  otpIdentifier: string = '';
  private timer: any;
  private maxAttempts: number = 3;
  private attemptCount: number = 0;

  /**
   * Initializes query params and starts OTP timer.
   */
  ngOnInit(): void {
    // Retrieve OTP identifier from query params
    this.route.queryParams.subscribe((params) => {
      this.otpIdentifier = params['otpIdentifier'] || '';
      this.contactType = params['type'] || 'email';
      if (this.otpIdentifier) {
        // this.maskedContact = this.maskContact(
        //   this.otpIdentifier,
        //   this.contactType
        // );
        this.maskedContact = 'your registered email'; // Generic message since email is not exposed
      } else {
        this.errorMessage =
          'No contact information provided. Please try again.';
        this.router.navigate(['/auth/login']);
      }
    });
    this.startTimer();
  }

  /**
   * Cleans up timer on component destruction.
   */
  ngOnDestroy(): void {
    this.clearTimer();
  }

  /**
   * Submits OTP for verification.
   */
  onVerifyOTP(): void {
    if (!this.isOtpComplete() || this.isVerifying) {
      return;
    }

    this.isVerifying = true;
    this.errorMessage = '';
    this.loadingText = 'Verifying OTP, please wait...';
    const otpCode = this.otpDigits.join('');
    const otpToken = this.authService.getOtpToken() || '';

    // Verify OTP
    this.authService
      .verifyOtp({ otpIdentifier: this.otpIdentifier, otp: otpCode, otpToken })
      .subscribe({
        next: (response) => {
          if (response.isAuthenticated && response.token) {
            localStorage.setItem('authToken', response.token);
            this.loadingText =
              'OTP validated Succesfully, navigating to dashboard...';
            setTimeout(() => {
              this.isVerifying = false;
               // Redirect to customer-dashboard if user has Customer role
              const user = this.authService.getUserDetail();
              if (user?.roles.includes('Customer')) {
                this.router.navigate(['/customer-dashboard']);
              } else {
                this.router.navigate(['/dashboard']);
              }
            }, 2000);
          } else {
            this.isVerifying = false;
            this.attemptCount++;
            this.handleInvalidOTP(response.message || 'Invalid OTP.');
          }
        },
        error: (error) => {
          this.isVerifying = false;
          this.attemptCount++;
          const message =
            error.status === 429
              ? `${
                  error.error?.message ||
                  'Too many OTP verification attempts. Please wait and try again.'
                } Try again in ${error.error?.retryAfterSeconds || 60} seconds.`
              : error.error?.message ||
                'An error occurred during OTP verification.';
          this.handleInvalidOTP(message);
        },
      });
  }

  onResendOTP(event: Event): void {
    event.preventDefault();
    if (this.timeLeft > 0 || this.isVerifying) {
      return;
    }

    this.isVerifying = true;
    this.errorMessage = '';
    this.loadingText = 'Resending OTP , please wait...';

    this.authService
      .resendOtp({ otpIdentifier: this.otpIdentifier })
      .subscribe({
        next: (response) => {
          if (response.isSucceeded) {
            this.timeLeft = 300;
            this.attemptCount = 0;
            this.startTimer();
            this.clearOTP();
            this.loadingText = response.message;
            setTimeout(() => {
              this.isVerifying = false;
              this.cdr.detectChanges();
            }, 1000);
          } else {
            this.isVerifying = false;
            this.errorMessage =
              response.message || 'Failed to resend OTP. Please try again.';
          }
        },
        error: (error) => {
          this.isVerifying = false;
          this.errorMessage =
            error.status === 429
              ? `${
                  error.error?.message ||
                  'Too many OTP resend requests. Please wait and try again.'
                } Try again in ${
                  error.error?.retryAfterSeconds || 300
                } seconds.`
              : error.error?.message ||
                'An error occurred while resending OTP.';
        },
      });
  }

  onChangeContact(event: Event): void {
    event.preventDefault();
    // this.router.navigate(['/auth/forgot-password'], {
    //   queryParams: { email: this.email },
    // });
    this.router.navigate(['/auth/forgot-password']);
  }

  onKeydown(event: KeyboardEvent, index: number): void {
    const input = event.target as HTMLInputElement;
    const value = event.key;

    // Allow only digits, Backspace, Enter
    if (!/^[0-9]$/.test(value) && value !== 'Backspace' && value !== 'Enter') {
      event.preventDefault();
      return;
    }

    // Handle digit input
    if (/^[0-9]$/.test(value)) {
      event.preventDefault();
      this.otpDigits[index] = value;

      // Clear all future inputs
      for (let i = index + 1; i < 6; i++) {
        this.otpDigits[i] = '';
        const nextInput = this.otpInputs.toArray()[i]?.nativeElement;
        if (nextInput) {
          nextInput.value = '';
        }
      }

      // Move to next input
      if (index < 5) {
        setTimeout(() => this.focusInput(index + 1), 500);
      }
    }

    // Handle Backspace
    if (value === 'Backspace' && !this.otpDigits[index] && index > 0) {
      event.preventDefault();
      this.otpDigits[index - 1] = '';
      setTimeout(() => this.focusInput(index - 1), 500);
    }

    // Handle Enter
    if (value === 'Enter' && this.isOtpComplete()) {
      event.preventDefault();
      this.onVerifyOTP();
    }

    this.updateInputStyling();
    this.cdr.detectChanges();
  }

  onInput(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.trim();

    // Clean up invalid input
    if (value && !/^[0-9]$/.test(value)) {
      this.otpDigits[index] = '';
      input.value = '';
      this.cdr.detectChanges();
      return;
    }
  }

  onPaste(event: ClipboardEvent, index: number): void {
    event.preventDefault();
    const pastedData = event.clipboardData?.getData('text').trim();
    if (pastedData) {
      // Reset OTP digits
      this.otpDigits = ['', '', '', '', '', ''];

      // Fill digits, stopping at non-digits
      let filledCount = 0;
      for (let i = 0; i < Math.min(pastedData.length, 6); i++) {
        if (/^[0-9]$/.test(pastedData[i])) {
          this.otpDigits[i] = pastedData[i];
          filledCount++;
        } else {
          break;
        }
      }

      // Update DOM
      this.cdr.detectChanges();

      // Focus the last filled input or first empty one
      const focusIndex = filledCount === 6 ? 5 : filledCount;
      setTimeout(() => this.focusInput(focusIndex), 500);

      this.updateInputStyling();
      // console.log('onPaste:', { pastedData, otpDigits: [...this.otpDigits], focusIndex, inputValues: this.otpInputs.map(inp => inp.nativeElement.value) });
    }
  }

  isOtpComplete(): boolean {
    return this.otpDigits.every((digit) => digit && /^[0-9]$/.test(digit));
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  private startTimer(): void {
    this.clearTimer();
    this.timer = setInterval(() => {
      this.timeLeft--;
      if (this.timeLeft <= 0) {
        this.clearTimer();
        this.cdr.detectChanges();
      }
    }, 1000);
  }

  private clearTimer(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  private maskContact(contact: string, type: 'email' | 'phone'): string {
    if (type === 'email') {
      const [username, domain] = contact.split('@');
      if (username.length <= 2) {
        return `${username[0]}***@${domain}`;
      }
      return `${username.slice(0, 2)}***@${domain}`;
    } else {
      if (contact.length <= 4) {
        return `***${contact.slice(-2)}`;
      }
      return `***-***-${contact.slice(-4)}`;
    }
    //  return 'your registered email';
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

  private handleInvalidOTP(message: string): void {
    this.errorMessage = message;
    this.updateInputStyling();
    if (this.attemptCount >= this.maxAttempts) {
      this.errorMessage =
        'Maximum attempts exceeded. Please request a new OTP.';
      this.clearOTP();
      this.timeLeft = 0;
      this.clearTimer();
    } else {
      this.clearOTP();
      setTimeout(() => this.focusInput(0), 500);
    }
    this.cdr.detectChanges();
  }

  private clearOTP(): void {
    this.otpDigits = ['', '', '', '', '', ''];
    this.updateInputStyling();
    this.cdr.detectChanges();
  }

  private focusInput(index: number): void {
    const input = this.otpInputs.toArray()[index]?.nativeElement;
    if (input) {
      input.focus();
    }
  }
}