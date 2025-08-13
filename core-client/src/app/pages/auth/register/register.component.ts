import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';
import { RegisterRequest } from '../../../interfaces/auth/auth-request/register-request';
import { AuthResponse } from '../../../interfaces/auth/auth-response/auth-response';
import { CompanyService } from '../../../services/company/company.service';

interface Company {
  id: number;
  name: string;
}

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  imports: [FormsModule, CommonModule, RouterModule],
  standalone: true,
})
export class RegisterComponent implements OnInit {
  @ViewChild('registerForm') registerForm!: NgForm;
  @ViewChild('formSection') formSection!: ElementRef;

  authService = inject(AuthService);
  companyService = inject(CompanyService);
  router = inject(Router);

  fullName: string = '';
  email: string = '';
  phoneNumber: string = '';
  password: string = '';
  confirmPassword: string = '';
  agreeToTerms: boolean = false;
  companyId: number = 0;
  companies: Company[] = [];
  showCompanyDropdown: boolean = false;
  companySearchTerm: string = '';
  filteredCompanies: Company[] = [];
  selectedCompanyDisplayName: string = '';

  showPassword: boolean = false;
  showConfirmPassword: boolean = false;
  isLoading: boolean = false;
  errorMessage: string = '';
  loadingText: string = 'Signing up, please wait...';

  ngOnInit(): void {
    this.loadCompanies();
  }

  loadCompanies(): void {
    this.companyService.getCompanies().subscribe({
      next: (companies) => {
        this.companies = companies;
        this.filterCompanies();
        if (companies.length === 0) {
          this.errorMessage = 'No companies are currently available. Please request a new company or contact the administrator.';
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to load companies. Please try again or contact the administrator.';
        console.error('Error loading companies:', error);
      },
    });
  }

  onRegister(): void {
    this.errorMessage = '';
    if (!this.registerForm.valid || !this.passwordsMatch()) {
      this.markAllFieldsAsTouched();
      this.scrollToError();
      return;
    }

    if (!this.agreeToTerms) {
      this.errorMessage = 'Please agree to the Terms & Conditions';
      this.scrollToError();
      return;
    }

    if (!this.companyId) {
      this.errorMessage = 'Please select a company. If your company is not listed, please request it.';
      this.scrollToError();
      return;
    }

    this.isLoading = true;
    this.loadingText = 'Signing up, please wait...';

    const registerData: RegisterRequest = {
      fullName: this.fullName,
      email: this.email,
      phoneNumber: this.phoneNumber.replace(/\D/g, ''),
      password: this.password,
      confirmPassword: this.confirmPassword,
      roles: [],
      companyId: this.companyId,
    };

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
            this.scrollToError();
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'An error occurred during registration.';
          this.scrollToError();
          console.error('Registration error:', error);
        },
      });
    }, 1000);
  }

  requestCompany(): void {
    if (!this.fullName || !this.email || !this.companySearchTerm) {
      this.errorMessage = 'Please provide your full name, email, and the desired company name to request.';
      this.scrollToError();
      return;
    }

    this.isLoading = true;
    this.loadingText = 'Sending request, please wait...';

    this.authService.requestCompany({
      fullName: this.fullName,
      email: this.email,
      companyName: this.companySearchTerm,
    }).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        this.errorMessage = response.message || 'Request sent successfully. The administrator will review it soon.';
        this.showCompanyDropdown = false;
        this.scrollToError();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to send request. Please try again or contact support.';
        this.scrollToError();
        console.error('Request error:', error);
      },
    });
  }

  toggleCompanyDropdown(): void {
    if (this.companies.length === 0) {
      this.errorMessage = 'No companies are available. Please request a new company or contact the administrator.';
      this.scrollToError();
      return;
    }
    this.showCompanyDropdown = !this.showCompanyDropdown;
    this.companySearchTerm = '';
    this.filterCompanies();
  }

  selectCompany(company: Company): void {
    this.companyId = company.id;
    this.selectedCompanyDisplayName = company.name;
    this.showCompanyDropdown = false;
    this.errorMessage = ''; // Clear error when a company is selected
  }

  filterCompanies(): void {
    if (!this.companySearchTerm) {
      this.filteredCompanies = [...this.companies];
    } else {
      this.filteredCompanies = this.companies.filter(company =>
        company.name.toLowerCase().includes(this.companySearchTerm.toLowerCase())
      );
    }
    if (this.filteredCompanies.length === 0 && this.companySearchTerm) {
      this.errorMessage = 'No matching companies found. Please <a href="#" (click)="requestCompany()">request</a> your company or contact the administrator.';
    } else {
      this.errorMessage = this.companies.length === 0 ? 'No companies are available. Please request a new company or contact the administrator.' : '';
    }
  }

  togglePasswordVisibility(): void { this.showPassword = !this.showPassword; }
  toggleConfirmPasswordVisibility(): void { this.showConfirmPassword = !this.showConfirmPassword; }

  passwordsMatch(): boolean { return this.password === this.confirmPassword && this.password.length > 0; }
  isFormValid(): boolean {
    return this.isFullNameValid() && this.isEmailValid() && this.isPhoneNumberValid() &&
           this.isPasswordValid() && this.passwordsMatch() && this.agreeToTerms;
  }
  isFullNameValid(): boolean { return this.fullName.trim().length >= 2 && this.fullName.length <= 255; }
  isEmailValid(): boolean { const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; return emailPattern.test(this.email) && this.email.length <= 255; }
  isPhoneNumberValid(): boolean { const cleanedPhone = this.phoneNumber.replace(/\D/g, ''); const phonePattern = /^[1-9]\d{6,19}$/; return phonePattern.test(cleanedPhone); }
  isPasswordValid(): boolean { const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,100}$/; return passwordPattern.test(this.password); }

  markAllFieldsAsTouched(): void {
    this.registerForm.control.markAllAsTouched();
    const missingFields = [];
    if (!this.isFullNameValid()) missingFields.push('Full Name');
    if (!this.isEmailValid()) missingFields.push('Email');
    if (!this.isPhoneNumberValid()) missingFields.push('Phone Number');
    if (!this.isPasswordValid()) missingFields.push('Password');
    if (!this.passwordsMatch()) missingFields.push('Password Confirmation');
    if (!this.agreeToTerms) missingFields.push('Terms Agreement');
    if (!this.companyId) missingFields.push('Company');
    if (missingFields.length > 0) this.errorMessage = `Please check the following fields: ${missingFields.join(', ')}`;
  }

  getPasswordStrength(): string {
    if (this.password.length === 0) return '';
    if (!this.isPasswordValid()) return 'weak';
    const lengthScore = this.password.length >= 12 ? 2 : this.password.length >= 8 ? 1 : 0;
    const criteriaScore = [/[A-Z]/.test(this.password), /[a-z]/.test(this.password), /\d/.test(this.password), /[!@#$%^&*(),.?":{}|<>]/.test(this.password)].filter(Boolean).length;
    const totalScore = lengthScore + criteriaScore;
    return totalScore >= 5 ? 'strong' : totalScore >= 3 ? 'medium' : 'weak';
  }

  resetForm(): void {
    this.fullName = ''; this.email = ''; this.phoneNumber = ''; this.password = ''; this.confirmPassword = '';
    this.agreeToTerms = false; this.companyId = 0; this.showPassword = false; this.showConfirmPassword = false;
    this.errorMessage = ''; this.isLoading = false; this.loadingText = 'Signing up, please wait...';
    this.registerForm.resetForm();
  }

  formatPhoneNumber(): void {
    let phone = this.phoneNumber.replace(/\D/g, '');
    if (phone.length >= 10) phone = phone.replace(/(\d{3})(\d{3})(\d{4})/, '($1) $2-$3');
    else if (phone.length >= 6) phone = phone.replace(/(\d{3})(\d{0,3})/, '($1) $2');
    else if (phone.length >= 3) phone = phone.replace(/(\d{3})/, '($1)');
    this.phoneNumber = phone;
  }

  suggestEmailCorrection(): string[] {
    const commonDomains = ['gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com'];
    const emailParts = this.email.split('@');
    if (emailParts.length !== 2) return [];
    const domain = emailParts[1].toLowerCase();
    return commonDomains.filter(d => d.includes(domain) || domain.includes(d.split('.')[0])).slice(0, 3);
  }

  onTermsClick(event: Event): void { event.preventDefault(); this.router.navigate(['/terms']); }
  onPrivacyClick(event: Event): void { event.preventDefault(); this.router.navigate(['/privacy']); }

  private scrollToError(): void {
    if (this.formSection && this.errorMessage) {
      this.formSection.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      setTimeout(() => {
        const errorElement = this.formSection.nativeElement.querySelector('.error-message');
        if (errorElement) errorElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
      }, 100);
    }
  }
}