// import { CommonModule } from '@angular/common';
// import { Component, inject, OnInit } from '@angular/core';
// import { FormsModule } from '@angular/forms';
// import { Router } from '@angular/router';
// import { AuthService } from '../../../services/auth.service';
// import { RegisterRequest } from '../../../interfaces/auth/auth-request/register-request';

// @Component({
//   selector: 'app-register',
//   templateUrl: './register.component.html',
//   styleUrls: ['./register.component.css'],
//   imports: [FormsModule, CommonModule],
//   standalone: true
// })
// export class RegisterComponent implements OnInit {
//   authService = inject(AuthService);
//   router = inject(Router);

//   // Form fields
//   fullName: string = '';
//   email: string = '';
//   phoneNumber: string = '';
//   password: string = '';
//   confirmPassword: string = '';
//   agreeToTerms: boolean = false;

//   // UI state
//   showPassword: boolean = false;
//   showConfirmPassword: boolean = false;
//   isLoading: boolean = false;
//   errorMessage: string = '';

//   ngOnInit(): void {}

//   onRegister(): void {
//     // Clear previous error
//     this.errorMessage = '';

//     // Validate form
//     if (!this.isFormValid()) {
//       this.markAllFieldsAsTouched();
//       return;
//     }

//     // Check password match
//     if (!this.passwordsMatch()) {
//       this.errorMessage = 'Passwords do not match';
//       return;
//     }

//     // Check terms agreement
//     if (!this.agreeToTerms) {
//       this.errorMessage = 'Please agree to the Terms & Conditions';
//       return;
//     }

//     this.isLoading = true;

//     // Prepare registration data
//     const registerData: RegisterRequest = {
//       fullName: this.fullName,
//       email: this.email,
//       phoneNumber: this.phoneNumber.replace(/\D/g, ''), // Remove non-digits
//       password: this.password,
//       confirmPassword: this.confirmPassword,
//       roles: [] // Default role, adjust as needed
//     };

//     // Call AuthService register
//     this.authService.register(registerData).subscribe({
//       next: (response) => {
//         this.isLoading = false;
//         if (response.isSuccess) {
//           // Navigate to login or OTP verification based on backend requirements
//           this.router.navigate(['auth/login']);
//         } else {
//           this.errorMessage = response.message || 'Registration failed. Please try again.';
//         }
//       },
//       error: (error) => {
//         this.isLoading = false;
//         this.errorMessage = error.error?.message || 'An error occurred during registration.';
//         console.error('Registration error:', error);
//       }
//     });
//   }

//   // Password visibility toggles
//   togglePasswordVisibility(): void {
//     this.showPassword = !this.showPassword;
//   }

//   toggleConfirmPasswordVisibility(): void {
//     this.showConfirmPassword = !this.showConfirmPassword;
//   }

//   // Validation methods
//   passwordsMatch(): boolean {
//     return this.password === this.confirmPassword && this.password.length > 0;
//   }

//   isFormValid(): boolean {
//     return (
//       this.isFullNameValid() &&
//       this.isEmailValid() &&
//       this.isPhoneNumberValid() &&
//       this.isPasswordValid() &&
//       this.passwordsMatch() &&
//       this.agreeToTerms
//     );
//   }

//   isFullNameValid(): boolean {
//     return this.fullName.trim().length >= 2 && this.fullName.length <= 255;
//   }

//   isEmailValid(): boolean {
//     const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
//     return emailPattern.test(this.email) && this.email.length <= 255;
//   }

//   isPhoneNumberValid(): boolean {
//     const cleanedPhone = this.phoneNumber.replace(/\D/g, '');
//     const phonePattern = /^[1-9]\d{6,19}$/;
//     return phonePattern.test(cleanedPhone);
//   }

//   isPasswordValid(): boolean {
//     const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,100}$/;
//     return passwordPattern.test(this.password);
//   }

//   // Utility methods
//   private markAllFieldsAsTouched(): void {
//     const missingFields = [];
    
//     if (!this.isFullNameValid()) missingFields.push('Full Name');
//     if (!this.isEmailValid()) missingFields.push('Email');
//     if (!this.isPhoneNumberValid()) missingFields.push('Phone Number');
//     if (!this.isPasswordValid()) missingFields.push('Password');
//     if (!this.passwordsMatch()) missingFields.push('Password Confirmation');
//     if (!this.agreeToTerms) missingFields.push('Terms Agreement');

//     if (missingFields.length > 0) {
//       this.errorMessage = `Please check the following fields: ${missingFields.join(', ')}`;
//     }
//   }

//   // Password strength checker
//   getPasswordStrength(): string {
//     if (this.password.length === 0) return '';
//     if (!this.isPasswordValid()) return 'weak';
    
//     const lengthScore = this.password.length >= 12 ? 2 : this.password.length >= 8 ? 1 : 0;
//     const criteriaScore = [
//       /[A-Z]/.test(this.password),
//       /[a-z]/.test(this.password),
//       /\d/.test(this.password),
//       /[!@#$%^&*(),.?":{}|<>]/.test(this.password)
//     ].filter(Boolean).length;

//     const totalScore = lengthScore + criteriaScore;
//     if (totalScore >= 5) return 'strong';
//     if (totalScore >= 3) return 'medium';
//     return 'weak';
//   }

//   // Form reset method
//   resetForm(): void {
//     this.fullName = '';
//     this.email = '';
//     this.phoneNumber = '';
//     this.password = '';
//     this.confirmPassword = '';
//     this.agreeToTerms = false;
//     this.showPassword = false;
//     this.showConfirmPassword = false;
//     this.errorMessage = '';
//   }

//   // Phone number formatting
//   formatPhoneNumber(): void {
//     let phone = this.phoneNumber.replace(/\D/g, '');
    
//     if (phone.length >= 10) {
//       phone = phone.replace(/(\d{3})(\d{3})(\d{4})/, '($1) $2-$3');
//     } else if (phone.length >= 6) {
//       phone = phone.replace(/(\d{3})(\d{0,3})/, '($1) $2');
//     } else if (phone.length >= 3) {
//       phone = phone.replace(/(\d{3})/, '($1)');
//     }
    
//     this.phoneNumber = phone;
//   }

//   // Email domain suggestions
//   suggestEmailCorrection(): string[] {
//     const commonDomains = ['gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com'];
//     const emailParts = this.email.split('@');
    
//     if (emailParts.length !== 2) return [];
    
//     const domain = emailParts[1].toLowerCase();
//     const suggestions = commonDomains.filter(d => 
//       d.includes(domain) || domain.includes(d.split('.')[0])
//     );
    
//     return suggestions.slice(0, 3);
//   }

//   // Terms and Privacy navigation
//   onTermsClick(event: Event): void {
//     event.preventDefault();
//     this.router.navigate(['/terms']);
//   }

//   onPrivacyClick(event: Event): void {
//     event.preventDefault();
//     this.router.navigate(['/privacy']);
//   }
// }

import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest } from '../../../interfaces/auth/auth-request/register-request';
import { AuthResponse } from '../../../interfaces/auth/auth-response/auth-response';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  imports: [FormsModule, CommonModule, RouterModule],
  standalone: true
})
export class RegisterComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);

  // Form fields
  fullName: string = '';
  email: string = '';
  phoneNumber: string = '';
  password: string = '';
  confirmPassword: string = '';
  agreeToTerms: boolean = false;

  // UI state
  showPassword: boolean = false;
  showConfirmPassword: boolean = false;
  isLoading: boolean = false;
  errorMessage: string = '';
  loadingText: string = 'Signing up, please wait...';

  ngOnInit(): void {}

  onRegister(): void {
    // Clear previous error
    this.errorMessage = '';

    // Validate form
    if (!this.isFormValid()) {
      this.markAllFieldsAsTouched();
      return;
    }

    // Check password match
    if (!this.passwordsMatch()) {
      this.errorMessage = 'Passwords do not match';
      return;
    }

    // Check terms agreement
    if (!this.agreeToTerms) {
      this.errorMessage = 'Please agree to the Terms & Conditions';
      return;
    }

    this.isLoading = true;
    this.loadingText = 'Signing up, please wait...';

    // Prepare registration data
    const registerData: RegisterRequest = {
      fullName: this.fullName,
      email: this.email,
      phoneNumber: this.phoneNumber.replace(/\D/g, ''),
      password: this.password,
      confirmPassword: this.confirmPassword,
      roles: []
    };

   // Introduce a delay for the "Signing up..." message
    setTimeout(() => {
      this.authService.register(registerData).subscribe({
        next: (response: AuthResponse) => { 
          if (response.isSucceeded) { 
            this.loadingText = 'Account created successfully, redirecting to login in few seconds...'; 
            setTimeout(() => {
              this.isLoading = false;
              this.router.navigate(['/auth/login']);
            }, 3000);
          } else {
            this.isLoading = false;
            this.errorMessage = response.message || 'Registration failed. Please try again.';
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'An error occurred during registration.';
          console.error('Registration error:', error);
        }
      });
    }, 1000);
  }

  // Password visibility toggles
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  // Validation methods
  passwordsMatch(): boolean {
    return this.password === this.confirmPassword && this.password.length > 0;
  }

  isFormValid(): boolean {
    return (
      this.isFullNameValid() &&
      this.isEmailValid() &&
      this.isPhoneNumberValid() &&
      this.isPasswordValid() &&
      this.passwordsMatch() &&
      this.agreeToTerms
    );
  }

  isFullNameValid(): boolean {
    return this.fullName.trim().length >= 2 && this.fullName.length <= 255;
  }

  isEmailValid(): boolean {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailPattern.test(this.email) && this.email.length <= 255;
  }

  isPhoneNumberValid(): boolean {
    const cleanedPhone = this.phoneNumber.replace(/\D/g, '');
    const phonePattern = /^[1-9]\d{6,19}$/;
    return phonePattern.test(cleanedPhone);
  }

  isPasswordValid(): boolean {
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,100}$/;
    return passwordPattern.test(this.password);
  }

  // Utility methods
  private markAllFieldsAsTouched(): void {
    const missingFields = [];
    
    if (!this.isFullNameValid()) missingFields.push('Full Name');
    if (!this.isEmailValid()) missingFields.push('Email');
    if (!this.isPhoneNumberValid()) missingFields.push('Phone Number');
    if (!this.isPasswordValid()) missingFields.push('Password');
    if (!this.passwordsMatch()) missingFields.push('Password Confirmation');
    if (!this.agreeToTerms) missingFields.push('Terms Agreement');

    if (missingFields.length > 0) {
      this.errorMessage = `Please check the following fields: ${missingFields.join(', ')}`;
    }
  }

  // Password strength checker
  getPasswordStrength(): string {
    if (this.password.length === 0) return '';
    if (!this.isPasswordValid()) return 'weak';
    
    const lengthScore = this.password.length >= 12 ? 2 : this.password.length >= 8 ? 1 : 0;
    const criteriaScore = [
      /[A-Z]/.test(this.password),
      /[a-z]/.test(this.password),
      /\d/.test(this.password),
      /[!@#$%^&*(),.?":{}|<>]/.test(this.password)
    ].filter(Boolean).length;

    const totalScore = lengthScore + criteriaScore;
    if (totalScore >= 5) return 'strong';
    if (totalScore >= 3) return 'medium';
    return 'weak';
  }

  // Form reset method
  resetForm(): void {
    this.fullName = '';
    this.email = '';
    this.phoneNumber = '';
    this.password = '';
    this.confirmPassword = '';
    this.agreeToTerms = false;
    this.showPassword = false;
    this.showConfirmPassword = false;
    this.errorMessage = '';
    this.isLoading = false;
    this.loadingText = 'Signing up, please wait...';
  }

  // Phone number formatting
  formatPhoneNumber(): void {
    let phone = this.phoneNumber.replace(/\D/g, '');
    
    if (phone.length >= 10) {
      phone = phone.replace(/(\d{3})(\d{3})(\d{4})/, '($1) $2-$3');
    } else if (phone.length >= 6) {
      phone = phone.replace(/(\d{3})(\d{0,3})/, '($1) $2');
    } else if (phone.length >= 3) {
      phone = phone.replace(/(\d{3})/, '($1)');
    }
    
    this.phoneNumber = phone;
  }

  // Email domain suggestions
  suggestEmailCorrection(): string[] {
    const commonDomains = ['gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com'];
    const emailParts = this.email.split('@');
    
    if (emailParts.length !== 2) return [];
    
    const domain = emailParts[1].toLowerCase();
    const suggestions = commonDomains.filter(d => 
      d.includes(domain) || domain.includes(d.split('.')[0])
    );
    
    return suggestions.slice(0, 3);
  }

  // Terms and Privacy navigation
  onTermsClick(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/terms']);
  }

  onPrivacyClick(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/privacy']);
  }
}