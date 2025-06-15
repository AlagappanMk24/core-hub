// verify-otp.component.ts
import { Component, OnInit, OnDestroy, ViewChildren, QueryList, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-verify-otp',
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.css'],
  imports: [FormsModule, CommonModule]
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  @ViewChildren('otpInput') otpInputs!: QueryList<ElementRef>;

  otpDigits: string[] = ['', '', '', '', '', ''];
  timeLeft: number = 300; // 5 minutes in seconds
  isVerifying: boolean = false;
  maskedContact: string = '';
  contactType: 'email' | 'phone' = 'email';
  private timer: any;
  private maxAttempts: number = 3;
  private attemptCount: number = 0;

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Get contact info from route parameters or service
    this.route.queryParams.subscribe(params => {
      const contact = params['contact'];
      const type = params['type'] || 'email';
      
      if (contact) {
        this.contactType = type;
        this.maskedContact = this.maskContact(contact, type);
      } else {
        // Default values for demo
        this.contactType = 'email';
        this.maskedContact = 'us***@gmail.com';
      }
    });

    this.startTimer();
  }

  ngOnDestroy(): void {
    this.clearTimer();
  }

  onOtpInput(event: any, index: number): void {
    const input = event.target;
    const value = input.value;

    // Only allow numeric input
    if (!/^\d*$/.test(value)) {
      input.value = '';
      this.otpDigits[index] = '';
      return;
    }

    this.otpDigits[index] = value;

    // Auto focus next input
    if (value && index < 5) {
      const nextInput = this.otpInputs.toArray()[index + 1];
      if (nextInput) {
        nextInput.nativeElement.focus();
      }
    }

    // Auto submit when all digits are filled
    if (this.isOtpComplete() && !this.isVerifying) {
      setTimeout(() => this.onVerifyOTP(), 500);
    }

    // Update input styling
    this.updateInputStyling();
  }

  onKeyDown(event: KeyboardEvent, index: number): void {
    const input = event.target as HTMLInputElement;

    // Handle backspace
    if (event.key === 'Backspace') {
      if (!input.value && index > 0) {
        // Move to previous input if current is empty
        const prevInput = this.otpInputs.toArray()[index - 1];
        if (prevInput) {
          prevInput.nativeElement.focus();
          this.otpDigits[index - 1] = '';
        }
      } else {
        // Clear current input
        this.otpDigits[index] = '';
      }
      this.updateInputStyling();
    }

    // Handle arrow keys
    if (event.key === 'ArrowLeft' && index > 0) {
      const prevInput = this.otpInputs.toArray()[index - 1];
      if (prevInput) {
        prevInput.nativeElement.focus();
      }
    }

    if (event.key === 'ArrowRight' && index < 5) {
      const nextInput = this.otpInputs.toArray()[index + 1];
      if (nextInput) {
        nextInput.nativeElement.focus();
      }
    }

    // Prevent non-numeric input
    if (!/[\d\b\t\r\n\f]/.test(event.key) && !event.ctrlKey) {
      event.preventDefault();
    }
  }

  onPaste(event: ClipboardEvent, index: number): void {
    event.preventDefault();
    const paste = event.clipboardData?.getData('text') || '';
    const digits = paste.replace(/\D/g, '').slice(0, 6);

    if (digits.length > 0) {
      for (let i = 0; i < 6; i++) {
        this.otpDigits[i] = digits[i] || '';
      }

      // Focus the last filled input or the first empty one
      const lastFilledIndex = digits.length - 1;
      const targetIndex = Math.min(lastFilledIndex + 1, 5);
      
      setTimeout(() => {
        const targetInput = this.otpInputs.toArray()[targetIndex];
        if (targetInput) {
          targetInput.nativeElement.focus();
        }
      }, 0);

      this.updateInputStyling();

      // Auto submit if complete
      if (this.isOtpComplete()) {
        setTimeout(() => this.onVerifyOTP(), 500);
      }
    }
  }

  onVerifyOTP(): void {
    if (!this.isOtpComplete() || this.isVerifying) {
      return;
    }

    this.isVerifying = true;
    const otpCode = this.otpDigits.join('');

    console.log('Verifying OTP:', otpCode);

    // Simulate API call
    setTimeout(() => {
      this.attemptCount++;

      // For demo purposes - accept 123456 as valid OTP
      if (otpCode === '123456') {
        console.log('OTP verification successful');
        alert('OTP verified successfully!');
        this.router.navigate(['/dashboard']); // Navigate to next page
      } else {
        console.log('Invalid OTP');
        this.handleInvalidOTP();
      }

      this.isVerifying = false;
    }, 2000);
  }

  onResendOTP(event: Event): void {
    event.preventDefault();
    
    if (this.timeLeft > 0) {
      return;
    }

    console.log('Resending OTP to:', this.maskedContact);
    
    // Reset timer and attempts
    this.timeLeft = 300;
    this.attemptCount = 0;
    this.startTimer();
    
    // Clear current OTP
    this.clearOTP();
    
    // Simulate resend API call
    setTimeout(() => {
      alert(`New OTP sent to ${this.maskedContact}`);
    }, 1000);
  }

  onChangeContact(event: Event): void {
    event.preventDefault();
    console.log('Change contact clicked');
    
    // Navigate back to previous step or show contact change modal
    this.router.navigate(['/auth/forgot-password']);
  }

  isOtpComplete(): boolean {
    return this.otpDigits.every(digit => digit !== '');
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
      // Phone number masking
      if (contact.length <= 4) {
        return `***${contact.slice(-2)}`;
      }
      return `***-***-${contact.slice(-4)}`;
    }
  }

  private updateInputStyling(): void {
    setTimeout(() => {
      this.otpInputs.forEach((input, index) => {
        const element = input.nativeElement;
        element.classList.remove('filled', 'error');
        
        if (this.otpDigits[index]) {
          element.classList.add('filled');
        }
      });
    }, 0);
  }

  private handleInvalidOTP(): void {
    // Add error styling
    this.otpInputs.forEach(input => {
      input.nativeElement.classList.add('error');
    });

    // Remove error styling after 3 seconds
    setTimeout(() => {
      this.otpInputs.forEach(input => {
        input.nativeElement.classList.remove('error');
      });
    }, 3000);

    if (this.attemptCount >= this.maxAttempts) {
      alert('Maximum attempts exceeded. Please request a new OTP.');
      this.clearOTP();
      this.timeLeft = 0;
      this.clearTimer();
    } else {
      alert(`Invalid OTP. ${this.maxAttempts - this.attemptCount} attempts remaining.`);
      this.clearOTP();
      
      // Focus first input
      setTimeout(() => {
        const firstInput = this.otpInputs.toArray()[0];
        if (firstInput) {
          firstInput.nativeElement.focus();
        }
      }, 100);
    }
  }

  private clearOTP(): void {
    this.otpDigits = ['', '', '', '', '', ''];
    this.updateInputStyling();
  }

  // Additional utility methods
  getProgressPercentage(): number {
    const filledCount = this.otpDigits.filter(digit => digit !== '').length;
    return (filledCount / 6) * 100;
  }

  isTimeExpired(): boolean {
    return this.timeLeft <= 0;
  }

  getRemainingAttempts(): number {
    return Math.max(0, this.maxAttempts - this.attemptCount);
  }

  // Method to handle programmatic OTP verification (useful for testing)
  verifyProgrammatically(otp: string): void {
    if (otp.length === 6 && /^\d+$/.test(otp)) {
      this.otpDigits = otp.split('');
      this.updateInputStyling();
      this.onVerifyOTP();
    }
  }
}