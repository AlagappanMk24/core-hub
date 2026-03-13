// select-company.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CompanyService } from '../../services/company/company.service';
import { AuthService } from '../../services/auth/auth.service';
import { jwtDecode } from 'jwt-decode';
import { CompanyRequestService } from '../../services/company-request/company-request.service';

interface Company {
  id: number;
  name: string;
}

@Component({
  selector: 'app-select-company',
  templateUrl: './select-company.component.html',
  styleUrls: ['./select-company.component.css'],
  imports: [CommonModule, FormsModule, RouterModule],
  standalone: true,
})
export class SelectCompanyComponent implements OnInit {
  companyService = inject(CompanyService);
  authService = inject(AuthService);
  companyRequestService = inject(CompanyRequestService);
  router = inject(Router);

  companies: Company[] = [];
  selectedCompanyId: number = 0;
  errorMessage: string = '';
  isLoading: boolean = false;
  loadingText: string = 'Saving selection...';

  // Request company properties
  showRequestForm: boolean = false;
  requestFullName: string = '';
  requestCompanyName: string = '';
  requestEmail: string = '';
  requestSuccess: string = '';
  showSuccessMessage: boolean = false;

  ngOnInit(): void {
    this.loadCompanies();
    this.loadUserEmail();
  }

  loadUserEmail(): void {
    const user = this.authService.getUserDetail();
    if (user?.email) {
      this.requestEmail = user.email;
    }
  }

  // Add toggle methods
  toggleToRequestForm(): void {
    this.showRequestForm = true;
    this.errorMessage = '';
    this.showSuccessMessage = false; // Hide success message when toggling
  }
  toggleToSelectionForm(): void {
    this.showRequestForm = false;
    this.errorMessage = '';
  }

  loadCompanies(): void {
    this.companyService.getCompanies().subscribe({
      next: (companies) => {
        this.companies = companies;
        if (companies.length > 0) {
          this.selectedCompanyId = companies[0].id;
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to load companies. Please try again.';
        console.error('Error loading companies:', error);
      },
    });
  }

  saveCompany(): void {
    if (!this.selectedCompanyId) {
      this.errorMessage = 'Please select a company';
      return;
    }

    this.isLoading = true;
    this.authService.updateCompany(this.selectedCompanyId).subscribe({
      next: (response) => {
        if (response.token) {
          localStorage.setItem('authToken', response.token);

          const decoded: any = jwtDecode(response.token);
          const roles = this.extractRoles(decoded);

          this.loadingText = 'Company selected, redirecting...';

          setTimeout(() => {
            this.isLoading = false;

            if (
              roles.includes('Customer') &&
              !roles.includes('Admin') &&
              !roles.includes('User')
            ) {
              this.router.navigate(['/customer-dashboard']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          }, 1500);
        } else {
          this.isLoading = false;
          this.errorMessage = 'Failed to update company. Please try again.';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage =
          error.error?.message || 'Failed to save company selection';
        console.error('Error saving company:', error);
      },
    });
  }

  requestCompany(): void {
    if (
      !this.requestFullName ||
      !this.requestCompanyName ||
      !this.requestEmail
    ) {
      this.errorMessage = 'Please fill in all fields';
      return;
    }

    this.isLoading = true;
    this.loadingText = 'Sending request...';

    this.companyRequestService
      .createRequest({
        fullName: this.requestFullName,
        email: this.requestEmail,
        companyName: this.requestCompanyName,
      })
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          this.requestSuccess = response.message || 'Your request has been submitted successfully!';
        this.showSuccessMessage = true; // Show success message
        this.showRequestForm = false; // Go back to selection view

          // Clear form
          this.requestFullName = '';
          this.requestCompanyName = '';

             // Auto-hide success message after 5 seconds
        setTimeout(() => {
          this.showSuccessMessage = false;
        }, 5000);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage =
            error.error?.message || 'Failed to send request. Please try again.';
          console.error('Error requesting company:', error);
        },
      });
  }

  private extractRoles(decoded: any): string[] {
    const roleClaim =
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    if (decoded[roleClaim]) {
      return Array.isArray(decoded[roleClaim])
        ? decoded[roleClaim]
        : [decoded[roleClaim]];
    }
    return [];
  }
}
